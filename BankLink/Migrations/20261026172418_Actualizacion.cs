using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankLink.Migrations
{
    /// <inheritdoc />
    public partial class Actualizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_ClienteId",
                table: "Cuentas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_Numero",
                table: "Cuentas",
                column: "Numero",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cuentas_Clientes_ClienteId",
                table: "Cuentas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cuentas_Clientes_ClienteId",
                table: "Cuentas");

            migrationBuilder.DropIndex(
                name: "IX_Cuentas_ClienteId",
                table: "Cuentas");

            migrationBuilder.DropIndex(
                name: "IX_Cuentas_Numero",
                table: "Cuentas");
        }
    }
}
