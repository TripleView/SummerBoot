using System;

namespace SummerBoot.Test.DbExecute.Common;

public class TestUtils
{
    public static bool CompareTwoDate(DateTime date1, DateTime date2)
    {
        return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day &&
               date1.Hour == date2.Hour && date1.Minute == date2.Minute && Math.Abs(date1.Second - date2.Second) <= 1;
    }
}