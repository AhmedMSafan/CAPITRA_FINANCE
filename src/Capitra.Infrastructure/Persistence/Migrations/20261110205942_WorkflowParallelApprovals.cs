using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowParallelApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_WFE_TASKS_STEP_INSTANCE_ID",
                table: "WFE_TASKS");

            migrationBuilder.CreateIndex(
                name: "IX_WFE_TASKS_STEP_INSTANCE_ID",
                table: "WFE_TASKS",
                column: "STEP_INSTANCE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WFE_TASKS_STEP_INSTANCE_ID",
                table: "WFE_TASKS");

            migrationBuilder.CreateIndex(
                name: "UQ_WFE_TASKS_STEP_INSTANCE_ID",
                table: "WFE_TASKS",
                column: "STEP_INSTANCE_ID",
                unique: true);
        }
    }
}
