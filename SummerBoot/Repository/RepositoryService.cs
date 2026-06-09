using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SummerBoot.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public class RepositoryService : RepositoryAspectSupport
    {
        public RepositoryService(IUnitOfWork uow) : base(uow)
        {
        }

        public async Task ExecuteNoReturnAsync(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            await base.BaseExecuteNoReturnAsync(method, args.ToArray(), serviceProvider);
        }

        public void ExecuteNoReturn(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            base.BaseExecuteNoReturn(method, args.ToArray(), serviceProvider);
        }

        public async Task<int> ExecuteReturnCountAsync(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            return await base.BaseExecuteReturnCountAsync(method, args.ToArray(), serviceProvider);
        }

        public int ExecuteReturnCount(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            return base.BaseExecuteReturnCount(method, args.ToArray(), serviceProvider);
        }

        public async Task<T> ExecuteAsync<T, TBaseType>(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            return await base.BaseExecuteAsync<T, TBaseType>(method, args.ToArray(), serviceProvider);
        }

        public T Execute<T, TBaseType>(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            return base.BaseExecute<T, TBaseType>(method, args.ToArray(), serviceProvider);
        }

        public async Task<Page<T>> PageExecuteAsync<T>(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            return await base.PageBaseExecuteAsync<T>(method, args.ToArray(), serviceProvider);
        }

        public Page<T> PageExecute<T>(List<object> originArgs, MethodInfo method, IServiceProvider serviceProvider)
        {
            var args = new List<object>();
            originArgs.ForEach(it => args.Add(it));

            var interfaceType = method.DeclaringType;
            if (interfaceType == null) throw new Exception(nameof(interfaceType));

            return base.PageBaseExecute<T>(method, args.ToArray(), serviceProvider);
        }

    }
}