using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mywebsite.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectModelWithDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Projects");
        }
    }
}
