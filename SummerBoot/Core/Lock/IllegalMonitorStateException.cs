using System;

namespace SummerBoot.Core.Lock
{
    public class IllegalMonitorStateException:Exception
    {
        public IllegalMonitorStateException():base("IllegalMonitorState")
        {
    
        }
        public IllegalMonitorStateException(string message) : base(message)
        {
        }

        public IllegalMonitorStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}