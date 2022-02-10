using Microsoft.Extensions.Logging;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        private static readonly ILogger Logger =new LoggerFactory().CreateLogger(typeof(SbUtil).Name);
    }
}