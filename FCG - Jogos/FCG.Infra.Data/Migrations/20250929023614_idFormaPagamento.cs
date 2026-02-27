using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class idFormaPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormaPagamentoId",
                table: "Compra",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPagamentoId",
                table: "Compra");
        }
    }
}
