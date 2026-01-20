using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MOM.Data;
using MOM.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MOM.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public StaffController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? departmentId, string searchTerm)
        {
            var query = _context.Staff
                .Include(s => s.Department)
                .AsQueryable();

            // Apply Filters
            if (departmentId.HasValue)
            {
                query = query.Where(s => s.DepartmentID == departmentId);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.StaffName.Contains(searchTerm) || s.EmailAddress.Contains(searchTerm));
            }

            var staffData = await query
                .Select(s => new
                {
                    s.StaffID,
                    s.StaffName,
                    DepartmentName = s.Department.DepartmentName,
                    s.MobileNo,
                    s.EmailAddress,
                    s.Created,
                    // Subquery for stats (EF Core will optimize)
                    MeetingStats = _context.MeetingMembers
                        .Where(m => m.StaffID == s.StaffID && m.Meeting.IsCancelled != true)
                        .Select(m => new { m.IsPresent })
                        .ToList()
                })
                .ToListAsync();

            // Map to DTO in memory (simpler for stats calculation)
            var staffList = staffData.Select(s => {
                var total = s.MeetingStats.Count;
                var present = s.MeetingStats.Count(m => m.IsPresent);
                var rate = total > 0 ? (double)present / total * 100 : 0;

                return new StaffDto
                {
                    StaffID = s.StaffID,
                    StaffName = s.StaffName,
                    DepartmentName = s.DepartmentName,
                    MobileNo = s.MobileNo,
                    EmailAddress = s.EmailAddress,
                    Created = s.Created,
                    TotalMeetings = total,
                    AttendanceRate = Math.Round(rate, 1)
                };
            }).ToList();

            var model = new StaffListVM
            {
                StaffList = staffList,
                DepartmentID = departmentId,
                SearchTerm = searchTerm,
                Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentID", "DepartmentName")
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDepartments();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(StaffModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                    await connection.OpenAsync();
                    
                    var sql = @"
                        INSERT INTO MOM_Staff (StaffName, DepartmentID, EmailAddress, MobileNo, Remarks, Created, Modified)
                        VALUES (@StaffName, @DepartmentID, @EmailAddress, @MobileNo, @Remarks, @Created, @Modified)";
                    
                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@StaffName", model.StaffName);
                    command.Parameters.AddWithValue("@DepartmentID", model.DepartmentID);
                    command.Parameters.AddWithValue("@EmailAddress", model.EmailAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MobileNo", model.MobileNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Created", DateTime.Now);
                    command.Parameters.AddWithValue("@Modified", DateTime.Now);
                    
                    await command.ExecuteNonQueryAsync();
                    TempData["SuccessMessage"] = "Staff member created successfully!";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating staff member: {ex.Message}";
                }
            }
            
            await PopulateDepartments();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            StaffModel? staff = null;
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                using var command = new SqlCommand("SELECT * FROM MOM_Staff WHERE StaffID = @StaffID", connection);
                
                command.Parameters.AddWithValue("@StaffID", id);
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    staff = new StaffModel
                    {
                        StaffID = reader.GetInt32("StaffID"),
                        StaffName = reader.GetString("StaffName"),
                        DepartmentID = reader.GetInt32("DepartmentID"),
                        EmailAddress = reader.IsDBNull("EmailAddress") ? null : reader.GetString("EmailAddress"),
                        MobileNo = reader.IsDBNull("MobileNo") ? null : reader.GetString("MobileNo"),
                        Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                        Created = reader.IsDBNull("Created") ? DateTime.Now : reader.GetDateTime("Created")
                    };
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading staff member: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            
            if (staff == null)
            {
                return NotFound();
            }
            
            await PopulateDepartments();
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StaffModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                    using var command = new SqlCommand(@"
                        UPDATE MOM_Staff 
                        SET StaffName = @StaffName, 
                            DepartmentID = @DepartmentID, 
                            EmailAddress = @EmailAddress, 
                            MobileNo = @MobileNo, 
                            Remarks = @Remarks, 
                            Modified = @Modified
                        WHERE StaffID = @StaffID", 
                        connection);
                    
                    command.Parameters.AddWithValue("@StaffID", model.StaffID);
                    command.Parameters.AddWithValue("@StaffName", model.StaffName);
                    command.Parameters.AddWithValue("@DepartmentID", model.DepartmentID);
                    command.Parameters.AddWithValue("@EmailAddress", model.EmailAddress ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MobileNo", model.MobileNo ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Modified", DateTime.Now);
                    
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    
                    TempData["SuccessMessage"] = "Staff member updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating staff member: {ex.Message}";
                }
            }
            
            await PopulateDepartments();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();
                
                // Check if staff member is used in meeting members
                using var checkCommand = new SqlCommand("SELECT COUNT(*) FROM MOM_MeetingMember WHERE StaffID = @StaffID", connection);
                checkCommand.Parameters.AddWithValue("@StaffID", id);
                
                var meetingMemberships = (int)await checkCommand.ExecuteScalarAsync();

                if (meetingMemberships > 0)
                {
                    TempData["ErrorMessage"] = $"Cannot delete this staff member because they are assigned to {meetingMemberships} meeting(s). Please remove them from meetings first.";
                    return RedirectToAction(nameof(Index));
                }

                // If not used in meetings, proceed with deletion
                using var deleteCommand = new SqlCommand("DELETE FROM MOM_Staff WHERE StaffID = @StaffID", connection);
                deleteCommand.Parameters.AddWithValue("@StaffID", id);
                
                var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Staff member deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Staff member not found or could not be deleted.";
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.Contains("REFERENCE constraint"))
                {
                    TempData["ErrorMessage"] = "Cannot delete this staff member because they are referenced in other records.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Database error: {sqlEx.Message}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting staff member: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDepartments()
        {
            var departments = new List<DepartmentModel>();
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT DepartmentID, DepartmentName FROM MOM_Department ORDER BY DepartmentName", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    departments.Add(new DepartmentModel
                    {
                        DepartmentID = reader.GetInt32("DepartmentID"),
                        DepartmentName = reader.GetString("DepartmentName")
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading departments: {ex.Message}";
            }

            ViewBag.Departments = departments.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.DepartmentID.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }
    }
}
