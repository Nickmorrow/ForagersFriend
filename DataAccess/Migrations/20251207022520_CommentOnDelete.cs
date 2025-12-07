using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CommentOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments",
                column: "UscParentCommentId",
                principalTable: "UserFindsComments",
                principalColumn: "UscId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments",
                column: "UscParentCommentId",
                principalTable: "UserFindsComments",
                principalColumn: "UscId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
