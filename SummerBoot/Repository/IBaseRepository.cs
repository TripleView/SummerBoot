﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseParser.ExpressionParser;

namespace SummerBoot.Repository
{
    public interface IBaseRepository<T>  where T : class
    {
        T Get(dynamic id);
        List<T> GetAll();
        void Update(T t);
        void Update(List<T> list);

        void Delete(T t);
        void Delete(List<T> list);

        T Insert(T t);
        List<T> Insert(List<T> list);
    }
}