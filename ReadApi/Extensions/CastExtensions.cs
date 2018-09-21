using Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReadApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class CastExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        public static T Cast<T>(this Object myobj)
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);
            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
               .ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                value = myobj.GetType().GetProperty(memberInfo.Name)?.GetValue(myobj, null);

                propertyInfo.SetValue(x, value, null);
            }
            return (T)x;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static List<string> Array(this List<RolesListSort> model, List<string> keys)
        {
            var data = new List<string>();
            foreach (var item in model)
            {
                foreach (var item1 in item.Value)
                {
                    if (keys.Contains(item1.Key))
                    {
                        data = data.Union(item1.Value).ToList();
                    }
                }
            }
            return data;
        }
    }
}
