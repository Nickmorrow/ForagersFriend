using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MessageSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_Users_UserUsrId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UserUsrId",
                table: "UserMessages");

            migrationBuilder.DropColumn(
                name: "UserUsrId",
                table: "UserMessages");

            migrationBuilder.RenameColumn(
                name: "UsmUsrId",
                table: "UserMessages",
                newName: "UsmThreadId");

            migrationBuilder.AlterColumn<string>(
                name: "UsmSubject",
                table: "UserMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "UsmParentMessageId",
                table: "UserMessages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserMessageThreads",
                columns: table => new
                {
                    UmtId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UmtUserAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UmtUserBId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UmtCreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessageThreads", x => x.UmtId);
                    table.ForeignKey(
                        name: "FK_UserMessageThreads_Users_UmtUserAId",
                        column: x => x.UmtUserAId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMessageThreads_Users_UmtUserBId",
                        column: x => x.UmtUserBId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserThreadStates",
                columns: table => new
                {
                    UtsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtsThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtsUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtsLastReadUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtsArchivedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtsDeletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtsUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserThreadStates", x => x.UtsId);
                    table.ForeignKey(
                        name: "FK_UserThreadStates_UserMessageThreads_UtsThreadId",
                        column: x => x.UtsThreadId,
                        principalTable: "UserMessageThreads",
                        principalColumn: "UmtId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserThreadStates_Users_UtsUserId",
                        column: x => x.UtsUserId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Cascade);
                });
            migrationBuilder.Sql(@"
UPDATE ur
SET
    UrlUserAId = ur.UrlUserBId,
    UrlUserBId = ur.UrlUserAId
FROM dbo.UserRelationship ur
WHERE ur.UrlUserAId > ur.UrlUserBId;
");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRelationship_UserA_LessThan_UserB",
                table: "UserRelationship",
                sql: "[UrlUserAId] < [UrlUserBId]");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmParentMessageId",
                table: "UserMessages",
                column: "UsmParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmRecipientId",
                table: "UserMessages",
                column: "UsmRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmSenderId",
                table: "UserMessages",
                column: "UsmSenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmThreadId",
                table: "UserMessages",
                column: "UsmThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageThreads_UmtUserAId_UmtUserBId",
                table: "UserMessageThreads",
                columns: new[] { "UmtUserAId", "UmtUserBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMessageThreads_UmtUserBId",
                table: "UserMessageThreads",
                column: "UmtUserBId");

            migrationBuilder.CreateIndex(
                name: "IX_UserThreadStates_UtsThreadId_UtsUserId",
                table: "UserThreadStates",
                columns: new[] { "UtsThreadId", "UtsUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserThreadStates_UtsUserId",
                table: "UserThreadStates",
                column: "UtsUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_UserMessageThreads_UsmThreadId",
                table: "UserMessages",
                column: "UsmThreadId",
                principalTable: "UserMessageThreads",
                principalColumn: "UmtId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_UserMessages_UsmParentMessageId",
                table: "UserMessages",
                column: "UsmParentMessageId",
                principalTable: "UserMessages",
                principalColumn: "UsmId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_Users_UsmRecipientId",
                table: "UserMessages",
                column: "UsmRecipientId",
                principalTable: "Users",
                principalColumn: "UsrId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_Users_UsmSenderId",
                table: "UserMessages",
                column: "UsmSenderId",
                principalTable: "Users",
                principalColumn: "UsrId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_UserMessageThreads_UsmThreadId",
                table: "UserMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_UserMessages_UsmParentMessageId",
                table: "UserMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_Users_UsmRecipientId",
                table: "UserMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_Users_UsmSenderId",
                table: "UserMessages");

            migrationBuilder.DropTable(
                name: "UserThreadStates");

            migrationBuilder.DropTable(
                name: "UserMessageThreads");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRelationship_UserA_LessThan_UserB",
                table: "UserRelationship");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UsmParentMessageId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UsmRecipientId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UsmSenderId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UsmThreadId",
                table: "UserMessages");

            migrationBuilder.DropColumn(
                name: "UsmParentMessageId",
                table: "UserMessages");

            migrationBuilder.RenameColumn(
                name: "UsmThreadId",
                table: "UserMessages",
                newName: "UsmUsrId");

            migrationBuilder.AlterColumn<string>(
                name: "UsmSubject",
                table: "UserMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "UserUsrId",
                table: "UserMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UserUsrId",
                table: "UserMessages",
                column: "UserUsrId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMessages_Users_UserUsrId",
                table: "UserMessages",
                column: "UserUsrId",
                principalTable: "Users",
                principalColumn: "UsrId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
