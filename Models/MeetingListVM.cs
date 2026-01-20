
namespace MOM.Models;

public class MeetingListVM
{
    public int MeetingID { get; set; }
    public DateTime MeetingDate { get; set; }

    public string MeetingTypeName { get; set; }
    public string MeetingVenueName { get; set; }
    public string DepartmentName { get; set; }

    public bool IsCancelled { get; set; }
}
