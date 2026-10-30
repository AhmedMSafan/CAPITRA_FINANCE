using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductRuleJournalSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ahmed, 30 Oct 2026: products no longer go through business events (GL_ACCOUNTING_EVENT_TYPES). A product event
            // rule carries its own journal source and category, and a workflow step points at the rule itself (finova:ruleCode).
            // GL_ACCOUNTING_EVENT_TYPES stays: the GL module's auto-accounting still uses it.
            migrationBuilder.DropForeignKey(
                name: "FK_ADM_PRODUCT_EVENT_RULES_GL_ACCOUNTING_EVENT_TYPES",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULES_PRODUCT_ID_EVENT_TYPE_ID_RULE_CODE",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_EVENT_TYPE_ID",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.AddColumn<long>(
                name: "JOURNAL_SOURCE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "JOURNAL_CATEGORY_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                type: "bigint",
                nullable: true);

            // the source and category the rule's business event gave; a category of that source, or the first category, when the event had none
            migrationBuilder.Sql("""
                UPDATE "ADM_PRODUCT_EVENT_RULES" r
                   SET "JOURNAL_SOURCE_ID" = t."SOURCE_ID",
                       "JOURNAL_CATEGORY_ID" = COALESCE(t."DEFAULT_CATEGORY_ID",
                           (SELECT c."CATEGORY_ID" FROM "GL_JOURNAL_CATEGORIES" c WHERE c."DEFAULT_SOURCE_ID" = t."SOURCE_ID" ORDER BY c."CATEGORY_ID" LIMIT 1),
                           (SELECT c."CATEGORY_ID" FROM "GL_JOURNAL_CATEGORIES" c ORDER BY c."CATEGORY_ID" LIMIT 1))
                  FROM "GL_ACCOUNTING_EVENT_TYPES" t
                 WHERE t."EVENT_TYPE_ID" = r."EVENT_TYPE_ID";
                """);

            // a rule code is now unique per product (it was per product and event): a repeated code gets the rule ID added
            migrationBuilder.Sql("""
                UPDATE "ADM_PRODUCT_EVENT_RULES" r
                   SET "RULE_CODE" = left(r."RULE_CODE", 40) || '_' || r."PRODUCT_EVENT_RULE_ID"
                 WHERE EXISTS (SELECT 1 FROM "ADM_PRODUCT_EVENT_RULES" o
                                WHERE o."PRODUCT_ID" = r."PRODUCT_ID" AND o."RULE_CODE" = r."RULE_CODE" AND o."PRODUCT_EVENT_RULE_ID" < r."PRODUCT_EVENT_RULE_ID");
                """);

            // workflow steps: finova:eventCode="<event>" becomes finova:ruleCode="<the rule of that event>" (the purchased portfolio
            // product's rule first, then the lowest priority); an event with no rule is dropped from the step
            migrationBuilder.Sql("""
                DO $$
                DECLARE x record;
                BEGIN
                  FOR x IN SELECT DISTINCT ON (t."EVENT_TYPE_CODE") t."EVENT_TYPE_CODE" AS ev, r."RULE_CODE" AS rule
                             FROM "ADM_PRODUCT_EVENT_RULES" r
                             JOIN "GL_ACCOUNTING_EVENT_TYPES" t ON t."EVENT_TYPE_ID" = r."EVENT_TYPE_ID"
                             JOIN "ADM_PRODUCTS" p ON p."PRODUCT_ID" = r."PRODUCT_ID"
                            ORDER BY t."EVENT_TYPE_CODE", (p."PRODUCT_CODE" = 'MTG_PORTFOLIO_PURCHASING') DESC, r."IS_ACTIVE" DESC, r."PRIORITY_NO", r."PRODUCT_EVENT_RULE_ID"
                  LOOP
                    UPDATE "WFE_WORKFLOW_DEFINITIONS"
                       SET "BPMN_XML" = replace("BPMN_XML", 'finova:eventCode="' || x.ev || '"', 'finova:ruleCode="' || x.rule || '"')
                     WHERE "BPMN_XML" LIKE '%finova:eventCode="' || x.ev || '"%';
                  END LOOP;
                  UPDATE "WFE_WORKFLOW_DEFINITIONS"
                     SET "BPMN_XML" = regexp_replace("BPMN_XML", '\s+finova:eventCode="[^"]*"', '', 'g')
                   WHERE "BPMN_XML" LIKE '%finova:eventCode=%';
                END $$;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "JOURNAL_SOURCE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "JOURNAL_CATEGORY_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "EVENT_TYPE_ID",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_JOURNAL_SOURCE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "JOURNAL_SOURCE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_JOURNAL_CATEGORY_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "JOURNAL_CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULES_PRODUCT_ID_RULE_CODE",
                table: "ADM_PRODUCT_EVENT_RULES",
                columns: new[] { "PRODUCT_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ADM_PRODUCT_EVENT_RULES_GL_JOURNAL_CATEGORIES",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "JOURNAL_CATEGORY_ID",
                principalTable: "GL_JOURNAL_CATEGORIES",
                principalColumn: "CATEGORY_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ADM_PRODUCT_EVENT_RULES_GL_JOURNAL_SOURCES",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "JOURNAL_SOURCE_ID",
                principalTable: "GL_JOURNAL_SOURCES",
                principalColumn: "SOURCE_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ADM_PRODUCT_EVENT_RULES_GL_JOURNAL_CATEGORIES",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropForeignKey(
                name: "FK_ADM_PRODUCT_EVENT_RULES_GL_JOURNAL_SOURCES",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_JOURNAL_CATEGORY_ID",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_JOURNAL_SOURCE_ID",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULES_PRODUCT_ID_RULE_CODE",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.AddColumn<long>(
                name: "EVENT_TYPE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                type: "bigint",
                nullable: true);

            // back to a business event: the one with the rule's code, else one of the rule's source
            migrationBuilder.Sql("""
                UPDATE "ADM_PRODUCT_EVENT_RULES" r
                   SET "EVENT_TYPE_ID" = COALESCE(
                       (SELECT t."EVENT_TYPE_ID" FROM "GL_ACCOUNTING_EVENT_TYPES" t WHERE t."EVENT_TYPE_CODE" = r."RULE_CODE"),
                       (SELECT t."EVENT_TYPE_ID" FROM "GL_ACCOUNTING_EVENT_TYPES" t WHERE t."SOURCE_ID" = r."JOURNAL_SOURCE_ID" ORDER BY t."EVENT_TYPE_ID" LIMIT 1),
                       (SELECT t."EVENT_TYPE_ID" FROM "GL_ACCOUNTING_EVENT_TYPES" t ORDER BY t."EVENT_TYPE_ID" LIMIT 1));
                UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = replace("BPMN_XML", 'finova:ruleCode=', 'finova:eventCode=')
                 WHERE "BPMN_XML" LIKE '%finova:ruleCode=%';
                """);

            migrationBuilder.AlterColumn<long>(
                name: "EVENT_TYPE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "JOURNAL_CATEGORY_ID",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.DropColumn(
                name: "JOURNAL_SOURCE_ID",
                table: "ADM_PRODUCT_EVENT_RULES");

            migrationBuilder.CreateIndex(
                name: "IX_ADM_PRODUCT_EVENT_RULES_EVENT_TYPE_ID",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "EVENT_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_ADM_PRODUCT_EVENT_RULES_PRODUCT_ID_EVENT_TYPE_ID_RULE_CODE",
                table: "ADM_PRODUCT_EVENT_RULES",
                columns: new[] { "PRODUCT_ID", "EVENT_TYPE_ID", "RULE_CODE" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ADM_PRODUCT_EVENT_RULES_GL_ACCOUNTING_EVENT_TYPES",
                table: "ADM_PRODUCT_EVENT_RULES",
                column: "EVENT_TYPE_ID",
                principalTable: "GL_ACCOUNTING_EVENT_TYPES",
                principalColumn: "EVENT_TYPE_ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
