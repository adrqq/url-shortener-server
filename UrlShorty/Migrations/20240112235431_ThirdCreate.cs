using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShorty.Migrations
{
    /// <inheritdoc />
    public partial class ThirdCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ShortUrlModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortUrlModel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlModel_Slug",
                table: "ShortUrlModel",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrlModel_Url",
                table: "ShortUrlModel",
                column: "Url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShortUrlModel");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "User");
        }
    }
}
