using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.Oracle.Models;

public class TestClob
{
    [Column(name: "ClobField", TypeName = "clob")]
    public string ClobField { get; set; }
}