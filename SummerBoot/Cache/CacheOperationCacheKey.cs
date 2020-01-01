using System;
using System.Reflection;

namespace SummerBoot.Cache
{
    public sealed class CacheOperationCacheKey : IComparable<CacheOperationCacheKey>
    {

        private readonly CacheOperation _cacheOperation;

        private readonly AttributeKey _attributeKey;

        public CacheOperationCacheKey(CacheOperation cacheOperation, MethodInfo method, Type targetType)
        {
            this._cacheOperation = cacheOperation;
            this._attributeKey=new AttributeKey(method,targetType);
        }

        public int CompareTo(CacheOperationCacheKey other)
        {
            int result = this._cacheOperation.Name.CompareTo(other._cacheOperation.Name);
            if (result == 0)
            {
                result = this._attributeKey.CompareTo(other._attributeKey);
            }
            return result;
        }

        public override bool Equals(object obj)
        {

            if (this == obj) return true;
            if (!(obj is CacheOperationCacheKey)) return false;
            CacheOperationCacheKey otherKey = (CacheOperationCacheKey)obj;

            return _cacheOperation.Equals(otherKey._cacheOperation) && _attributeKey.Equals(otherKey._attributeKey);
        }

        public override int GetHashCode()
        {
            return _cacheOperation.GetHashCode() * 31 + _attributeKey.GetHashCode();
        }

        public override string ToString()
        {
            return _cacheOperation.ToString()+" on "+_attributeKey.ToString();
        }
    }


    public sealed class AttributeKey : IComparable<AttributeKey>
    {
        private MethodInfo method;
        private Type type;

        public AttributeKey(MethodInfo method, Type type)
        {
            this.method = method;
            this.type = type;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (!(obj is AttributeKey)) return false;
            AttributeKey otherKey = (AttributeKey)obj;

            return method.Equals(otherKey.method)&&object.ReferenceEquals(this.type,otherKey.type);
        }

        public override int GetHashCode()
        {
            return this.method.GetHashCode() + this.type.GetHashCode();
        }

        public override string ToString()
        {
            return this.method.ToString() + this.type.ToString();
        }

        public int CompareTo(AttributeKey other)
        {
            int result = this.method.ToString().CompareTo(other.method.ToString());
            if (result == 0 && this.type != null)
            {
                if (other.type == null)
                {
                    return 1;
                }
                result = this.type.Name.CompareTo(other.type.Name);
            }
            return result;
        }
    }
}