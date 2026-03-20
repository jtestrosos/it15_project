using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace production_system.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoleColumnMigrateToAspNetRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Migrate existing role data from Role column to AspNetUserRoles table
            migrationBuilder.Sql(@"
                -- Insert user-role mappings for users who have a Role value
                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT 
                    u.Id,
                    r.Id
                FROM AspNetUsers u
                INNER JOIN AspNetRoles r ON u.Role = r.Name
                WHERE u.Role IS NOT NULL 
                  AND u.Role != ''
                  AND NOT EXISTS (
                      SELECT 1 
                      FROM AspNetUserRoles ur 
                      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
                  )
            ");

            // Step 2: Drop the Role column
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "nvarchar(25)",
                nullable: false,
                defaultValue: "");
        }
    }
}

