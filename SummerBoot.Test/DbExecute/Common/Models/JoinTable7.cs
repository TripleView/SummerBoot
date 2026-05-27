using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.DbExecute.Common.Models;

public class JoinTable7
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
    public int Id { set; get; }

    public string Name { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreateTime { get; set; }

    public int Table6Id { get; set; }
}