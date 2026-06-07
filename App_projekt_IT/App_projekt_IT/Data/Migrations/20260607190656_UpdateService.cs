using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_projekt_IT.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Services",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "ImagePath", "IsHighlighted" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "ImagePath", "IsHighlighted" },
                values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "ImagePath", "IsHighlighted" },
                values: new object[] { null, null, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "Services");
        }
    }
}
