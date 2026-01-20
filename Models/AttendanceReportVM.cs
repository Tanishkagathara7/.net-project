using System;
using System.Collections.Generic;

namespace MOM.Models
{
    public class AttendanceReportVM
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public int TotalRecords { get; set; }
        public int PresentCount { get; set; }
        public double AttendanceRate { get; set; }
        public int UniqueMeetings { get; set; }
        
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
