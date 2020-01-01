namespace SummerBoot.Core
{
    public class ValueWrapper : IValueWrapper
    {
        private readonly object value;

        public ValueWrapper(object value)
        {
            this.value = value;
        }
        public object Get()
        {
            return this.value;
        }
    }
}