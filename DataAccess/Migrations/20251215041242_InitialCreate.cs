using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFindsComments",
                columns: table => new
                {
                    UscId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UscComment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UscCommentScore = table.Column<int>(type: "int", nullable: true),
                    UscCommentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UscParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFindsComments", x => x.UscId);
                    table.ForeignKey(
                        name: "FK_UserFindsComments_UserFindsComments_UscParentCommentId",
                        column: x => x.UscParentCommentId,
                        principalTable: "UserFindsComments",
                        principalColumn: "UscId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsrName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsrBio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsrEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsrFindsNum = table.Column<int>(type: "int", nullable: true),
                    UsrExpScore = table.Column<int>(type: "int", nullable: true),
                    UsrJoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsrCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsrStateorProvince = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsrZipCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UsrId);
                });

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
                name: "UserFinds",
                columns: table => new
                {
                    UsfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsfName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfUsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsfFindDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsfSpeciesName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfSpeciesType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfUseCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfFeatures = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfLookAlikes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfHarvestMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfTastesLike = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsfAccuracyScore = table.Column<int>(type: "int", nullable: true),
                    UsfAccessibility = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFinds", x => x.UsfId);
                    table.ForeignKey(
                        name: "FK_UserFinds_Users_UsfUsrId",
                        column: x => x.UsfUsrId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.CheckConstraint("CK_UserRelationship_UserA_LessThan_UserB", "[UrlUserAId] < [UrlUserBId]");
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

            migrationBuilder.CreateTable(
                name: "UserSecurity",
                columns: table => new
                {
                    UssId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UssUsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UssUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UssPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UssLastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UssLastLogoffDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSecurity", x => x.UssId);
                    table.ForeignKey(
                        name: "FK_UserSecurity_Users_UssUsrId",
                        column: x => x.UssUsrId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFindLocation",
                columns: table => new
                {
                    UslId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UslUsfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UslLatitude = table.Column<double>(type: "float", nullable: false),
                    UslLongitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFindLocation", x => x.UslId);
                    table.ForeignKey(
                        name: "FK_UserFindLocation_UserFinds_UslUsfId",
                        column: x => x.UslUsfId,
                        principalTable: "UserFinds",
                        principalColumn: "UsfId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFindsCommentXref",
                columns: table => new
                {
                    UcxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcxUsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcxUscId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UcxUsfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFindsCommentXref", x => x.UcxId);
                    table.ForeignKey(
                        name: "FK_UserFindsCommentXref_UserFindsComments_UcxUscId",
                        column: x => x.UcxUscId,
                        principalTable: "UserFindsComments",
                        principalColumn: "UscId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFindsCommentXref_UserFinds_UcxUsfId",
                        column: x => x.UcxUsfId,
                        principalTable: "UserFinds",
                        principalColumn: "UsfId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFindsCommentXref_Users_UcxUsrId",
                        column: x => x.UcxUsrId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserImages",
                columns: table => new
                {
                    UsiId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsiUsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsiUsfId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsiImageData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserImages", x => x.UsiId);
                    table.ForeignKey(
                        name: "FK_UserImages_UserFinds_UsiUsfId",
                        column: x => x.UsiUsfId,
                        principalTable: "UserFinds",
                        principalColumn: "UsfId");
                    table.ForeignKey(
                        name: "FK_UserImages_Users_UsiUsrId",
                        column: x => x.UsiUsrId,
                        principalTable: "Users",
                        principalColumn: "UsrId");
                });

            migrationBuilder.CreateTable(
                name: "UserVotes",
                columns: table => new
                {
                    UsvId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsvUsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsvUscId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsvVoteValue = table.Column<int>(type: "int", nullable: false),
                    UsvUsfId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "UserMessages",
                columns: table => new
                {
                    UsmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsmThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsmParentMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsmSenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsmRecipientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsmSubject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UsmMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsmSendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsmReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsmStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessages", x => x.UsmId);
                    table.ForeignKey(
                        name: "FK_UserMessages_UserMessageThreads_UsmThreadId",
                        column: x => x.UsmThreadId,
                        principalTable: "UserMessageThreads",
                        principalColumn: "UmtId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMessages_UserMessages_UsmParentMessageId",
                        column: x => x.UsmParentMessageId,
                        principalTable: "UserMessages",
                        principalColumn: "UsmId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMessages_Users_UsmRecipientId",
                        column: x => x.UsmRecipientId,
                        principalTable: "Users",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMessages_Users_UsmSenderId",
                        column: x => x.UsmSenderId,
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
                    UtsLastInboundUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtsLastOutboundUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "IX_UserFindLocation_UslUsfId",
                table: "UserFindLocation",
                column: "UslUsfId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFinds_UsfUsrId",
                table: "UserFinds",
                column: "UsfUsrId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFindsComments_UscParentCommentId",
                table: "UserFindsComments",
                column: "UscParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFindsCommentXref_UcxUscId",
                table: "UserFindsCommentXref",
                column: "UcxUscId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFindsCommentXref_UcxUsfId",
                table: "UserFindsCommentXref",
                column: "UcxUsfId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFindsCommentXref_UcxUsrId",
                table: "UserFindsCommentXref",
                column: "UcxUsrId");

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UsiUsfId",
                table: "UserImages",
                column: "UsiUsfId");

            migrationBuilder.CreateIndex(
                name: "IX_UserImages_UsiUsrId",
                table: "UserImages",
                column: "UsiUsrId",
                unique: true,
                filter: "[UsiUsrId] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserSecurity_UssUsrId",
                table: "UserSecurity",
                column: "UssUsrId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserThreadStates_UtsThreadId_UtsUserId",
                table: "UserThreadStates",
                columns: new[] { "UtsThreadId", "UtsUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserThreadStates_UtsUserId",
                table: "UserThreadStates",
                column: "UtsUserId");

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
                name: "FriendRequest");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "UserFindLocation");

            migrationBuilder.DropTable(
                name: "UserFindsCommentXref");

            migrationBuilder.DropTable(
                name: "UserImages");

            migrationBuilder.DropTable(
                name: "UserMessages");

            migrationBuilder.DropTable(
                name: "UserRelationship");

            migrationBuilder.DropTable(
                name: "UserSecurity");

            migrationBuilder.DropTable(
                name: "UserThreadStates");

            migrationBuilder.DropTable(
                name: "UserVotes");

            migrationBuilder.DropTable(
                name: "UserMessageThreads");

            migrationBuilder.DropTable(
                name: "UserFindsComments");

            migrationBuilder.DropTable(
                name: "UserFinds");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
