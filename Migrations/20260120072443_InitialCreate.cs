using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MOM_Department",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOM_Department", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "MOM_MeetingType",
                columns: table => new
                {
                    MeetingTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOM_MeetingType", x => x.MeetingTypeID);
                });

            migrationBuilder.CreateTable(
                name: "MOM_MeetingVenue",
                columns: table => new
                {
                    MeetingVenueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingVenueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOM_MeetingVenue", x => x.MeetingVenueID);
                });

            migrationBuilder.CreateTable(
                name: "MOM_Staff",
                columns: table => new
                {
                    StaffID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    StaffName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MobileNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOM_Staff", x => x.StaffID);
                    table.ForeignKey(
                        name: "FK_MOM_Staff_MOM_Department_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "MOM_Department",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MOM_Meetings",
                columns: table => new
                {
                    MeetingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MeetingVenueID = table.Column<int>(type: "int", nullable: false),
                    MeetingTypeID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    MeetingDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: true),
                    CancellationDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOM_Meetings", x => x.MeetingID);
                    table.ForeignKey(
                        name: "FK_MOM_Meetings_MOM_Department_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "MOM_Department",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOM_Meetings_MOM_MeetingType_MeetingTypeID",
                        column: x => x.MeetingTypeID,
                        principalTable: "MOM_MeetingType",
                        principalColumn: "MeetingTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOM_Meetings_MOM_MeetingVenue_MeetingVenueID",
                        column: x => x.MeetingVenueID,
                        principalTable: "MOM_MeetingVenue",
                        principalColumn: "MeetingVenueID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MOM_MeetingMember",
                columns: table => new
                {
                    MeetingMemberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetingID = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MOM_MeetingMember", x => x.MeetingMemberID);
                    table.ForeignKey(
                        name: "FK_MOM_MeetingMember_MOM_Meetings_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "MOM_Meetings",
                        principalColumn: "MeetingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MOM_MeetingMember_MOM_Staff_StaffID",
                        column: x => x.StaffID,
                        principalTable: "MOM_Staff",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MOM_MeetingMember_MeetingID",
                table: "MOM_MeetingMember",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_MOM_MeetingMember_StaffID",
                table: "MOM_MeetingMember",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_MOM_Meetings_DepartmentID",
                table: "MOM_Meetings",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_MOM_Meetings_MeetingTypeID",
                table: "MOM_Meetings",
                column: "MeetingTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MOM_Meetings_MeetingVenueID",
                table: "MOM_Meetings",
                column: "MeetingVenueID");

            migrationBuilder.CreateIndex(
                name: "IX_MOM_Staff_DepartmentID",
                table: "MOM_Staff",
                column: "DepartmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MOM_MeetingMember");

            migrationBuilder.DropTable(
                name: "MOM_Meetings");

            migrationBuilder.DropTable(
                name: "MOM_Staff");

            migrationBuilder.DropTable(
                name: "MOM_MeetingType");

            migrationBuilder.DropTable(
                name: "MOM_MeetingVenue");

            migrationBuilder.DropTable(
                name: "MOM_Department");
        }
    }
}
