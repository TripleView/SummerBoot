using System;

namespace SummerBoot.Core
{
    public static class SbAssert
    {
        /// <summary>
        /// 类非空报错
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="msg">报错提示信息</param>
        public static void NotNull(object obj, string msg)
        {
            //|| obj.GetType().IsClass
            if (obj == null )
            {
                throw new Exception(msg);
            }
            //var type=obj.GetType();
            //if (type.IsClass && obj == null)
            //{

            //}
        }
    }
}