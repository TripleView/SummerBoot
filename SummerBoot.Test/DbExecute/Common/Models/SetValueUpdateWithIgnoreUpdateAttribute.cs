using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SummerBoot.Repository.Attributes;

namespace SummerBoot.Test.DbExecute.Common.Models;

public class SetValueUpdateWithIgnoreUpdateAttribute
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { set; get; }
    public string Name { set; get; }
    /// <summary>
    /// ª·‘±∫≈
    /// </summary>
    [IgnoreWhenUpdate]
    public string CustomerNo { set; get; }
}