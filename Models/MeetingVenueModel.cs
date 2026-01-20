using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MOM.Models;

[Table("MOM_MeetingVenue")]

public class MeetingVenueModel
{
    [Key]
    public int MeetingVenueID { get; set; }

    [StringLength(100)]
    public string MeetingVenueName { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;

    public DateTime Modified { get; set; } = DateTime.Now;
}