using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IntroduceIdentityAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM dbo.Users AS profile
                    LEFT JOIN dbo.UserSecurity AS security ON security.UssUsrId = profile.UsrId
                    WHERE security.UssUsrId IS NULL)
                    THROW 51000, 'Every profile must have a matching legacy account before Identity migration.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.UserSecurity AS security
                    LEFT JOIN dbo.Users AS profile ON profile.UsrId = security.UssUsrId
                    WHERE profile.UsrId IS NULL)
                    THROW 51000, 'Orphaned legacy accounts must be resolved before Identity migration.', 1;

                IF EXISTS (
                    SELECT UPPER(LTRIM(RTRIM(UssUsername)))
                    FROM dbo.UserSecurity
                    GROUP BY UPPER(LTRIM(RTRIM(UssUsername)))
                    HAVING COUNT_BIG(*) > 1)
                    THROW 51000, 'Duplicate normalized usernames must be resolved before Identity migration.', 1;

                IF EXISTS (
                    SELECT UssUsrId
                    FROM dbo.UserSecurity
                    GROUP BY UssUsrId
                    HAVING COUNT_BIG(*) > 1)
                    THROW 51000, 'Each profile must have exactly one legacy account before Identity migration.', 1;

                IF EXISTS (
                    SELECT UPPER(LTRIM(RTRIM(UsrEmail)))
                    FROM dbo.Users
                    WHERE NULLIF(LTRIM(RTRIM(UsrEmail)), N'') IS NOT NULL
                    GROUP BY UPPER(LTRIM(RTRIM(UsrEmail)))
                    HAVING COUNT_BIG(*) > 1)
                    THROW 51000, 'Duplicate normalized emails must be resolved before Identity migration.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.UserSecurity
                    WHERE NULLIF(LTRIM(RTRIM(UssUsername)), N'') IS NULL
                        OR LEN(LTRIM(RTRIM(UssUsername))) > 256)
                    THROW 51000, 'Invalid or oversized usernames must be resolved before Identity migration.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.Users
                    WHERE LEN(LTRIM(RTRIM(UsrEmail))) > 256
                        OR (NULLIF(LTRIM(RTRIM(UsrEmail)), N'') IS NOT NULL
                            AND LTRIM(RTRIM(UsrEmail)) NOT LIKE N'%_@_%._%'))
                    THROW 51000, 'Invalid or oversized account fields must be resolved before Identity migration.', 1;

                INSERT INTO dbo.AspNetUsers
                    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                     PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
                     TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount)
                SELECT profile.UsrId,
                       LTRIM(RTRIM(security.UssUsername)),
                       UPPER(LTRIM(RTRIM(security.UssUsername))),
                       NULLIF(LTRIM(RTRIM(profile.UsrEmail)), N''),
                       UPPER(NULLIF(LTRIM(RTRIM(profile.UsrEmail)), N'')),
                       CONVERT(bit, 0),
                       NULL,
                       CONVERT(nvarchar(36), NEWID()),
                       CONVERT(nvarchar(36), NEWID()),
                       NULL,
                       CONVERT(bit, 0),
                       CONVERT(bit, 0),
                       NULL,
                       CONVERT(bit, 0),
                       0
                FROM dbo.Users AS profile
                INNER JOIN dbo.UserSecurity AS security ON security.UssUsrId = profile.UsrId;
                """);

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspNetUsers_UsrId",
                table: "Users",
                column: "UsrId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspNetUsers_UsrId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
