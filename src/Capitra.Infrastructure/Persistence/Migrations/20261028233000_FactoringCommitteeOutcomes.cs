using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Committee Approval in FACTORING_LIMIT_APPROVAL had two unnamed exits, so it always went on to Client Acceptance.
    /// It now has ACCEPT (to Client Acceptance) and REJECT (ends the workflow), as Ahmed confirmed, and offers both actions.
    /// The seed migration carries the same change for new databases, so this update does nothing there.
    /// </summary>
    [DbContext(typeof(CapitraDbContext))]
    [Migration("20261028233000_FactoringCommitteeOutcomes")]
    public partial class FactoringCommitteeOutcomes : Migration
    {
        private static readonly (string Before, string After)[] Changes =
        [
            ("""finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">""",
             """finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">"""),
            ("""<bpmn:sequenceFlow id="Flow_0qv4imo" name="" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="" />""",
             """<bpmn:sequenceFlow id="Flow_0qv4imo" name="ACCEPT" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="ACCEPT" />"""),
            ("""<bpmn:sequenceFlow id="Flow_1bcnv9y" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" />""",
             """<bpmn:sequenceFlow id="Flow_1bcnv9y" name="REJECT" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" finova:outcome="REJECT" />""")
        ];

        // one nested replace(), since an UPDATE can set a column only once
        private static string Sql(bool up)
        {
            var value = "\"BPMN_XML\"";
            foreach (var (before, after) in Changes)
                value = up ? $"replace({value}, '{before}', '{after}')" : $"replace({value}, '{after}', '{before}')";
            return $"""UPDATE "WFE_WORKFLOW_DEFINITIONS" SET "BPMN_XML" = {value} WHERE "WORKFLOW_CODE" = 'FACTORING_LIMIT_APPROVAL';""";
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Sql(up: true));

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Sql(up: false));
    }
}
