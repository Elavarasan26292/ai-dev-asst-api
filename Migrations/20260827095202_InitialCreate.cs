using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ai_dev_asst_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PERMISSIONS",
                columns: table => new
                {
                    PERMISSIONID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PERMISSIONNAME = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSIONS", x => x.PERMISSIONID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    ROLEID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ROLENAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CREATEDAT = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CREATEDBYUSERID = table.Column<int>(type: "int", nullable: true),
                    LASTUPDATEDAT = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ISDELETED = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.ROLEID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ROLEPERMISSIONS",
                columns: table => new
                {
                    ROLEID = table.Column<int>(type: "int", nullable: false),
                    PERMISSIONID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLEPERMISSIONS", x => new { x.ROLEID, x.PERMISSIONID });
                    table.ForeignKey(
                        name: "FK_ROLEPERMISSIONS_PERMISSIONS_PERMISSIONID",
                        column: x => x.PERMISSIONID,
                        principalTable: "PERMISSIONS",
                        principalColumn: "PERMISSIONID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLEPERMISSIONS_ROLES_ROLEID",
                        column: x => x.ROLEID,
                        principalTable: "ROLES",
                        principalColumn: "ROLEID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    USERID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FIRSTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LASTNAME = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PHONE = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EMAIL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PASSWORDHASH = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ROLEID = table.Column<int>(type: "int", nullable: false),
                    CREATEDAT = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CREATEDBYUSERID = table.Column<int>(type: "int", nullable: true),
                    LASTUPDATEDAT = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    THEME = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ISDELETED = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.USERID);
                    table.ForeignKey(
                        name: "FK_USERS_ROLES_ROLEID",
                        column: x => x.ROLEID,
                        principalTable: "ROLES",
                        principalColumn: "ROLEID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "REFRESHTOKENS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    USERID = table.Column<int>(type: "int", nullable: false),
                    TOKEN = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EXPIRESAT = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ISREVOKED = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REFRESHTOKENS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_REFRESHTOKENS_USERS_USERID",
                        column: x => x.USERID,
                        principalTable: "USERS",
                        principalColumn: "USERID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "PERMISSIONS",
                columns: new[] { "PERMISSIONID", "PERMISSIONNAME" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567801"), "Allow User to Access User" },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567802"), "Allow User to Create & Edit Roles" }
                });

            migrationBuilder.InsertData(
                table: "ROLES",
                columns: new[] { "ROLEID", "CREATEDAT", "CREATEDBYUSERID", "ISDELETED", "LASTUPDATEDAT", "ROLENAME" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, null, "Admin" });

            migrationBuilder.InsertData(
                table: "ROLEPERMISSIONS",
                columns: new[] { "PERMISSIONID", "ROLEID" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567801"), 1 },
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567802"), 1 }
                });

            migrationBuilder.InsertData(
                table: "USERS",
                columns: new[] { "USERID", "CREATEDAT", "CREATEDBYUSERID", "EMAIL", "FIRSTNAME", "ISDELETED", "LASTNAME", "LASTUPDATEDAT", "PASSWORDHASH", "PHONE", "ROLEID", "THEME" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "elavarasan261992@gmail.com", "Elavarasan", false, "Saravanan", null, "$2a$11$lXEwCzE9ZDByR1iehY1rVOnh.DTU3nomnebFmAUTJyyvtUIJYEkS2", "7010315801", 1, "default" });

            migrationBuilder.CreateIndex(
                name: "IX_PERMISSIONS_PERMISSIONNAME",
                table: "PERMISSIONS",
                column: "PERMISSIONNAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REFRESHTOKENS_USERID",
                table: "REFRESHTOKENS",
                column: "USERID");

            migrationBuilder.CreateIndex(
                name: "IX_ROLEPERMISSIONS_PERMISSIONID",
                table: "ROLEPERMISSIONS",
                column: "PERMISSIONID");

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_ROLENAME",
                table: "ROLES",
                column: "ROLENAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_EMAIL",
                table: "USERS",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_ROLEID",
                table: "USERS",
                column: "ROLEID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REFRESHTOKENS");

            migrationBuilder.DropTable(
                name: "ROLEPERMISSIONS");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "PERMISSIONS");

            migrationBuilder.DropTable(
                name: "ROLES");
        }
    }
}
