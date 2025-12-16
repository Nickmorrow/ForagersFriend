using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixUserImagesRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserImages_UsiUsrId",
                table: "UserImages");

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UsiUsrId",
                table: "UserImages",
                column: "UsiUsrId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserImages_UsiUsrId",
                table: "UserImages");

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UsiUsrId",
                table: "UserImages",
                column: "UsiUsrId",
                unique: true,
                filter: "[UsiUsrId] IS NOT NULL");
        }
    }
}
