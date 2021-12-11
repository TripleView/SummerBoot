using System;

namespace SummerBoot.Cache
{
    public interface IRedisCacheWriter
    {
        /// <summary>
        /// InternalGet the binary value representation from Redis stored for the given key.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        byte[] Get(string name,byte [] key);

        /// <summary>
        /// Write the given key/value pair to Redis an set the expiration time if defined.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        void Put(string name,byte[] key, byte[] value,TimeSpan ttl);

        /// <summary>
        /// Remove the given key from Redis.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        void Remove(string name,byte[] key);

        /// <summary>
        /// Remove all keys following the given pattern.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pattern"></param>
        void Clean(string name, byte[] pattern);

        /// <summary>
        /// Write the given value to Redis if the key does not already exist.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        byte[] PutIfAbsent(string name, byte[] key, byte[] value, TimeSpan ttl);
    }
}