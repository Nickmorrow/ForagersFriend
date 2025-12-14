using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ThreatStatePropsDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UtsLastInboundUtc",
                table: "UserThreadStates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UtsLastOutboundUtc",
                table: "UserThreadStates",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UtsLastInboundUtc",
                table: "UserThreadStates");

            migrationBuilder.DropColumn(
                name: "UtsLastOutboundUtc",
                table: "UserThreadStates");
        }
    }
}
