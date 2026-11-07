using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WFE_WORKFLOW_DEFINITIONS",
                columns: table => new
                {
                    WORKFLOW_DEFINITION_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKFLOW_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WORKFLOW_NAME = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    VERSION_NO = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DEFINITION_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BPMN_XML = table.Column<string>(type: "text", nullable: false),
                    DESCRIPTION = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_WORKFLOW_DEFINITIONS", x => x.WORKFLOW_DEFINITION_ID);
                });

            migrationBuilder.CreateTable(
                name: "WFE_WORKFLOW_INSTANCES",
                columns: table => new
                {
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKFLOW_DEFINITION_ID = table.Column<long>(type: "bigint", nullable: false),
                    INSTANCE_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BUSINESS_REFERENCE = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CURRENT_ELEMENT_ID = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    STARTED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    STARTED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ERROR_MESSAGE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_WORKFLOW_INSTANCES", x => x.WORKFLOW_INSTANCE_ID);
                    table.ForeignKey(
                        name: "FK_WFE_WORKFLOW_INSTANCES_WFE_WORKFLOW_DEFINITIONS",
                        column: x => x.WORKFLOW_DEFINITION_ID,
                        principalTable: "WFE_WORKFLOW_DEFINITIONS",
                        principalColumn: "WORKFLOW_DEFINITION_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WFE_INSTANCE_HIST",
                columns: table => new
                {
                    INSTANCE_HIST_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    EVENT_TYPE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ELEMENT_ID = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ACTION_CODE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PERFORMED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DETAILS = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EVENT_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_INSTANCE_HIST", x => x.INSTANCE_HIST_ID);
                    table.ForeignKey(
                        name: "FK_WFE_INSTANCE_HIST_WFE_WORKFLOW_INSTANCES",
                        column: x => x.WORKFLOW_INSTANCE_ID,
                        principalTable: "WFE_WORKFLOW_INSTANCES",
                        principalColumn: "WORKFLOW_INSTANCE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFE_INSTANCE_VARIABLES",
                columns: table => new
                {
                    INSTANCE_VARIABLE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    VARIABLE_NAME = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    VARIABLE_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VARIABLE_VALUE = table.Column<string>(type: "text", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_INSTANCE_VARIABLES", x => x.INSTANCE_VARIABLE_ID);
                    table.ForeignKey(
                        name: "FK_WFE_INSTANCE_VARIABLES_WFE_WORKFLOW_INSTANCES",
                        column: x => x.WORKFLOW_INSTANCE_ID,
                        principalTable: "WFE_WORKFLOW_INSTANCES",
                        principalColumn: "WORKFLOW_INSTANCE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFE_STEP_INSTANCES",
                columns: table => new
                {
                    STEP_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    ELEMENT_ID = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    STEP_CODE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    STEP_NAME = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ELEMENT_TYPE = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    STEP_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ENTERED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OUTCOME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_STEP_INSTANCES", x => x.STEP_INSTANCE_ID);
                    table.ForeignKey(
                        name: "FK_WFE_STEP_INSTANCES_WFE_WORKFLOW_INSTANCES",
                        column: x => x.WORKFLOW_INSTANCE_ID,
                        principalTable: "WFE_WORKFLOW_INSTANCES",
                        principalColumn: "WORKFLOW_INSTANCE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFE_TASKS",
                columns: table => new
                {
                    TASK_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    STEP_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    TASK_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ASSIGNMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ASSIGNEE = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CLAIMED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CLAIMED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROUTE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ROUTE_PARAMETERS = table.Column<string>(type: "text", nullable: true),
                    ALLOWED_ACTIONS = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_TASKS", x => x.TASK_ID);
                    table.ForeignKey(
                        name: "FK_WFE_TASKS_WFE_STEP_INSTANCES",
                        column: x => x.STEP_INSTANCE_ID,
                        principalTable: "WFE_STEP_INSTANCES",
                        principalColumn: "STEP_INSTANCE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WFE_INBOX",
                columns: table => new
                {
                    INBOX_ITEM_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TASK_ID = table.Column<long>(type: "bigint", nullable: false),
                    WORKFLOW_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    STEP_INSTANCE_ID = table.Column<long>(type: "bigint", nullable: false),
                    INBOX_STATUS = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ASSIGNMENT_TYPE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ASSIGNED_TO = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ASSIGNED_ROLE = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CLAIMED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SUBJECT = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ROUTE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ROUTE_PARAMETERS = table.Column<string>(type: "text", nullable: true),
                    ALLOWED_ACTIONS = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OUTCOME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CLAIMED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    COMPLETED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CREATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_BY = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ROW_VERSION = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WFE_INBOX", x => x.INBOX_ITEM_ID);
                    table.ForeignKey(
                        name: "FK_WFE_INBOX_WFE_STEP_INSTANCES",
                        column: x => x.STEP_INSTANCE_ID,
                        principalTable: "WFE_STEP_INSTANCES",
                        principalColumn: "STEP_INSTANCE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFE_INBOX_WFE_TASKS",
                        column: x => x.TASK_ID,
                        principalTable: "WFE_TASKS",
                        principalColumn: "TASK_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WFE_INBOX_WFE_WORKFLOW_INSTANCES",
                        column: x => x.WORKFLOW_INSTANCE_ID,
                        principalTable: "WFE_WORKFLOW_INSTANCES",
                        principalColumn: "WORKFLOW_INSTANCE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_INBOX_INBOX_STATUS_ASSIGNED_ROLE",
                table: "WFE_INBOX",
                columns: new[] { "INBOX_STATUS", "ASSIGNED_ROLE" });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_INBOX_INBOX_STATUS_ASSIGNED_TO",
                table: "WFE_INBOX",
                columns: new[] { "INBOX_STATUS", "ASSIGNED_TO" });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_INBOX_STEP_INSTANCE_ID",
                table: "WFE_INBOX",
                column: "STEP_INSTANCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WFE_INBOX_WORKFLOW_INSTANCE_ID",
                table: "WFE_INBOX",
                column: "WORKFLOW_INSTANCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_INBOX_TASK_ID",
                table: "WFE_INBOX",
                column: "TASK_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WFE_INSTANCE_HIST_WORKFLOW_INSTANCE_ID",
                table: "WFE_INSTANCE_HIST",
                column: "WORKFLOW_INSTANCE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_INSTANCE_VARIABLES_WORKFLOW_INSTANCE_ID_VARIABLE_NAME",
                table: "WFE_INSTANCE_VARIABLES",
                columns: new[] { "WORKFLOW_INSTANCE_ID", "VARIABLE_NAME" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WFE_STEP_INSTANCES_WORKFLOW_INSTANCE_ID_ELEMENT_ID",
                table: "WFE_STEP_INSTANCES",
                columns: new[] { "WORKFLOW_INSTANCE_ID", "ELEMENT_ID" });

            migrationBuilder.CreateIndex(
                name: "IX_WFE_TASKS_TASK_STATUS",
                table: "WFE_TASKS",
                column: "TASK_STATUS");

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_TASKS_STEP_INSTANCE_ID",
                table: "WFE_TASKS",
                column: "STEP_INSTANCE_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_WORKFLOW_DEFINITIONS_WORKFLOW_CODE",
                table: "WFE_WORKFLOW_DEFINITIONS",
                column: "WORKFLOW_CODE",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WFE_WORKFLOW_INSTANCES_BUSINESS_REFERENCE",
                table: "WFE_WORKFLOW_INSTANCES",
                column: "BUSINESS_REFERENCE");

            migrationBuilder.CreateIndex(
                name: "IX_WFE_WORKFLOW_INSTANCES_WORKFLOW_DEFINITION_ID",
                table: "WFE_WORKFLOW_INSTANCES",
                column: "WORKFLOW_DEFINITION_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WFE_INBOX");

            migrationBuilder.DropTable(
                name: "WFE_INSTANCE_HIST");

            migrationBuilder.DropTable(
                name: "WFE_INSTANCE_VARIABLES");

            migrationBuilder.DropTable(
                name: "WFE_TASKS");

            migrationBuilder.DropTable(
                name: "WFE_STEP_INSTANCES");

            migrationBuilder.DropTable(
                name: "WFE_WORKFLOW_INSTANCES");

            migrationBuilder.DropTable(
                name: "WFE_WORKFLOW_DEFINITIONS");
        }
    }
}
