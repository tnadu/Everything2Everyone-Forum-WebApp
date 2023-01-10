using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Everything2Everyone.Migrations
{
    public partial class SingleTypeOfContentForChapters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentParsed",
                table: "ChapterVersions");

            migrationBuilder.DropColumn(
                name: "ContentParsed",
                table: "Chapters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentParsed",
                table: "ChapterVersions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentParsed",
                table: "Chapters",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
