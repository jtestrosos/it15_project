using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace manufacturing_system.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Components",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Components");
        }
    }
}
