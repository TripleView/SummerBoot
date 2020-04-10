using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gitlab用户同步.Model;
using SummerBoot.Feign;

namespace Gitlab用户同步.Serive
{
    [FeignClient(name:"AddUser",url: "http://172.23.11.94/api/v4/")]
    public interface IAddUserService
    {
        [GetMapping("users")]
        Task<List<GUser>> GetAllUsers(string private_token,int per_page);

        [PostMapping("users")]
        [Headers("KeepAlive:true")]
        Task<object> AddUsers(string private_token,[Form]NewUser user);
    }
}
