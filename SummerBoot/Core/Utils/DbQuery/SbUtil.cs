using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;
using System.Threading;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static async Task<int> ExecuteNonQueryAsync(this IDbCommand cmd, CancellationToken cancel)
        {
            if (cmd is DbCommand dbCommand)
            {
                var result = await dbCommand.ExecuteNonQueryAsync(cancel);
                return result;
            }
            else
            {
                throw new InvalidOperationException("Async operations require use of a DbCommand");
            }
        }
    }
}