using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatacampAICoordinator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDbModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DatacampId",
                table: "Student",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatacampId",
                table: "Student");
        }
    }
}
