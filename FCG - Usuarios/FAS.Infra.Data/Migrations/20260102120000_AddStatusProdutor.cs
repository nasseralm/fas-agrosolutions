using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAS.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusProdutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Usuario",
                type: "int",
                nullable: false,
                defaultValue: 1); // Ativo
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Usuario");
        }
    }
}
