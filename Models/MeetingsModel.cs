using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOM.Models;

[Table("MOM_Meetings")]

public class MeetingsModel
{
    [Key]
    public int MeetingID { get; set; }

    [Required]
    public DateTime MeetingDate { get; set; }

    [Required]
    public int MeetingVenueID { get; set; }

    [Required]
    public int MeetingTypeID { get; set; }

    [Required]
    public int DepartmentID { get; set; }

    [StringLength(250)]
    public string? MeetingDescription { get; set; }

    [StringLength(250)]
    public string? DocumentPath { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime Modified { get; set; } = DateTime.Now;

    public bool? IsCancelled { get; set; }

    



    public DateTime? CancellationDateTime { get; set; }

    [StringLength(250)]
    public string? CancellationReason { get; set; }

    [ForeignKey("MeetingTypeID")]
    public virtual MeetingTypeModel MeetingType { get; set; }

    [ForeignKey("MeetingVenueID")]
    public virtual MeetingVenueModel MeetingVenue { get; set; }

    [ForeignKey("DepartmentID")]
    public virtual DepartmentModel Department { get; set; }
}