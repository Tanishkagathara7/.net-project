using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOM.Data;
using MOM.Models;
using System.Text;

namespace MOM.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Default to last 30 days if no dates provided
            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue) endDate = DateTime.Today;

            var model = new AttendanceReportVM
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Query Data
            var query = _context.MeetingMembers
                .Include(mm => mm.Meeting)
                    .ThenInclude(m => m.MeetingType)
                .Include(mm => mm.Meeting)
                    .ThenInclude(m => m.MeetingVenue)
                .Include(mm => mm.Meeting)
                    .ThenInclude(m => m.Department)
                .Include(mm => mm.Staff)
                .Where(mm => mm.Meeting.IsCancelled != true); // Exclude cancelled meetings

            if (startDate.HasValue)
                query = query.Where(mm => mm.Meeting.MeetingDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(mm => mm.Meeting.MeetingDate <= endDate.Value.AddDays(1).AddTicks(-1)); // End of day

            var records = await query.OrderByDescending(mm => mm.Meeting.MeetingDate)
                .Select(mm => new AttendanceRecord
                {
                    MeetingID = mm.MeetingID,
                    MeetingDate = mm.Meeting.MeetingDate,
                    MeetingType = mm.Meeting.MeetingType.MeetingTypeName,
                    Venue = mm.Meeting.MeetingVenue.MeetingVenueName,
                    Department = mm.Meeting.Department.DepartmentName,
                    StaffName = mm.Staff.StaffName,
                    Email = mm.Staff.EmailAddress,
                    IsPresent = mm.IsPresent,
                    Remarks = mm.Remarks
                }).ToListAsync();

            model.AttendanceRecords = records;

            // Calculate Stats
            model.TotalRecords = records.Count;
            model.PresentCount = records.Count(r => r.IsPresent);
            model.AttendanceRate = model.TotalRecords > 0 
                ? Math.Round((double)model.PresentCount / model.TotalRecords * 100, 1) 
                : 0;
            model.UniqueMeetings = records.Select(r => r.MeetingID).Distinct().Count();

            return View(model);
        }

        public async Task<IActionResult> Export(DateTime? startDate, DateTime? endDate)
        {
            // Re-run query (could be optimized but keeping simple for now)
             var query = _context.MeetingMembers
                .Include(mm => mm.Meeting)
                    .ThenInclude(m => m.MeetingType)
                .Include(mm => mm.Meeting)
                    .ThenInclude(m => m.MeetingVenue)
                .Include(mm => mm.Meeting)
                    .ThenInclude(m => m.Department)
                .Include(mm => mm.Staff)
                .Where(mm => mm.Meeting.IsCancelled != true);

            if (startDate.HasValue)
                query = query.Where(mm => mm.Meeting.MeetingDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(mm => mm.Meeting.MeetingDate <= endDate.Value.AddDays(1).AddTicks(-1));

            var records = await query.OrderByDescending(mm => mm.Meeting.MeetingDate)
                .Select(mm => new AttendanceRecord
                {
                    MeetingDate = mm.Meeting.MeetingDate,
                    MeetingType = mm.Meeting.MeetingType.MeetingTypeName,
                    Venue = mm.Meeting.MeetingVenue.MeetingVenueName,
                    Department = mm.Meeting.Department.DepartmentName,
                    StaffName = mm.Staff.StaffName,
                    Email = mm.Staff.EmailAddress,
                    IsPresent = mm.IsPresent,
                    Remarks = mm.Remarks
                }).ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Meeting Date,Meeting Type,Venue,Department,Staff Name,Email,Attendance,Remarks");

            foreach (var record in records)
            {
                builder.AppendLine($"{record.MeetingDate},{record.MeetingType},{record.Venue},{record.Department},{record.StaffName},{record.Email},{(record.IsPresent ? "Present" : "Absent")},{record.Remarks}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Attendance_Report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
