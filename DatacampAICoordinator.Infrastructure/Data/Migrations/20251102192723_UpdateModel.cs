using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatacampAICoordinator.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DatacampId",
                table: "Student",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_StudentDailyStatus_ProcessId",
                table: "StudentDailyStatus",
                column: "ProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentDailyStatus_Process_ProcessId",
                table: "StudentDailyStatus",
                column: "ProcessId",
                principalTable: "Process",
                principalColumn: "ProcessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentDailyStatus_Process_ProcessId",
                table: "StudentDailyStatus");

            migrationBuilder.DropIndex(
                name: "IX_StudentDailyStatus_ProcessId",
                table: "StudentDailyStatus");

            migrationBuilder.AlterColumn<string>(
                name: "DatacampId",
                table: "Student",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
