using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SummerBoot.Performance.Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //注意：在mysql数据库上执行"set global local_infile=1"开启批量上传，才能批量快速插入

            var performance = new Performance();
            await performance.TestFreeSqlBatchInsert();
            await performance.TestSummerbootBatchInsert();
            await performance.TestFreeSqlSelect();
            await performance.TestSummerBootSelect();
            await performance.TestFreeSqlDelete();
            await performance.TestSummerBootDelete();
            //BenchmarkRunner.Run<Performance>(new DebugBuildConfig());
            Console.WriteLine("Hello, World!");
        }
    }
}