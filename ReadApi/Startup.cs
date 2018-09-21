using System;
using System.IO;
using System.Linq;
using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReadApi.Data;
using ReadApi.Repository;
using Serilog;
using Serilog.Context;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.DataProtection;
using Contracts.Models;
using MassTransit;
using MassTransit.Util;
using Consumers;

namespace ReadApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri($"http://{Environment.GetEnvironmentVariable("ES_HOST")}:{Environment.GetEnvironmentVariable("ES_PORT")}/"))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = "logstash-api-query-{0:yyyy}"
                })
            .CreateLogger();
        }

        /// <summary>
        /// 
        /// </summary>
        public IContainer ApplicationContainer { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = $"{Environment.GetEnvironmentVariable("IS_SERVER")}";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api";
                    options.ApiSecret = "secret";
                });

            services.AddDataProtection()
                .SetApplicationName("api-read")
                .PersistKeysToFileSystem(new DirectoryInfo(@"/var/dpkeys/"));

            services.AddIdentityWithMongoStoresUsingCustomTypes<ApplicationUser, ApplicationUserRole>($"mongodb://{Environment.GetEnvironmentVariable("MONGODB_USERNAME")}:{Environment.GetEnvironmentVariable("MONGODB_PASSWORD")}@{Environment.GetEnvironmentVariable("USER_MONGODB_HOST")}:{Environment.GetEnvironmentVariable("USER_MONGODB_PORT")}/{Environment.GetEnvironmentVariable("USER_MONGODB_DATABASE_NAME")}")
                    .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 5;
            });

            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = $"{Environment.GetEnvironmentVariable("REDIS_HOST")}:{Environment.GetEnvironmentVariable("REDIS_PORT")}";
            });

            services.Configure<ElasticSearchSettings>(options =>
            {
                options.Host = Environment.GetEnvironmentVariable("ES_HOST");
                options.Port = Environment.GetEnvironmentVariable("ES_PORT");
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "ReadApi", Version = "v1" });
                var filePath = Path.Combine(AppContext.BaseDirectory, "ReadApi.xml");
                c.IncludeXmlComments(filePath);
            });

            var builder = new ContainerBuilder();
            builder.RegisterType<CompanyRepository>().As<ICompanyRepository>();
            builder.RegisterType<LeadRepository>().As<ILeadRepository>();
            builder.RegisterType<MigrateRepository>().As<IMigrateRepository>();
            builder.RegisterType<AllRepository>().As<IAllRepository>();
            builder.RegisterType<CommonDataRepository>().As<ICommonDataRepository>();
            builder.RegisterType<AccountRepository>().As<IAccountRepository>();
            builder.RegisterType<ChatLeadRepository>().As<IChatLeadRepository>();
            builder.RegisterType<ContactLeadRepository>().As<IContactLeadRepository>();
            builder.RegisterType<ActivityHistoryLeadRepository>().As<IActivityHistoryLeadRepository>();
            builder.RegisterType<ReportRepository>().As<IReportRepository>();
            builder.RegisterType<ProductCategoryRepository>().As<IProductCategoryRepository>();
            builder.RegisterType<ProductRepository>().As<IProductRepository>();
            builder.RegisterType<AccessRightRepository>().As<IAccessRightRepository>();
            builder.RegisterType<TeamRepository>().As<ITeamRepository>();
            builder.RegisterType<TeamUsersRepository>().As<ITeamUsersRepository>();

            builder.RegisterType<ApplicationDbContext>().WithParameter("connectionString", $"mongodb://{Environment.GetEnvironmentVariable("MONGODB_USERNAME")}:{Environment.GetEnvironmentVariable("MONGODB_PASSWORD")}@{Environment.GetEnvironmentVariable("COMPANY_MONGODB_HOST")}:{Environment.GetEnvironmentVariable("COMPANY_MONGODB_PORT")}")
                   .WithParameter("database", $"{Environment.GetEnvironmentVariable("COMPANY_MONGODB_DATABASE_NAME")}");

            var timeout = TimeSpan.FromSeconds(30);

            services.AddScoped<GetUsersConsumer>();
            services.AddScoped<CheckAccessRightConsumer>();

            builder.Register(c => new MessageRequestClient<ICheckAccessRight, CheckAccessRightResponse>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/check_access_right"), timeout))
                .As<IRequestClient<ICheckAccessRight, CheckAccessRightResponse>>()
                .SingleInstance();

            builder.Register(context =>
            {
                return Bus.Factory.CreateUsingRabbitMq(sbc =>
                {
                    var host = sbc.Host(new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/"), h =>
                    {
                        h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"));
                        h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"));
                    });
                    sbc.ReceiveEndpoint(host, "get_users", ep =>
                    {
                        ep.Consumer<GetUsersConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "check_access_right", ep =>
                    {
                        ep.Consumer<CheckAccessRightConsumer>(context);
                    });
                });
            })
            .As<IBus>()
            .As<IBusControl>()
            .As<IPublishEndpoint>()
            .SingleInstance();
            //end mass transit endpoint

            builder.Populate(services);
            ApplicationContainer = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(ApplicationContainer);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="appLifetime"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReadApi V1");
                });
            }
            loggerFactory.AddSerilog();

            IPHostEntry local = Dns.GetHostEntry(Environment.GetEnvironmentVariable("LOADBALANCER"));
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
                KnownProxies = { local.AddressList[0] }
            });
            app.Use(async (ctx, next) =>
            {
                using (LogContext.PushProperty("IPAddress", ctx.Connection.RemoteIpAddress))
                using (LogContext.PushProperty("UserName", ctx.User?.Claims?.FirstOrDefault(_ => _.Type == "userName")?.Value))
                {
                    await next();
                }
            });
            app.UseCors("default");
            app.UseAuthentication();
            app.UseMvc();
            //resolve the bus from the container
            var bus = ApplicationContainer.Resolve<IBusControl>();
            //start the bus
            var busHandle = TaskUtil.Await(() => bus.StartAsync());
            appLifetime.ApplicationStopped.Register(() => { busHandle.Stop(); ApplicationContainer.Dispose(); });
        }
    }
}
