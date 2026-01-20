using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOM.Models;

[Table("MOM_Staff")]

public class StaffModel
{
    [Key]
    public int StaffID { get; set; }

    [Required]
    public int DepartmentID { get; set; }

    [StringLength(50)]
    public string StaffName { get; set; }

    [StringLength(20)]
    public string MobileNo { get; set; }

    [StringLength(50)]
    public string EmailAddress { get; set; }

    [StringLength(250)]
    public string? Remarks { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime Modified { get; set; } = DateTime.Now;

    [ForeignKey("DepartmentID")]
    public virtual DepartmentModel Department { get; set; }
}