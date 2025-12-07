using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CommentCascadeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments",
                column: "UscParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments",
                column: "UscParentCommentId",
                principalTable: "UserFindsComments",
                principalColumn: "UscId",
                onDelete: ReferentialAction.NoAction); // or ReferentialAction.Restrict

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments");

            migrationBuilder.DropIndex(
                name: "IX_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments");
        }
    }
}
