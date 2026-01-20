using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MOM.Models;
using System.Data;

namespace MOM.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeetingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            List<MeetingListVM> list = new();

            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();

                using var command = new SqlCommand(@"
                    SELECT 
                        m.MeetingID,
                        m.MeetingDate,
                        m.MeetingTypeID,
                        m.MeetingVenueID, 
                        m.DepartmentID,
                        mt.MeetingTypeName,
                        mv.MeetingVenueName,
                        d.DepartmentName,
                        ISNULL(m.IsCancelled, 0) as IsCancelled
                    FROM MOM_Meetings m
                    LEFT JOIN MOM_MeetingType mt ON m.MeetingTypeID = mt.MeetingTypeID
                    LEFT JOIN MOM_MeetingVenue mv ON m.MeetingVenueID = mv.MeetingVenueID
                    LEFT JOIN MOM_Department d ON m.DepartmentID = d.DepartmentID
                    ORDER BY m.MeetingDate DESC", connection);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MeetingListVM
                    {
                        MeetingID = reader.GetInt32("MeetingID"),
                        MeetingDate = reader.GetDateTime("MeetingDate"),
                        MeetingTypeName = reader.IsDBNull("MeetingTypeName") ? $"Type ID: {reader.GetInt32("MeetingTypeID")}" : reader.GetString("MeetingTypeName"),
                        MeetingVenueName = reader.IsDBNull("MeetingVenueName") ? $"Venue ID: {reader.GetInt32("MeetingVenueID")}" : reader.GetString("MeetingVenueName"),
                        DepartmentName = reader.IsDBNull("DepartmentName") ? $"Dept ID: {reader.GetInt32("DepartmentID")}" : reader.GetString("DepartmentName"),
                        IsCancelled = reader.GetBoolean("IsCancelled")
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading meetings: {ex.Message}";
            }

            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MeetingsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                    await connection.OpenAsync();
                    
                    var sql = @"
                        INSERT INTO MOM_Meetings (MeetingDate, MeetingVenueID, MeetingTypeID, DepartmentID, MeetingDescription, DocumentPath, Created, Modified, IsCancelled)
                        VALUES (@MeetingDate, @MeetingVenueID, @MeetingTypeID, @DepartmentID, @MeetingDescription, @DocumentPath, @Created, @Modified, @IsCancelled)";
                    
                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@MeetingDate", model.MeetingDate);
                    command.Parameters.AddWithValue("@MeetingVenueID", model.MeetingVenueID);
                    command.Parameters.AddWithValue("@MeetingTypeID", model.MeetingTypeID);
                    command.Parameters.AddWithValue("@DepartmentID", model.DepartmentID);
                    command.Parameters.AddWithValue("@MeetingDescription", model.MeetingDescription ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentPath", model.DocumentPath ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Created", DateTime.Now);
                    command.Parameters.AddWithValue("@Modified", DateTime.Now);
                    command.Parameters.AddWithValue("@IsCancelled", false);
                    
                    await command.ExecuteNonQueryAsync();
                    TempData["SuccessMessage"] = "Meeting created successfully!";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating meeting: {ex.Message}";
                }
            }
            
            await PopulateDropdowns();
            return View(model);
        }

        private async Task PopulateDropdowns()
        {
            var meetingTypes = new List<MeetingTypeModel>();
            var meetingVenues = new List<MeetingVenueModel>();
            var departments = new List<DepartmentModel>();
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();

                // Get Meeting Types
                using var mtCommand = new SqlCommand("SELECT MeetingTypeID, MeetingTypeName FROM MOM_MeetingType ORDER BY MeetingTypeName", connection);
                using var mtReader = await mtCommand.ExecuteReaderAsync();
                while (await mtReader.ReadAsync())
                {
                    meetingTypes.Add(new MeetingTypeModel
                    {
                        MeetingTypeID = mtReader.GetInt32("MeetingTypeID"),
                        MeetingTypeName = mtReader.GetString("MeetingTypeName")
                    });
                }
                mtReader.Close();

                // Get Meeting Venues
                using var mvCommand = new SqlCommand("SELECT MeetingVenueID, MeetingVenueName FROM MOM_MeetingVenue ORDER BY MeetingVenueName", connection);
                using var mvReader = await mvCommand.ExecuteReaderAsync();
                while (await mvReader.ReadAsync())
                {
                    meetingVenues.Add(new MeetingVenueModel
                    {
                        MeetingVenueID = mvReader.GetInt32("MeetingVenueID"),
                        MeetingVenueName = mvReader.GetString("MeetingVenueName")
                    });
                }
                mvReader.Close();

                // Get Departments
                using var dCommand = new SqlCommand("SELECT DepartmentID, DepartmentName FROM MOM_Department ORDER BY DepartmentName", connection);
                using var dReader = await dCommand.ExecuteReaderAsync();
                while (await dReader.ReadAsync())
                {
                    departments.Add(new DepartmentModel
                    {
                        DepartmentID = dReader.GetInt32("DepartmentID"),
                        DepartmentName = dReader.GetString("DepartmentName")
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dropdown data: {ex.Message}";
            }

            ViewBag.MeetingTypes = meetingTypes.Select(mt => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = mt.MeetingTypeID.ToString(),
                Text = mt.MeetingTypeName
            }).ToList();

            ViewBag.MeetingVenues = meetingVenues.Select(mv => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = mv.MeetingVenueID.ToString(),
                Text = mv.MeetingVenueName
            }).ToList();

            ViewBag.Departments = departments.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.DepartmentID.ToString(),
                Text = d.DepartmentName
            }).ToList();
        }

        public async Task<IActionResult> Edit(int id)
        {
            MeetingsModel? meeting = null;
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                using var command = new SqlCommand("SELECT * FROM MOM_Meetings WHERE MeetingID = @MeetingID", connection);
                
                command.Parameters.AddWithValue("@MeetingID", id);
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    meeting = new MeetingsModel
                    {
                        MeetingID = reader.GetInt32("MeetingID"),
                        MeetingDate = reader.GetDateTime("MeetingDate"),
                        MeetingVenueID = reader.GetInt32("MeetingVenueID"),
                        MeetingTypeID = reader.GetInt32("MeetingTypeID"),
                        DepartmentID = reader.GetInt32("DepartmentID"),
                        MeetingDescription = reader.IsDBNull("MeetingDescription") ? null : reader.GetString("MeetingDescription"),
                        DocumentPath = reader.IsDBNull("DocumentPath") ? null : reader.GetString("DocumentPath"),
                        IsCancelled = reader.IsDBNull("IsCancelled") ? false : reader.GetBoolean("IsCancelled"),
                        Created = reader.IsDBNull("Created") ? DateTime.Now : reader.GetDateTime("Created")
                    };
                }
            }
            catch (Exception ex)
            {
                // Handle error
                return NotFound();
            }
            
            if (meeting == null)
            {
                return NotFound();
            }
            
            await PopulateDropdowns();
            return View(meeting);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MeetingsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                    using var command = new SqlCommand(@"
                        UPDATE MOM_Meetings 
                        SET MeetingDate = @MeetingDate, 
                            MeetingVenueID = @MeetingVenueID, 
                            MeetingTypeID = @MeetingTypeID, 
                            DepartmentID = @DepartmentID, 
                            MeetingDescription = @MeetingDescription, 
                            DocumentPath = @DocumentPath, 
                            Modified = @Modified
                        WHERE MeetingID = @MeetingID", 
                        connection);
                    
                    command.Parameters.AddWithValue("@MeetingID", model.MeetingID);
                    command.Parameters.AddWithValue("@MeetingDate", model.MeetingDate);
                    command.Parameters.AddWithValue("@MeetingVenueID", model.MeetingVenueID);
                    command.Parameters.AddWithValue("@MeetingTypeID", model.MeetingTypeID);
                    command.Parameters.AddWithValue("@DepartmentID", model.DepartmentID);
                    command.Parameters.AddWithValue("@MeetingDescription", model.MeetingDescription ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DocumentPath", model.DocumentPath ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Modified", DateTime.Now);
                    
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Database error: {ex.Message}");
                }
            }
            
            await PopulateDropdowns();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Console.WriteLine($"MeetingsController Delete method called with ID: {id}");
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();
                
                // Start a transaction to ensure data consistency
                using var transaction = connection.BeginTransaction();
                
                try
                {
                    // First check if the meeting exists
                    using var existsCommand = new SqlCommand("SELECT COUNT(*) FROM MOM_Meetings WHERE MeetingID = @MeetingID", connection, transaction);
                    existsCommand.Parameters.AddWithValue("@MeetingID", id);
                    var meetingExists = (int)await existsCommand.ExecuteScalarAsync();
                    
                    if (meetingExists == 0)
                    {
                        TempData["ErrorMessage"] = "Meeting not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    Console.WriteLine($"Meeting {id} exists, proceeding with deletion");
                    
                    // Delete all related records first
                    
                    // 1. Delete meeting members
                    using var deleteMembersCommand = new SqlCommand("DELETE FROM MOM_MeetingMember WHERE MeetingID = @MeetingID", connection, transaction);
                    deleteMembersCommand.Parameters.AddWithValue("@MeetingID", id);
                    var membersDeleted = await deleteMembersCommand.ExecuteNonQueryAsync();
                    Console.WriteLine($"Deleted {membersDeleted} meeting members");
                    
                    // 2. Check for any other related tables that might reference this meeting
                    // Add more DELETE statements here if there are other related tables
                    
                    // 3. Finally delete the meeting itself
                    using var deleteMeetingCommand = new SqlCommand("DELETE FROM MOM_Meetings WHERE MeetingID = @MeetingID", connection, transaction);
                    deleteMeetingCommand.Parameters.AddWithValue("@MeetingID", id);
                    var meetingsDeleted = await deleteMeetingCommand.ExecuteNonQueryAsync();
                    Console.WriteLine($"Deleted {meetingsDeleted} meeting records");
                    
                    // Commit the transaction
                    transaction.Commit();
                    
                    if (meetingsDeleted > 0)
                    {
                        TempData["SuccessMessage"] = "Meeting deleted successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Meeting could not be deleted.";
                    }
                }
                catch (Exception ex)
                {
                    // Rollback the transaction on error
                    transaction.Rollback();
                    throw;
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Error in MeetingsController.Delete: {sqlEx.Message}");
                
                if (sqlEx.Message.Contains("REFERENCE constraint") || sqlEx.Message.Contains("FK_"))
                {
                    TempData["ErrorMessage"] = "Cannot delete this meeting because it is referenced by other records. Please remove all related data first.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Database error: {sqlEx.Message}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in MeetingsController.Delete: {ex.Message}");
                TempData["ErrorMessage"] = $"Error deleting meeting: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Alternative GET method for delete (for testing)
        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            return await Delete(id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();

                using var command = new SqlCommand("UPDATE MOM_Meetings SET IsCancelled = 1, Modified = @Modified WHERE MeetingID = @MeetingID", connection);
                command.Parameters.AddWithValue("@MeetingID", id);
                command.Parameters.AddWithValue("@Modified", DateTime.Now);

                await command.ExecuteNonQueryAsync();
                TempData["SuccessMessage"] = "Meeting cancelled successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling meeting: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
