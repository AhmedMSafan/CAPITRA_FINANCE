using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADM_LKP_COUNTRY",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_COUNTRY", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_CURRENCY",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_CURRENCY", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "ADM_LKP_RISK_LEVEL",
                columns: table => new
                {
                    CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    NAME_AR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NAME_EN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DISPLAY_ORDER = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADM_LKP_RISK_LEVEL", x => x.CODE);
                });

            migrationBuilder.CreateTable(
                name: "SEC_ROLE",
                columns: table => new
                {
                    ROLE_ID = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    ROLE_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NORMALIZED_ROLE_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CONCURRENCY_STAMP = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_ROLE", x => x.ROLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "SEC_USER",
                columns: table => new
                {
                    USER_ID = table.Column<string>(type: "text", nullable: false),
                    FULL_NAME = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EMPLOYEE_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DEPARTMENT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    JOB_TITLE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LAST_LOGIN_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NORMALIZED_USER_NAME = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EMAIL = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NORMALIZED_EMAIL = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IS_EMAIL_CONFIRMED = table.Column<bool>(type: "boolean", nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "text", nullable: true),
                    SECURITY_STAMP = table.Column<string>(type: "text", nullable: true),
                    CONCURRENCY_STAMP = table.Column<string>(type: "text", nullable: true),
                    PHONE_NO = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IS_PHONE_NO_CONFIRMED = table.Column<bool>(type: "boolean", nullable: false),
                    IS_TWO_FACTOR_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    LOCKOUT_END_AT = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IS_LOCKOUT_ENABLED = table.Column<bool>(type: "boolean", nullable: false),
                    ACCESS_FAILED_COUNT = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_USER", x => x.USER_ID);
                });

            migrationBuilder.CreateTable(
                name: "SEC_ROLE_CLAIM",
                columns: table => new
                {
                    ROLE_CLAIM_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    ROLE_ID = table.Column<string>(type: "text", nullable: false),
                    CLAIM_TYPE = table.Column<string>(type: "text", nullable: true),
                    CLAIM_VALUE = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_ROLE_CLAIM", x => x.ROLE_CLAIM_ID);
                    table.ForeignKey(
                        name: "FK_SEC_ROLE_CLAIM_SEC_ROLE",
                        column: x => x.ROLE_ID,
                        principalTable: "SEC_ROLE",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SEC_USER_CLAIM",
                columns: table => new
                {
                    USER_CLAIM_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    USER_ID = table.Column<string>(type: "text", nullable: false),
                    CLAIM_TYPE = table.Column<string>(type: "text", nullable: true),
                    CLAIM_VALUE = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_USER_CLAIM", x => x.USER_CLAIM_ID);
                    table.ForeignKey(
                        name: "FK_SEC_USER_CLAIM_SEC_USER",
                        column: x => x.USER_ID,
                        principalTable: "SEC_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SEC_USER_LOGIN",
                columns: table => new
                {
                    LOGIN_PROVIDER = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PROVIDER_KEY = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    PROVIDER_DISPLAY_NAME = table.Column<string>(type: "text", nullable: true),
                    USER_ID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_USER_LOGIN", x => new { x.LOGIN_PROVIDER, x.PROVIDER_KEY });
                    table.ForeignKey(
                        name: "FK_SEC_USER_LOGIN_SEC_USER",
                        column: x => x.USER_ID,
                        principalTable: "SEC_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SEC_USER_PASSKEY",
                columns: table => new
                {
                    CREDENTIAL_ID = table.Column<byte[]>(type: "bytea", maxLength: 1024, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    USER_ID = table.Column<string>(type: "text", nullable: false),
                    PASSKEY_DATA = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_USER_PASSKEY", x => x.CREDENTIAL_ID);
                    table.ForeignKey(
                        name: "FK_SEC_USER_PASSKEY_SEC_USER",
                        column: x => x.USER_ID,
                        principalTable: "SEC_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SEC_USER_ROLE",
                columns: table => new
                {
                    USER_ID = table.Column<string>(type: "text", nullable: false),
                    ROLE_ID = table.Column<string>(type: "text", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_USER_ROLE", x => new { x.USER_ID, x.ROLE_ID });
                    table.ForeignKey(
                        name: "FK_SEC_USER_ROLE_SEC_ROLE",
                        column: x => x.ROLE_ID,
                        principalTable: "SEC_ROLE",
                        principalColumn: "ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SEC_USER_ROLE_SEC_USER",
                        column: x => x.USER_ID,
                        principalTable: "SEC_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SEC_USER_TOKEN",
                columns: table => new
                {
                    USER_ID = table.Column<string>(type: "text", nullable: false),
                    LOGIN_PROVIDER = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TOKEN_NAME = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false),
                    TOKEN_VALUE = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEC_USER_TOKEN", x => new { x.USER_ID, x.LOGIN_PROVIDER, x.TOKEN_NAME });
                    table.ForeignKey(
                        name: "FK_SEC_USER_TOKEN_SEC_USER",
                        column: x => x.USER_ID,
                        principalTable: "SEC_USER",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_SEC_ROLE_NORMALIZED_ROLE_NAME",
                table: "SEC_ROLE",
                column: "NORMALIZED_ROLE_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SEC_ROLE_CLAIM_ROLE_ID",
                table: "SEC_ROLE_CLAIM",
                column: "ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_USER_NORMALIZED_EMAIL",
                table: "SEC_USER",
                column: "NORMALIZED_EMAIL");

            migrationBuilder.CreateIndex(
                name: "UQ_SEC_USER_NORMALIZED_USER_NAME",
                table: "SEC_USER",
                column: "NORMALIZED_USER_NAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SEC_USER_CLAIM_USER_ID",
                table: "SEC_USER_CLAIM",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_USER_LOGIN_USER_ID",
                table: "SEC_USER_LOGIN",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_USER_PASSKEY_USER_ID",
                table: "SEC_USER_PASSKEY",
                column: "USER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SEC_USER_ROLE_ROLE_ID",
                table: "SEC_USER_ROLE",
                column: "ROLE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADM_LKP_COUNTRY");

            migrationBuilder.DropTable(
                name: "ADM_LKP_CURRENCY");

            migrationBuilder.DropTable(
                name: "ADM_LKP_RISK_LEVEL");

            migrationBuilder.DropTable(
                name: "SEC_ROLE_CLAIM");

            migrationBuilder.DropTable(
                name: "SEC_USER_CLAIM");

            migrationBuilder.DropTable(
                name: "SEC_USER_LOGIN");

            migrationBuilder.DropTable(
                name: "SEC_USER_PASSKEY");

            migrationBuilder.DropTable(
                name: "SEC_USER_ROLE");

            migrationBuilder.DropTable(
                name: "SEC_USER_TOKEN");

            migrationBuilder.DropTable(
                name: "SEC_ROLE");

            migrationBuilder.DropTable(
                name: "SEC_USER");
        }
    }
}
