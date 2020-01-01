using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicExpresso;

namespace SummerBoot.Cache
{
    public class CacheOperationExpressionEvaluator
    {
        public static readonly object NO_RESULT=new object();
        //public static readonly object RESULT_UNAVAILABLE = new object();
        public static readonly string RESULT_VARIABLE = "result";

        public bool Condition(string expression, CacheAspectSupport.CacheOperationContext context, object lockObj)
        {
            var interpreter = new Interpreter();

            var parameters = new List<Parameter>
            {
                new Parameter("methodName", typeof(string), context.GetMethod().Name),
                new Parameter("method", typeof(MethodInfo), context.GetMethod()),
                new Parameter("target", context.Metadata.TargeType, context.Target),
                new Parameter("args", typeof(object[]), context.Args)
            };

            if (lockObj !=NO_RESULT)
            {
                parameters.Add(new Parameter(RESULT_VARIABLE, lockObj.GetType(), lockObj));
            }

            var result = interpreter.Eval<bool>(expression, parameters.ToArray());

            return result;
        }

        public bool Unless(string expression, CacheAspectSupport.CacheOperationContext context, object lockObj)
        {
            var interpreter = new Interpreter();

            var parameters = new List<Parameter>
            {
                new Parameter("methodName", typeof(string), context.GetMethod().Name),
                new Parameter("method", typeof(MethodInfo), context.GetMethod()),
                new Parameter("target", context.Metadata.TargeType, context.Target),
                new Parameter("args", typeof(object[]), context.Args)
            };

            if (lockObj != NO_RESULT)
            {
                parameters.Add(new Parameter(RESULT_VARIABLE, lockObj.GetType(), lockObj));
            }

            var result = interpreter.Eval<bool>(expression, parameters.ToArray());

            return result;
        }
    }
}