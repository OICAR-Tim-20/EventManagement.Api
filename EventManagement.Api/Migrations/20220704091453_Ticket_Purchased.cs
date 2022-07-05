using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Api.Migrations
{
    public partial class Ticket_Purchased : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Purchased",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purchased",
                table: "Tickets");
        }
    }
}
