using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompanyLoginsTemporaryPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IS_PASSWORD_CHANGE_REQUIRED",
                table: "SEC_USERS",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PASSWORD_CHANGED_AT",
                table: "SEC_USERS",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IS_PASSWORD_CHANGE_REQUIRED",
                table: "SEC_USERS");

            migrationBuilder.DropColumn(
                name: "PASSWORD_CHANGED_AT",
                table: "SEC_USERS");
        }
    }
}
