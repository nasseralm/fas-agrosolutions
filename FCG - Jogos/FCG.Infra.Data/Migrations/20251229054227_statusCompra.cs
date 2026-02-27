using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class statusCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataStatus",
                table: "Compra",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MotivoRecusa",
                table: "Compra",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentId",
                table: "Compra",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Compra",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataStatus",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "MotivoRecusa",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Compra");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Compra");
        }
    }
}
