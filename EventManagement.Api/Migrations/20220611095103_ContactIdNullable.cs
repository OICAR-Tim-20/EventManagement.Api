using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Api.Migrations
{
    public partial class ContactIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Contacts_ContactId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "ContactId",
                table: "Tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Contacts_ContactId",
                table: "Tickets",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "ContactId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Contacts_ContactId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "ContactId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Contacts_ContactId",
                table: "Tickets",
                column: "ContactId",
                principalTable: "Contacts",
                principalColumn: "ContactId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
