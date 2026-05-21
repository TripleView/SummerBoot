using System;
using System.Collections.Generic;
using System.Linq;
using SummerBoot.Core;
using Xunit;

namespace SummerBoot.Test.DbExecute.Common;

public class TestUtils
{
    public static bool CompareTwoDate(DateTime date1, DateTime date2)
    {
        return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day &&
               date1.Hour == date2.Hour && date1.Minute == date2.Minute && Math.Abs(date1.Second - date2.Second) <= 1;
    }

    /// <summary>
    /// 对比2个实体类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="m1"></param>
    /// <param name="m2"></param>
    /// <param name="ignoreColumnNames"></param>
    public static void CompareTwoModel<T>(T m1, T m2, List<string> ignoreColumnNames = null)
    {
        var table = SbUtil.GetTableInfo(typeof(T));
        var columnNames = table.Columns.Select(x => x.Property.Name).ToList();
        foreach (var columnName in columnNames)
        {
            if (ignoreColumnNames?.Contains(columnName) == true)
            {
                continue;
            }
            var m1Value = m1.GetPropertyValue(columnName);
            var m2Value = m2.GetPropertyValue(columnName);
            if (m1Value is DateTime d1 && m2Value is DateTime d2)
            {
                TestUtils.CompareTwoDate(d1, d2);
            }
            else
            {
                Assert.Equal(m1Value, m2Value);
            }

        }
    }
}