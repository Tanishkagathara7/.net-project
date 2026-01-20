using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOM.Models;

[Table("MOM_MeetingType")]

public class MeetingTypeModel
{
    [Key]
    public int MeetingTypeID { get; set; }

    [Required]
    [StringLength(100)]
    public string MeetingTypeName { get; set; }

    public string? Remarks { get; set; }

    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
}
