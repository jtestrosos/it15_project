using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace manufacturing_system.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToBOM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedUsers_Facilities_FacilityID",
                table: "ArchivedUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchivedUsers",
                table: "ArchivedUsers");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "ArchivedUsers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "ArchivedUsers");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "ArchivedUsers",
                newName: "UserName");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "BillOfMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "ArchivedUsers",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<int>(
                name: "FacilityID",
                table: "ArchivedUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "ArchivedUsers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "ArchivedUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ArchivedUsers",
                type: "nvarchar(256)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "ArchivedUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchivedUsers",
                table: "ArchivedUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedUsers_Facilities_FacilityID",
                table: "ArchivedUsers",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchivedUsers_Facilities_FacilityID",
                table: "ArchivedUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArchivedUsers",
                table: "ArchivedUsers");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArchivedUsers");

            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "ArchivedUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ArchivedUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "ArchivedUsers");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "ArchivedUsers",
                newName: "Username");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "ArchivedUsers",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FacilityID",
                table: "ArchivedUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "ArchivedUsers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "ArchivedUsers",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArchivedUsers",
                table: "ArchivedUsers",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivedUsers_Facilities_FacilityID",
                table: "ArchivedUsers",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
