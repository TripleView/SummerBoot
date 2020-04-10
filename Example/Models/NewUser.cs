using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gitlab用户同步.Model
{
    public class NewUser
    {
        public bool reset_password { set; get; }
        public bool force_random_password { set; get; }
        public bool skip_confirmation { set; get; }
        
        public string password { set; get; }
        public string name { set; get; }
        public string username { set; get; }

        public string email { set; get; }
    }
}
