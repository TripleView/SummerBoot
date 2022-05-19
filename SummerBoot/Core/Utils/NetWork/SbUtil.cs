using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SummerBoot.Core
{
    public static partial class SbUtil
    {
        /// <summary>
        /// 获取本机ip
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentIp( )
        {
            var instanceIp = "127.0.0.1";

            try
            {
                // 获取可用网卡
                var nics = NetworkInterface.GetAllNetworkInterfaces()?.Where(network => network.OperationalStatus == OperationalStatus.Up).ToList();

                // 获取所有可用网卡IP信息
                var ipCollection = nics?.Select(x => x.GetIPProperties())?.SelectMany(x => x.UnicastAddresses).ToList();

                foreach (var ipadd in ipCollection)
                {
                    if (!IPAddress.IsLoopback(ipadd.Address) && ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        instanceIp = ipadd.Address.ToString();
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return instanceIp;
        }
    }
}