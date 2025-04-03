using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5cfaa6d8-708a-4003-85db-da9d1b64b855");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "890449e2-158f-4bf6-8a69-ffc5bfe8d1ba");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f62e2a39-d165-4699-87f5-8c03086783d4");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "76882e2f-7111-41bf-94c3-5d4c8f83250e", "2", "User", "User" },
                    { "ecd4f9f8-e91c-4dea-b3de-46aca025fc93", "1", "Admin", "Admin" },
                    { "fdfb20b5-6875-41f9-bcbc-1fb327501853", "3", "Dentist", "Dentist" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "76882e2f-7111-41bf-94c3-5d4c8f83250e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ecd4f9f8-e91c-4dea-b3de-46aca025fc93");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fdfb20b5-6875-41f9-bcbc-1fb327501853");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5cfaa6d8-708a-4003-85db-da9d1b64b855", "3", "Dentist", "Dentist" },
                    { "890449e2-158f-4bf6-8a69-ffc5bfe8d1ba", "2", "User", "User" },
                    { "f62e2a39-d165-4699-87f5-8c03086783d4", "1", "Admin", "Admin" }
                });
        }
    }
}
