using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAS.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260214100000AddBusinessFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AreaHectares",
                table: "Talhao",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Talhao",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Talhao",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Safra",
                table: "Talhao",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Variedade",
                table: "Talhao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AreaTotalHectares",
                table: "Propriedade",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativa",
                table: "Propriedade",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Propriedade",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "Propriedade",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Uf",
                table: "Propriedade",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Talhao_PropriedadeId_Codigo",
                table: "Talhao",
                columns: new[] { "PropriedadeId", "Codigo" });

            migrationBuilder.CreateIndex(
                name: "IX_Propriedade_ProducerId_Codigo",
                table: "Propriedade",
                columns: new[] { "ProducerId", "Codigo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Talhao_PropriedadeId_Codigo",
                table: "Talhao");

            migrationBuilder.DropIndex(
                name: "IX_Propriedade_ProducerId_Codigo",
                table: "Propriedade");

            migrationBuilder.DropColumn(
                name: "AreaHectares",
                table: "Talhao");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Talhao");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Talhao");

            migrationBuilder.DropColumn(
                name: "Safra",
                table: "Talhao");

            migrationBuilder.DropColumn(
                name: "Variedade",
                table: "Talhao");

            migrationBuilder.DropColumn(
                name: "AreaTotalHectares",
                table: "Propriedade");

            migrationBuilder.DropColumn(
                name: "Ativa",
                table: "Propriedade");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Propriedade");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "Propriedade");

            migrationBuilder.DropColumn(
                name: "Uf",
                table: "Propriedade");
        }
    }
}
