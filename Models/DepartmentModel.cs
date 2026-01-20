using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOM.Models;

[Table("MOM_Department")]

public class DepartmentModel
{
    [Key]
    public int DepartmentID { get; set; }

    [StringLength(100)]
    public string DepartmentName { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime Modified { get; set; } = DateTime.Now;
}