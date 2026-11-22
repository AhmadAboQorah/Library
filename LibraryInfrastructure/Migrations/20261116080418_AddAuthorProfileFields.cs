using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Authors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Authors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Authors");
        }
    }
}
