namespace SummerBoot.Repository
{
    /// <summary>
    /// where更新条件
    /// </summary>
    public class WhereItem<T>
    {
        public WhereItem(bool active, T value)
        {
            Active = active;
            Value = value;
        }
        /// <summary>
        /// 该条件是否激活
        /// </summary>
        public bool Active { get; set; } = false;

        public T Value { get; set; }

        
    }

    public class WhereBuilder
    {
        public static WhereItem<T> Of<T>(bool active, T value)
        {
            return new WhereItem<T>(active, value);
        }

        /// <summary>
        /// 空条件
        /// </summary>
        /// <returns></returns>
        public static WhereItem<T> Empty<T>()
        {
            return new WhereItem<T>(false, default);
        }

        public static WhereItem<T> HasValue<T>(T param)
        {
            return new WhereItem<T>(true, param);
        }
    }
}