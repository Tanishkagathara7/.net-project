namespace MOM.Data;
using Microsoft.EntityFrameworkCore;
using MOM.Models;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MeetingTypeModel> MeetingTypes { get; set; }
    public DbSet<DepartmentModel> Departments { get; set; }
    public DbSet<MeetingVenueModel> MeetingVenues { get; set; }
    public DbSet<StaffModel> Staff { get; set; }
    public DbSet<MeetingsModel> Meetings { get; set; }
    public DbSet<MeetingMemberModel> MeetingMembers { get; set; }



}

