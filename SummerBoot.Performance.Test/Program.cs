using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SummerBoot.Performance.Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new Performance().TestFreeSqlBatchInsert();
            await new Performance().TestSummerbootBatchInsert();
            //BenchmarkRunner.Run<Performance>(new DebugBuildConfig());
            Console.WriteLine("Hello, World!");
        }
    }
}