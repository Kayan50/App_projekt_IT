using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_projekt_IT.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentSlots_AspNetUsers_UserId",
                table: "AppointmentSlots");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AppointmentSlots",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentSlots_AspNetUsers_UserId",
                table: "AppointmentSlots",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentSlots_AspNetUsers_UserId",
                table: "AppointmentSlots");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "AppointmentSlots",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentSlots_AspNetUsers_UserId",
                table: "AppointmentSlots",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
