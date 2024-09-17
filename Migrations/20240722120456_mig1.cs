using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SunucuBakimKontrol.Migrations
{
    /// <inheritdoc />
    public partial class mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.HolidayId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsibleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.ServerId);
                    table.ForeignKey(
                        name: "FK_Servers_Users_ResponsibleId",
                        column: x => x.ResponsibleId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenancePoints",
                columns: table => new
                {
                    MaintenancePointId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerId = table.Column<int>(type: "int", nullable: false),
                    MaintenancePointName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenancePoints", x => x.MaintenancePointId);
                    table.ForeignKey(
                        name: "FK_MaintenancePoints_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.CreateTable(
        name: "MaintenanceLogs",
        columns: table => new
        {
            LogId = table.Column<int>(nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            ServerId = table.Column<int>(nullable: false),
            MaintenancePointId = table.Column<int>(nullable: false),
            LogDate = table.Column<DateTime>(nullable: false),
            IsCompleted = table.Column<bool>(nullable: false),
            NotCompletedReason = table.Column<string>(nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_MaintenanceLogs", x => x.LogId);
            table.ForeignKey(
                name: "FK_MaintenanceLogs_MaintenancePoints_MaintenancePointId",
                column: x => x.MaintenancePointId,
                principalTable: "MaintenancePoints",
                principalColumn: "MaintenancePointId",
                onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                name: "FK_MaintenanceLogs_Servers_ServerId",
                column: x => x.ServerId,
                principalTable: "Servers",
                principalColumn: "ServerId",
                onDelete: ReferentialAction.Restrict); // Change to Restrict
        });
            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_MaintenancePointId",
                table: "MaintenanceLogs",
                column: "MaintenancePointId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceLogs_ServerId",
                table: "MaintenanceLogs",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenancePoints_ServerId",
                table: "MaintenancePoints",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ResponsibleId",
                table: "Servers",
                column: "ResponsibleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "MaintenanceLogs");

            migrationBuilder.DropTable(
                name: "MaintenancePoints");

            migrationBuilder.DropTable(
                name: "Servers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
