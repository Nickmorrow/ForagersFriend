using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpDownVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
