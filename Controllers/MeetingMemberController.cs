using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MOM.Data;
using MOM.Models;
using System.Data;

namespace MOM.Controllers
{
    public class MeetingMemberController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public MeetingMemberController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {
            List<MeetingMemberVM> list = new();

            using SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("MOMConnection"));

            SqlCommand cmd = new SqlCommand(
                "PR_MOM_MeetingMember_SelectAll_Join", con);

            cmd.CommandType = CommandType.StoredProcedure;
            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                list.Add(new MeetingMemberVM
                {
                    MeetingMemberID = Convert.ToInt32(dr["MeetingMemberID"]),
                    MeetingDate = Convert.ToDateTime(dr["MeetingDate"]),
                    StaffName = dr["StaffName"]?.ToString() ?? "",
                    IsPresent = Convert.ToBoolean(dr["IsPresent"]),
                    Remarks = dr["Remarks"]?.ToString() ?? ""
                });

            }

            return View(list);
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MeetingMemberModel model)
        {
            if (ModelState.IsValid)
            {
                // Duplicate Check: Prevent same staff in same meeting
                bool exists = await _context.MeetingMembers.AnyAsync(m => 
                    m.MeetingID == model.MeetingID && m.StaffID == model.StaffID);

                if (exists)
                {
                    ModelState.AddModelError("", "This staff member is already added to this meeting.");
                }
                else
                {
                    try
                    {
                        model.Created = DateTime.Now;
                        model.Modified = DateTime.Now;

                        _context.MeetingMembers.Add(model);
                        await _context.SaveChangesAsync();
                        
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Database error: {ex.Message}");
                    }
                }
            }
            PopulateDropdowns();
            return View(model);
        }

        private void PopulateDropdowns()
        {
            ViewBag.MeetingList = _context.Meetings
                .Select(m => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = m.MeetingID.ToString(),
                    Text = "ID: " + m.MeetingID + " - " + (m.MeetingDescription ?? "No Description")
                }).ToList();

            ViewBag.StaffList = _context.Staff
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.StaffID.ToString(),
                    Text = s.StaffName
                }).ToList();
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var member = await _context.MeetingMembers.FindAsync(id);
                if (member != null)
                {
                    _context.MeetingMembers.Remove(member);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error or set TempData message if needed
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
