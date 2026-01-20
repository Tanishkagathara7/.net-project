using System;

namespace MOM.Models
{
    public class AttendanceRecord
    {
        public int MeetingID { get; set; }
        public DateTime MeetingDate { get; set; }
        public string MeetingType { get; set; }
        public string Venue { get; set; }
        public string Department { get; set; }
        public string StaffName { get; set; }
        public string Email { get; set; }
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
    }
}
