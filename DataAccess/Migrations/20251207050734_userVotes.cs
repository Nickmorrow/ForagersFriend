using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class userVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UscDownVotes",
                table: "UserFindsComments");

            migrationBuilder.DropColumn(
                name: "UscUpVotes",
                table: "UserFindsComments");

            migrationBuilder.DropColumn(
                name: "UsfDownVotes",
                table: "UserFinds");

            migrationBuilder.DropColumn(
                name: "UsfUpVotes",
                table: "UserFinds");

            migrationBuilder.CreateTable(
                name: "UserVotes",
                columns: table => new
                {
                    UsvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsvUsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsvUscId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsvVoteValue = table.Column<int>(type: "int", nullable: false),
                    UsvUsfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVotes", x => x.UsvId);
                    table.ForeignKey(
                        name: "FK_UserVotes_UserFindsComments_UsvUscId",
                        column: x => x.UsvUscId,
                        principalTable: "UserFindsComments",
                        principalColumn: "UscId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVotes_UserFinds_UsvUsfId",
                        column: x => x.UsvUsfId,
                        principalTable: "UserFinds",
                        principalColumn: "UsfId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserVotes_Users_UsvUsrId",
                        column: x => x.UsvUsrId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UsvUscId",
                table: "UserVotes",
                column: "UsvUscId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UsvUsfId",
                table: "UserVotes",
                column: "UsvUsfId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UsvUsrId",
                table: "UserVotes",
                column: "UsvUsrId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVotes");

            migrationBuilder.AddColumn<int>(
                name: "UscDownVotes",
                table: "UserFindsComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UscUpVotes",
                table: "UserFindsComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsfDownVotes",
                table: "UserFinds",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsfUpVotes",
                table: "UserFinds",
                type: "int",
                nullable: true);
        }
    }
}
