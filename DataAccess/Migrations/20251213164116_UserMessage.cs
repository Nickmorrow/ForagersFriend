using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserMessages_Users_UserUsrId')
    ALTER TABLE [dbo].[UserMessages] DROP CONSTRAINT [FK_UserMessages_Users_UserUsrId];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_UserMessages_UserUsrId' 
             AND object_id = OBJECT_ID('[dbo].[UserMessages]'))
    DROP INDEX [IX_UserMessages_UserUsrId] ON [dbo].[UserMessages];
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('[dbo].[UserMessages]', 'UserUsrId') IS NOT NULL
    ALTER TABLE [dbo].[UserMessages] DROP COLUMN [UserUsrId];
");


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

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmParentMessageId",
                table: "UserMessages",
                column: "UsmParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmRecipientId_UsmStatus_UsmSendDate",
                table: "UserMessages",
                columns: new[] { "UsmRecipientId", "UsmStatus", "UsmSendDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmSenderId",
                table: "UserMessages",
                column: "UsmSenderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMessages_UsmThreadId",
                table: "UserMessages",
                column: "UsmThreadId");

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
                name: "FK_UserMessages_UserMessages_UsmParentMessageId",
                table: "UserMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_Users_UsmRecipientId",
                table: "UserMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMessages_Users_UsmSenderId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UsmParentMessageId",
                table: "UserMessages");

            migrationBuilder.DropIndex(
                name: "IX_UserMessages_UsmRecipientId_UsmStatus_UsmSendDate",
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
