
namespace MOM.Models;

public class MeetingMemberVM
{
    public int MeetingMemberID { get; set; }
    public DateTime MeetingDate { get; set; }
    public string MeetingTitle { get; set; }
    public string StaffName { get; set; }

    public bool IsPresent { get; set; }
    public string Remarks { get; set; }
}
