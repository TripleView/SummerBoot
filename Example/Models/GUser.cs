using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gitlab用户同步.Model
{
    public class GUser
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string UserName { set; get; }

        public string Email { set; get; }
    }
}
