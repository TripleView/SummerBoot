using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SummerBoot.Repository
{
    public interface IRepositorySqlPath
    {
        void AddPath(string modelName,string sql);
        string GetPath(string modelName);
    }
}
