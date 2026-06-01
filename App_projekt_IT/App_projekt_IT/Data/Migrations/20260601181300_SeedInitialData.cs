using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App_projekt_IT.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Name", "Voivodeship" },
                values: new object[,]
                {
                    { 1, "Kraków", "Małopolskie" },
                    { 2, "Warszawa", "Mazowieckie" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "IsNFZ", "Name" },
                values: new object[,]
                {
                    { 1, true, "Konsultacja kardiologiczna" },
                    { 2, false, "Rezonans magnetyczny" },
                    { 3, true, "Konsultacja ortopedyczna" }
                });

            migrationBuilder.InsertData(
                table: "Clinics",
                columns: new[] { "Id", "Address", "CityId", "Email", "Name", "Phone", "PostalCode" },
                values: new object[,]
                {
                    { 1, "ul. Karmelicka 10", 1, "kontakt@zdrowie.pl", "Centrum Medyczne Zdrowie", "123456789", "31-128" },
                    { 2, "ul. Nowy Świat 5", 2, "biuro@lecznica.pl", "Prywatna Lecznica", "987654321", "00-029" }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "ClinicId", "FirstName", "LastName", "Title" },
                values: new object[,]
                {
                    { 1, 1, "Jan", "Kowalski", "lek. med." },
                    { 2, 1, "Anna", "Nowak", "dr n. med." },
                    { 3, 2, "Piotr", "Wiśniewski", "lek. med." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
