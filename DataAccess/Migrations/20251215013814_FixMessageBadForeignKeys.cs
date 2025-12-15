using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixMessageBadForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserMessages_User')
BEGIN
    ALTER TABLE [dbo].[UserMessages] DROP CONSTRAINT [FK_UserMessages_User];
END
");

            // OPTIONAL cleanup: if you accidentally created duplicate sender/recipient FKs, drop the older named ones.
            // Keep the ones you actually want (typically: FK_UserMessages_Users_UsmSenderId and FK_UserMessages_Users_UsmRecipientId).

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserMessages_Sender')
BEGIN
    ALTER TABLE [dbo].[UserMessages] DROP CONSTRAINT [FK_UserMessages_Sender];
END

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserMessages_Recipient')
BEGIN
    ALTER TABLE [dbo].[UserMessages] DROP CONSTRAINT [FK_UserMessages_Recipient];
END
");
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
