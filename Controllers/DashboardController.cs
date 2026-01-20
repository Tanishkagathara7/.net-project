using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOM.Data;
using MOM.ViewModels;

namespace MOM.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            DashboardViewModel model = new DashboardViewModel
            {
                TotalMeetingTypes = await _context.MeetingTypes.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalStaff = await _context.Staff.CountAsync(),
                TotalVenues = await _context.MeetingVenues.CountAsync(),
                TotalMeetings = await _context.Meetings.CountAsync(),
                CancelledMeetings = await _context.Meetings
                    .Where(m => m.IsCancelled == true)
                    .CountAsync()
            };

            return View(model);
        }
    }
}
