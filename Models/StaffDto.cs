using System;

namespace MOM.Models
{
    public class StaffDto
    {
        public int StaffID { get; set; }
        public string StaffName { get; set; }
        public string DepartmentName { get; set; }
        public string MobileNo { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Created { get; set; }
        
        // Stats
        public int TotalMeetings { get; set; }
        public double AttendanceRate { get; set; }
    }
}
