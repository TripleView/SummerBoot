using Newtonsoft.Json;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static T DeepClone<T>(this T oldObj) where T : class
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(oldObj));
        }
    }
}