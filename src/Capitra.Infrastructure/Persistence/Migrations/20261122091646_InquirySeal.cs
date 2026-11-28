using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capitra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InquirySeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PREV_HASH",
                table: "SCR_INQUIRIES",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RECORD_HASH",
                table: "SCR_INQUIRIES",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // screening inquiries are evidence: never deleted, and never changed once sealed (their hits neither)
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION "SCR_INQUIRIES_GUARD"() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                  IF TG_OP = 'DELETE' THEN
                    RAISE EXCEPTION 'Screening inquiry % cannot be deleted: inquiries are kept as evidence.', OLD."INQUIRY_REF";
                  END IF;
                  IF OLD."RECORD_HASH" IS NOT NULL THEN
                    RAISE EXCEPTION 'Screening inquiry % is sealed and cannot be changed.', OLD."INQUIRY_REF";
                  END IF;
                  RETURN NEW;
                END $$;
                CREATE TRIGGER "TRG_SCR_INQUIRIES_GUARD" BEFORE UPDATE OR DELETE ON "SCR_INQUIRIES"
                  FOR EACH ROW EXECUTE FUNCTION "SCR_INQUIRIES_GUARD"();

                CREATE OR REPLACE FUNCTION "SCR_INQUIRY_HITS_GUARD"() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                  IF TG_OP = 'DELETE' OR EXISTS (SELECT 1 FROM "SCR_INQUIRIES" i WHERE i."INQUIRY_ID" = OLD."INQUIRY_ID" AND i."RECORD_HASH" IS NOT NULL) THEN
                    RAISE EXCEPTION 'The hits of a screening inquiry cannot be changed or deleted.';
                  END IF;
                  RETURN NEW;
                END $$;
                CREATE TRIGGER "TRG_SCR_INQUIRY_HITS_GUARD" BEFORE UPDATE OR DELETE ON "SCR_INQUIRY_HITS"
                  FOR EACH ROW EXECUTE FUNCTION "SCR_INQUIRY_HITS_GUARD"();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TRG_SCR_INQUIRY_HITS_GUARD" ON "SCR_INQUIRY_HITS";
                DROP FUNCTION IF EXISTS "SCR_INQUIRY_HITS_GUARD"();
                DROP TRIGGER IF EXISTS "TRG_SCR_INQUIRIES_GUARD" ON "SCR_INQUIRIES";
                DROP FUNCTION IF EXISTS "SCR_INQUIRIES_GUARD"();
                """);

            migrationBuilder.DropColumn(
                name: "PREV_HASH",
                table: "SCR_INQUIRIES");

            migrationBuilder.DropColumn(
                name: "RECORD_HASH",
                table: "SCR_INQUIRIES");
        }
    }
}
