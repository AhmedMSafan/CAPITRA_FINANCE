using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TreasuryBankLimits : Migration
    {
        /// <summary>Every facility that is not closed gets one ACTIVE tranche for its whole limit and one General sublimit (every business
        /// line), and its outstanding and reserved balances are carried in as OPENING movements, so the tree shows today's usage.</summary>
        internal const string DefaultTreeSql = """
            INSERT INTO "TRS_FACILITY_TRANCHES" ("FACILITY_ID", "TRANCHE_NAME", "TRANCHE_AMOUNT", "STATUS_CODE", "RELEASED_DATE", "MATURITY_DATE", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
            SELECT f."FACILITY_ID", 'Tranche 1', f."APPROVED_LIMIT", 'ACTIVE', f."START_DATE", f."MATURITY_DATE", 'Whole facility (carried over)', 'MIGRATION', now(), 0
            FROM "TRS_CREDIT_FACILITIES" f
            WHERE f."FACILITY_STATUS" <> 'CLOSED' AND f."APPROVED_LIMIT" > 0;

            INSERT INTO "TRS_FACILITY_SUBLIMITS" ("FACILITY_ID", "TRANCHE_ID", "SUBLIMIT_NAME", "LIMIT_AMOUNT", "POOL_TYPE_CODE", "STATUS_CODE", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
            SELECT t."FACILITY_ID", t."TRANCHE_ID", 'General', t."TRANCHE_AMOUNT", 'DEDICATED', 'ACTIVE', 'All business lines (carried over)', 'MIGRATION', now(), 0
            FROM "TRS_FACILITY_TRANCHES" t;

            INSERT INTO "TRS_SUBLIMIT_MOVEMENTS" ("SUBLIMIT_ID", "MOVEMENT_TYPE_CODE", "MOVEMENT_DATE", "OUTSTANDING_CHANGE", "RESERVED_CHANGE", "NOTES", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
            SELECT s."SUBLIMIT_ID", 'OPENING', CURRENT_DATE, LEAST(f."OUTSTANDING_BALANCE", s."LIMIT_AMOUNT"), COALESCE(f."RESERVED_BALANCE", 0), 'Facility balance carried over', 'MIGRATION', now(), 0
            FROM "TRS_FACILITY_SUBLIMITS" s
            JOIN "TRS_CREDIT_FACILITIES" f ON f."FACILITY_ID" = s."FACILITY_ID"
            WHERE f."OUTSTANDING_BALANCE" > 0 OR COALESCE(f."RESERVED_BALANCE", 0) > 0;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TRS_BANK_RULES",
                columns: table => new
                {
                    RULE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BANK_ID = table.Column<long>(type: "bigint", nullable: false),
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: true),
                    RULE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SUBJECT_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SUBJECT_CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SUBJECT_VALUE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LIMIT_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    LIMIT_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    WARNING_PCT = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MAX_TENOR_DAYS = table.Column<int>(type: "integer", nullable: true),
                    BASIS_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BREACH_ACTION_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CHANGED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CHANGED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VERIFIED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VERIFIED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_BANK_RULES", x => x.RULE_ID);
                    table.ForeignKey(
                        name: "FK_TRS_BANK_RULES_CRM_CLIENTS",
                        column: x => x.SUBJECT_CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_BANK_RULES_TRS_BANKS",
                        column: x => x.BANK_ID,
                        principalTable: "TRS_BANKS",
                        principalColumn: "BANK_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_BANK_RULES_TRS_CREDIT_FACILITIES",
                        column: x => x.FACILITY_ID,
                        principalTable: "TRS_CREDIT_FACILITIES",
                        principalColumn: "FACILITY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_FACILITY_TRANCHES",
                columns: table => new
                {
                    TRANCHE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    TRANCHE_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TRANCHE_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RELEASED_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    MATURITY_DATE = table.Column<DateOnly>(type: "date", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_FACILITY_TRANCHES", x => x.TRANCHE_ID);
                    table.ForeignKey(
                        name: "FK_TRS_FACILITY_TRANCHES_TRS_CREDIT_FACILITIES",
                        column: x => x.FACILITY_ID,
                        principalTable: "TRS_CREDIT_FACILITIES",
                        principalColumn: "FACILITY_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_FACILITY_SUBLIMITS",
                columns: table => new
                {
                    SUBLIMIT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FACILITY_ID = table.Column<long>(type: "bigint", nullable: false),
                    TRANCHE_ID = table.Column<long>(type: "bigint", nullable: false),
                    SUBLIMIT_NAME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LIMIT_AMOUNT = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    POOL_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    STATUS_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_FACILITY_SUBLIMITS", x => x.SUBLIMIT_ID);
                    table.ForeignKey(
                        name: "FK_TRS_FACILITY_SUBLIMITS_TRS_CREDIT_FACILITIES",
                        column: x => x.FACILITY_ID,
                        principalTable: "TRS_CREDIT_FACILITIES",
                        principalColumn: "FACILITY_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_FACILITY_SUBLIMITS_TRS_FACILITY_TRANCHES",
                        column: x => x.TRANCHE_ID,
                        principalTable: "TRS_FACILITY_TRANCHES",
                        principalColumn: "TRANCHE_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_SUBLIMIT_MOVEMENTS",
                columns: table => new
                {
                    MOVEMENT_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SUBLIMIT_ID = table.Column<long>(type: "bigint", nullable: false),
                    MOVEMENT_TYPE_CODE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MOVEMENT_DATE = table.Column<DateOnly>(type: "date", nullable: false),
                    OUTSTANDING_CHANGE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    RESERVED_CHANGE = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    DISBURSEMENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    DEBTOR_CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    SELLER_CLIENT_ID = table.Column<long>(type: "bigint", nullable: true),
                    NOTES = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_SUBLIMIT_MOVEMENTS", x => x.MOVEMENT_ID);
                    table.ForeignKey(
                        name: "FK_TRS_SUBLIMIT_MOVEMENTS_CRM_CLIENTS_DEBTOR_CLIENT_ID",
                        column: x => x.DEBTOR_CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_SUBLIMIT_MOVEMENTS_CRM_CLIENTS_SELLER_CLIENT_ID",
                        column: x => x.SELLER_CLIENT_ID,
                        principalTable: "CRM_CLIENTS",
                        principalColumn: "CLIENT_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRS_SUBLIMIT_MOVEMENTS_TRS_FACILITY_SUBLIMITS",
                        column: x => x.SUBLIMIT_ID,
                        principalTable: "TRS_FACILITY_SUBLIMITS",
                        principalColumn: "SUBLIMIT_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRS_SUBLIMIT_SCOPES",
                columns: table => new
                {
                    SCOPE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SUBLIMIT_ID = table.Column<long>(type: "bigint", nullable: false),
                    SCOPE_TYPE_CODE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SCOPE_VALUE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRS_SUBLIMIT_SCOPES", x => x.SCOPE_ID);
                    table.ForeignKey(
                        name: "FK_TRS_SUBLIMIT_SCOPES_TRS_FACILITY_SUBLIMITS",
                        column: x => x.SUBLIMIT_ID,
                        principalTable: "TRS_FACILITY_SUBLIMITS",
                        principalColumn: "SUBLIMIT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANK_RULES_BANK_ID_STATUS_CODE",
                table: "TRS_BANK_RULES",
                columns: new[] { "BANK_ID", "STATUS_CODE" });

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANK_RULES_FACILITY_ID",
                table: "TRS_BANK_RULES",
                column: "FACILITY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_BANK_RULES_SUBJECT_CLIENT_ID",
                table: "TRS_BANK_RULES",
                column: "SUBJECT_CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_FACILITY_SUBLIMITS_FACILITY_ID",
                table: "TRS_FACILITY_SUBLIMITS",
                column: "FACILITY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_FACILITY_SUBLIMITS_TRANCHE_ID_SUBLIMIT_NAME",
                table: "TRS_FACILITY_SUBLIMITS",
                columns: new[] { "TRANCHE_ID", "SUBLIMIT_NAME" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_FACILITY_TRANCHES_FACILITY_ID_TRANCHE_NAME",
                table: "TRS_FACILITY_TRANCHES",
                columns: new[] { "FACILITY_ID", "TRANCHE_NAME" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUBLIMIT_MOVEMENTS_DEBTOR_CLIENT_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS",
                column: "DEBTOR_CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUBLIMIT_MOVEMENTS_DISBURSEMENT_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS",
                column: "DISBURSEMENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUBLIMIT_MOVEMENTS_SELLER_CLIENT_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS",
                column: "SELLER_CLIENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TRS_SUBLIMIT_MOVEMENTS_SUBLIMIT_ID",
                table: "TRS_SUBLIMIT_MOVEMENTS",
                column: "SUBLIMIT_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_TRS_SUBLIMIT_SCOPES_SUBLIMIT_ID_SCOPE_TYPE_CODE_SCOPE_VALUE",
                table: "TRS_SUBLIMIT_SCOPES",
                columns: new[] { "SUBLIMIT_ID", "SCOPE_TYPE_CODE", "SCOPE_VALUE" },
                unique: true);

            migrationBuilder.Sql(DefaultTreeSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TRS_BANK_RULES");

            migrationBuilder.DropTable(
                name: "TRS_SUBLIMIT_MOVEMENTS");

            migrationBuilder.DropTable(
                name: "TRS_SUBLIMIT_SCOPES");

            migrationBuilder.DropTable(
                name: "TRS_FACILITY_SUBLIMITS");

            migrationBuilder.DropTable(
                name: "TRS_FACILITY_TRANCHES");
        }
    }
}
