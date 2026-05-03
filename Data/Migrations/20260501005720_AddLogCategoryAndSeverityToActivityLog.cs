using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace production_system.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLogCategoryAndSeverityToActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "ActivityLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "ActivityLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "ActivityLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogCategory",
                table: "ActivityLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "ActivityLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "LogCategory",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "ActivityLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "ActivityLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
