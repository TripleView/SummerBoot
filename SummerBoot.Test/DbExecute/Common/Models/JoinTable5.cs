using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SummerBoot.Test.DbExecute.Common.Models;

public class JoinTable5
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("ID")]
    public int Id { set; get; }

    public string Name { get; set; }
    public int Index { get; set; }
    public DateTime CreateTime { get; set; }

    public int Table4Id { get; set; }
}