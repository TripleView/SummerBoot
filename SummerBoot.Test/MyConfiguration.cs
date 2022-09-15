using System.IO;
using Microsoft.Extensions.Configuration;

namespace SummerBoot.Test
{
    public class MyConfiguration
    {
        static readonly string CONFIG_FILE = "app.json";  // 配置文件地址

        public static IConfiguration Configs;

        static MyConfiguration()
        {
            if (Configs == null)
            {
                var build = new ConfigurationBuilder();
                build.SetBasePath(Directory.GetCurrentDirectory());  // 获取当前程序执行目录
                build.AddJsonFile(CONFIG_FILE, true, true);
                Configs = build.Build();
            }
        }
        /// <summary>
        /// 获取配置文件的配置节点
        /// </summary>
        /// <param name="configKey">多个节点可以用英文.号隔开</param>
        /// <returns></returns>
        public static string GetConfiguration(string configKey)
        {
           

            if (configKey.Contains("."))
            {
                IConfigurationSection child = null;
                foreach (string key in configKey.Split('.'))
                {
                    if (child == null)
                        child = Configs.GetSection(key);
                    else
                        child = child.GetSection(key);
                }

                return (child == null) ? "" : child.Value;
            }
            else
            {
                return Configs.GetSection(configKey).Value;
            }
        }
    }
}