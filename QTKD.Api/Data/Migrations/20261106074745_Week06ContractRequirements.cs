using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QTKD.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Week06ContractRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentReference",
                table: "Contracts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Contracts",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            // InitialCreate seeded explicit IDs. Advance identity sequences before week 06 POSTs.
            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('"Customers"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Customers"), 1), EXISTS(SELECT 1 FROM "Customers"));
                SELECT setval(pg_get_serial_sequence('"Contracts"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Contracts"), 1), EXISTS(SELECT 1 FROM "Contracts"));
                SELECT setval(pg_get_serial_sequence('"PriceLists"', 'Id'), COALESCE((SELECT MAX("Id") FROM "PriceLists"), 1), EXISTS(SELECT 1 FROM "PriceLists"));
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentReference",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Contracts");
        }
    }
}
