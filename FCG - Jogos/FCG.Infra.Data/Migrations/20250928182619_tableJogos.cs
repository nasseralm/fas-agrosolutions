using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class tableJogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Genero = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataLancamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Desenvolvedor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Distribuidora = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClassificacaoIndicativa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estoque = table.Column<int>(type: "int", nullable: false),
                    Plataforma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jogo");
        }
    }
}
