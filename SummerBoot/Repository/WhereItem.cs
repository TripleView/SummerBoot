namespace SummerBoot.Repository
{
    /// <summary>
    /// where更新条件
    /// </summary>
    public class WhereItem
    {
        public WhereItem(bool active, object value)
        {
            Active = active;
            Value = value;
        }
        /// <summary>
        /// 该条件是否激活
        /// </summary>
        public bool Active { get; set; } = false;

        public object Value { get; set; }

        public static WhereItem Of(bool active, object value)
        {
            return new WhereItem(active, value);
        }

        /// <summary>
        /// 未激活的条件
        /// </summary>
        /// <returns></returns>
        public static WhereItem OfInactivated()
        {
            return new WhereItem(false, null);
        }
    }
}