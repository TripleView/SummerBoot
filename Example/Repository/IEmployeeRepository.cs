using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gitlab用户同步.Model;
using SummerBoot.Repository;

namespace Example.Repository
{
    //[Repository]
    public interface IEmployeeRepository
    {
        [Select("select he.emp_no as username,he.emp_name as name,'1' as email from ATL_Custom.dbo.HR_Employee he")]
        Task<Page<NewUser>> GetEmployeesAsync(IPageable pa);
        [Select("select equipment as name,id as username from equipment")]
        Task<Page<NewUser>> GetEquipmentAsync(IPageable pa);
        [Select("select equipment as name,id as username from equipment")]
        Page<NewUser> GetEquipment(IPageable pa);
    }
}
