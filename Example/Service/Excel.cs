using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Example.Models;
using Gitlab用户同步.Model;
using OfficeOpenXml;

namespace Example.Service
{
    public class Excel
    {
        /// <summary>
        /// 读取sheet 内的数据进入实体
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public static List<NewUser> GetSheetValues(string filepath)
        {
            FileInfo file = new FileInfo(filepath);
            if (file != null)
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    //获取表格的列数和行数
                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;
                    var persons = new List<NewUser>();
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var person = new NewUser();
                        person.username = worksheet.Cells[row, 5].Value.ToString();
                        person.name = worksheet.Cells[row, 2].Value.ToString();
                        var emailTmp = worksheet.Cells[row, 3].Value.ToString();
                        
                        person.email = emailTmp.Split("@")[0] + "@ATLBattery.com";
                        persons.Add(person);
                    }
                    return persons;
                }
            }
            return null;
        }
    }
}
