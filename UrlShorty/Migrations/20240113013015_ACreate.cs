using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShorty.Migrations
{
    /// <inheritdoc />
    public partial class ACreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShortUrlModel",
                table: "ShortUrlModel");

            migrationBuilder.RenameTable(
                name: "ShortUrlModel",
                newName: "ShortUrl");

            migrationBuilder.RenameIndex(
                name: "IX_ShortUrlModel_Url",
                table: "ShortUrl",
                newName: "IX_ShortUrl_Url");

            migrationBuilder.RenameIndex(
                name: "IX_ShortUrlModel_Slug",
                table: "ShortUrl",
                newName: "IX_ShortUrl_Slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShortUrl",
                table: "ShortUrl",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Redirection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Redirection", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Redirection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShortUrl",
                table: "ShortUrl");

            migrationBuilder.RenameTable(
                name: "ShortUrl",
                newName: "ShortUrlModel");

            migrationBuilder.RenameIndex(
                name: "IX_ShortUrl_Url",
                table: "ShortUrlModel",
                newName: "IX_ShortUrlModel_Url");

            migrationBuilder.RenameIndex(
                name: "IX_ShortUrl_Slug",
                table: "ShortUrlModel",
                newName: "IX_ShortUrlModel_Slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShortUrlModel",
                table: "ShortUrlModel",
                column: "Id");
        }
    }
}
