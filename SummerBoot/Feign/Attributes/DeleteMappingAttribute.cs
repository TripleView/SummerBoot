namespace SummerBoot.Feign.Attributes
{
    public class DeleteMappingAttribute : HttpMappingAttribute
    {
        public DeleteMappingAttribute(string value) : base(value)
        {
        }
    }
}