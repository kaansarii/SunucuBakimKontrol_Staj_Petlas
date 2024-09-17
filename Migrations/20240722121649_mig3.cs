using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SunucuBakimKontrol.Migrations
{
    /// <inheritdoc />
    public partial class mig3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "denemeSutunu",
                table: "Holidays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "denemeSutunu",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
