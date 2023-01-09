using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.Infrastructure.Migrations
{
    public partial class UpdateRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c48f6b17-2381-44dd-afeb-6d3bcde90af6");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "10b35762-143e-423b-81b7-122e1ed81260", "ab7da443-028f-42bf-b077-552c9154e69b", "cso", "CSO" },
                    { "29281259-b319-4fd2-850b-1f2becf8213f", "913f82a1-889d-4766-80aa-0f7bef679322", "staff", "STAFF" },
                    { "7940e6b0-da81-4fa8-8084-da36fa601b89", "5a3219e7-0bee-4f4e-a360-f80f13418c9a", "editor", "EDITOR" },
                    { "7b247da1-9bb7-487f-bf99-d010d62c9284", "6c322a46-015b-4c98-bef2-bbaf2c6de314", "qc", "QC" },
                    { "8bc5760d-c597-458f-8c14-ddf3377c10c6", "6d82c363-ebf3-42eb-8c3b-6aa19265ca47", "admin", "ADMIN" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "10b35762-143e-423b-81b7-122e1ed81260");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "29281259-b319-4fd2-850b-1f2becf8213f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7940e6b0-da81-4fa8-8084-da36fa601b89");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7b247da1-9bb7-487f-bf99-d010d62c9284");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8bc5760d-c597-458f-8c14-ddf3377c10c6");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "c48f6b17-2381-44dd-afeb-6d3bcde90af6", "24da78ed-98a4-45a7-97fa-7920f32e4da9", "consumer", "CONSUMER" });
        }
    }
}
