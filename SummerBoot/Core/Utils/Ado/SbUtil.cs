using Microsoft.Extensions.Logging;
using System;
using System.Data;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        public static void LogAndClose(this IDbConnection dbConnection)
        {
            if (dbConnection == null) return;
            try
            {
                dbConnection.Close();
            }
            catch (Exception e)
            {
               Logger.LogError("close connection error+"+e.Message);
            }
        }

        public static bool IsClose(this IDbConnection dbConnection)
        {
            if (dbConnection == null) return true;
            return dbConnection.State == ConnectionState.Closed;
        }
    }
}