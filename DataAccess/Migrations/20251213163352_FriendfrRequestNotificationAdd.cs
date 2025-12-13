using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FriendfrRequestNotificationAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendRequest",
                columns: table => new
                {
                    FrqId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrqRequesterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrqAddresseeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FrqStatus = table.Column<int>(type: "int", nullable: false),
                    FrqCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrqAcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequest", x => x.FrqId);
                    table.ForeignKey(
                        name: "FK_FriendRequest_Users_FrqAddresseeUserId",
                        column: x => x.FrqAddresseeUserId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendRequest_Users_FrqRequesterUserId",
                        column: x => x.FrqRequesterUserId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NotMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NotIsRead = table.Column<bool>(type: "bit", nullable: false),
                    NotCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotReadDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotId);
                    table.ForeignKey(
                        name: "FK_Notification_Users_NotActorUserId",
                        column: x => x.NotActorUserId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notification_Users_NotUserId",
                        column: x => x.NotUserId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRelationship",
                columns: table => new
                {
                    UrlId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrlUserAId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrlUserBId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrlStatus = table.Column<int>(type: "int", nullable: false),
                    UrlActionUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UrlCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UrlUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRelationship", x => x.UrlId);
                    table.ForeignKey(
                        name: "FK_UserRelationship_Users_UrlActionUserId",
                        column: x => x.UrlActionUserId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRelationship_Users_UrlUserAId",
                        column: x => x.UrlUserAId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRelationship_Users_UrlUserBId",
                        column: x => x.UrlUserBId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_FrqAddresseeUserId",
                table: "FriendRequest",
                column: "FrqAddresseeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequest_FrqRequesterUserId_FrqAddresseeUserId",
                table: "FriendRequest",
                columns: new[] { "FrqRequesterUserId", "FrqAddresseeUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_NotActorUserId",
                table: "Notification",
                column: "NotActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_NotUserId_NotIsRead",
                table: "Notification",
                columns: new[] { "NotUserId", "NotIsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationship_UrlActionUserId",
                table: "UserRelationship",
                column: "UrlActionUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationship_UrlUserAId_UrlUserBId",
                table: "UserRelationship",
                columns: new[] { "UrlUserAId", "UrlUserBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationship_UrlUserBId",
                table: "UserRelationship",
                column: "UrlUserBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendRequest");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "UserRelationship");
        }
    }
}
