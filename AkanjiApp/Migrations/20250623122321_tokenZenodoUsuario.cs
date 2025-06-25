using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkanjiApp.Migrations
{
    /// <inheritdoc />
    public partial class tokenZenodoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZenodoToken",
                table: "Usuarios",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZenodoToken",
                table: "Usuarios");
        }
    }
}
