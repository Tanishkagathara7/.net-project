using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOM.Models;

[Table("MOM_MeetingMember")]

public class MeetingMemberModel
{
    [Key]
    public int MeetingMemberID { get; set; }

    [Required]  
    public int MeetingID { get; set; }

    [Required]
    public int StaffID { get; set; }

    [Required]
    public bool IsPresent { get; set; }

    [StringLength(250)]
    public string? Remarks { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime Modified { get; set; } = DateTime.Now;

    [ForeignKey("MeetingID")]
    public virtual MeetingsModel Meeting { get; set; }

    [ForeignKey("StaffID")]
    public virtual StaffModel Staff { get; set; }
}
