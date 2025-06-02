using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AkanjiApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFundersToDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Funder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Identifier = table.Column<string>(type: "longtext", nullable: false),
                    Scheme = table.Column<string>(type: "longtext", nullable: false),
                    GrantNumber = table.Column<string>(type: "longtext", nullable: false),
                    DocumentoDOI = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funder_Documentos_DocumentoDOI",
                        column: x => x.DocumentoDOI,
                        principalTable: "Documentos",
                        principalColumn: "DOI",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Funder_DocumentoDOI",
                table: "Funder",
                column: "DocumentoDOI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Funder");
        }
    }
}
