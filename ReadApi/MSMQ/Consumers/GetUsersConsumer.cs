using Contracts.Commands;
using MassTransit;
using ReadApi.Repository;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class GetUsersConsumer : IConsumer<IGetUsers>
    {
        private IAccountRepository _accountRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountRepository"></param>
        public GetUsersConsumer(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IGetUsers> context)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console(theme: ConsoleTheme.None)
               .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);

            var data = context.Message;
            var result = _accountRepository.GetUsers(data.Ids);
            await context.RespondAsync<IUsersGetted>(new { Users = result });
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
