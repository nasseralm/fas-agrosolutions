using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FAS.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260213110000InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Propriedade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProducerId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescricaoLocalizacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Localizacao = table.Column<Geometry>(type: "geography", nullable: true),
                    LocalizacaoGeoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propriedade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Talhao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropriedadeId = table.Column<int>(type: "int", nullable: false),
                    ProducerId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cultura = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Delimitacao = table.Column<Geometry>(type: "geography", nullable: false),
                    DelimitacaoGeoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Talhao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Talhao_Propriedade_PropriedadeId",
                        column: x => x.PropriedadeId,
                        principalTable: "Propriedade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Propriedade_ProducerId_Nome",
                table: "Propriedade",
                columns: new[] { "ProducerId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_Talhao_PropriedadeId_Nome",
                table: "Talhao",
                columns: new[] { "PropriedadeId", "Nome" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Talhao");

            migrationBuilder.DropTable(
                name: "Propriedade");
        }
    }
}
