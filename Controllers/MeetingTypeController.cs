using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MOM.Models;
using System.Data;

namespace MOM.Controllers
{
    public class MeetingTypeController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeetingTypeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            List<MeetingTypeModel> list = new();

            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();

                using var command = new SqlCommand(@"
                    SELECT MeetingTypeID, MeetingTypeName, Remarks, Created, Modified 
                    FROM MOM_MeetingType 
                    ORDER BY MeetingTypeName", connection);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MeetingTypeModel
                    {
                        MeetingTypeID = reader.GetInt32("MeetingTypeID"),
                        MeetingTypeName = reader.GetString("MeetingTypeName"),
                        Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                        Created = reader.IsDBNull("Created") ? DateTime.Now : reader.GetDateTime("Created"),
                        Modified = reader.IsDBNull("Modified") ? DateTime.Now : reader.GetDateTime("Modified")
                    });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading meeting types: {ex.Message}";
            }

            return View(list);
        }

        // ADD
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(MeetingTypeModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                    await connection.OpenAsync();
                    
                    var sql = @"
                        INSERT INTO MOM_MeetingType (MeetingTypeName, Remarks, Created, Modified)
                        VALUES (@MeetingTypeName, @Remarks, @Created, @Modified)";
                    
                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@MeetingTypeName", model.MeetingTypeName);
                    command.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Created", DateTime.Now);
                    command.Parameters.AddWithValue("@Modified", DateTime.Now);
                    
                    await command.ExecuteNonQueryAsync();
                    TempData["SuccessMessage"] = "Meeting type created successfully!";
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error creating meeting type: {ex.Message}";
                }
            }
            return View(model);
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            MeetingTypeModel? meetingType = null;
            
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                using var command = new SqlCommand("SELECT * FROM MOM_MeetingType WHERE MeetingTypeID = @MeetingTypeID", connection);
                
                command.Parameters.AddWithValue("@MeetingTypeID", id);
                
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    meetingType = new MeetingTypeModel
                    {
                        MeetingTypeID = reader.GetInt32("MeetingTypeID"),
                        MeetingTypeName = reader.GetString("MeetingTypeName"),
                        Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                        Created = reader.IsDBNull("Created") ? DateTime.Now : reader.GetDateTime("Created")
                    };
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading meeting type: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            
            if (meetingType == null)
            {
                return NotFound();
            }
            
            return View(meetingType);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MeetingTypeModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                    using var command = new SqlCommand(@"
                        UPDATE MOM_MeetingType 
                        SET MeetingTypeName = @MeetingTypeName, 
                            Remarks = @Remarks, 
                            Modified = @Modified
                        WHERE MeetingTypeID = @MeetingTypeID", 
                        connection);
                    
                    command.Parameters.AddWithValue("@MeetingTypeID", model.MeetingTypeID);
                    command.Parameters.AddWithValue("@MeetingTypeName", model.MeetingTypeName);
                    command.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Modified", DateTime.Now);
                    
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    
                    TempData["SuccessMessage"] = "Meeting type updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating meeting type: {ex.Message}";
                }
            }
            return View(model);
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("MOMConnection"));
                await connection.OpenAsync();
                
                // Use a more robust approach - try to delete and catch the constraint error
                // But first, let's get detailed information about what's blocking the delete
                
                // Check for meetings using this type with detailed info
                using var checkCommand = new SqlCommand(@"
                    SELECT m.MeetingID, m.MeetingDate, ISNULL(m.MeetingDescription, 'No description') as Description
                    FROM MOM_Meetings m 
                    WHERE m.MeetingTypeID = @MeetingTypeID 
                    ORDER BY m.MeetingDate DESC", connection);
                checkCommand.Parameters.AddWithValue("@MeetingTypeID", id);
                
                var blockingMeetings = new List<string>();
                using var reader = await checkCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var meetingDate = reader.GetDateTime("MeetingDate").ToString("MMM dd, yyyy HH:mm");
                    var description = reader.GetString("Description");
                    blockingMeetings.Add($"Meeting on {meetingDate} ({description})");
                }
                reader.Close();
                
                if (blockingMeetings.Any())
                {
                    var meetingList = string.Join("; ", blockingMeetings.Take(3));
                    if (blockingMeetings.Count > 3)
                    {
                        meetingList += $"; and {blockingMeetings.Count - 3} more meetings";
                    }
                    
                    TempData["ErrorMessage"] = $"Cannot delete this meeting type because it is being used by {blockingMeetings.Count} meeting(s): {meetingList}. Please reassign or delete those meetings first.";
                    return RedirectToAction(nameof(Index));
                }
                
                // If we get here, no meetings are using this type, so it should be safe to delete
                // But let's still use a try-catch as a final safety net
                try
                {
                    using var deleteCommand = new SqlCommand("DELETE FROM MOM_MeetingType WHERE MeetingTypeID = @MeetingTypeID", connection);
                    deleteCommand.Parameters.AddWithValue("@MeetingTypeID", id);
                    
                    var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();
                    
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Meeting type deleted successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Meeting type not found or could not be deleted.";
                    }
                }
                catch (SqlException deleteEx) when (deleteEx.Message.Contains("REFERENCE constraint"))
                {
                    // This should not happen if our check above worked, but just in case...
                    TempData["ErrorMessage"] = "Cannot delete this meeting type because it is still being referenced by meetings. There may have been a concurrent change. Please refresh and try again.";
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.Contains("REFERENCE constraint") || sqlEx.Message.Contains("FK_"))
                {
                    TempData["ErrorMessage"] = "Cannot delete this meeting type because it is being used by existing meetings. Please reassign or delete those meetings first.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Database error: {sqlEx.Message}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting meeting type: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
