namespace SummerBoot.Feign.Attributes
{
    public class PutMappingAttribute : HttpMappingAttribute
    {
        public PutMappingAttribute(string value) : base(value)
        {
        }
    }
}