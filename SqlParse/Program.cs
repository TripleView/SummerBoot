using SqlParser.Dialect;
using System;

namespace SqlParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var sql = @"SELECT DISTINCT `p0`.`Name` As `Key`, SUM(`p0`.`Age`) As `Count` FROM `Person` As `p0` GROUP BY `p0`.`Name` LIMIT @y0,@y1";


            var result = new MysqlParser().Parse(sql);
            foreach (var sqlToken in result)
            {
                Console.WriteLine(sqlToken.ToString());
            }

            Console.Read();
        }
    }
}
