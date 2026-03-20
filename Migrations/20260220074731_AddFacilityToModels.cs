using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace production_system.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "ProductionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "InventoryTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "Costs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "Components",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "BillOfMaterials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacilityID",
                table: "ActivityLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_FacilityID",
                table: "WorkOrders",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FacilityID",
                table: "Products",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlans_FacilityID",
                table: "ProductionPlans",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_FacilityID",
                table: "InventoryTransactions",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Costs_FacilityID",
                table: "Costs",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_Components_FacilityID",
                table: "Components",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterials_FacilityID",
                table: "BillOfMaterials",
                column: "FacilityID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_FacilityID",
                table: "ActivityLogs",
                column: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Facilities_FacilityID",
                table: "ActivityLogs",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterials_Facilities_FacilityID",
                table: "BillOfMaterials",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Components_Facilities_FacilityID",
                table: "Components",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Costs_Facilities_FacilityID",
                table: "Costs",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Facilities_FacilityID",
                table: "InventoryTransactions",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPlans_Facilities_FacilityID",
                table: "ProductionPlans",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Facilities_FacilityID",
                table: "Products",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Facilities_FacilityID",
                table: "WorkOrders",
                column: "FacilityID",
                principalTable: "Facilities",
                principalColumn: "FacilityID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Facilities_FacilityID",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterials_Facilities_FacilityID",
                table: "BillOfMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_Components_Facilities_FacilityID",
                table: "Components");

            migrationBuilder.DropForeignKey(
                name: "FK_Costs_Facilities_FacilityID",
                table: "Costs");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Facilities_FacilityID",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPlans_Facilities_FacilityID",
                table: "ProductionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Facilities_FacilityID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Facilities_FacilityID",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_FacilityID",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_Products_FacilityID",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPlans_FacilityID",
                table: "ProductionPlans");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_FacilityID",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Costs_FacilityID",
                table: "Costs");

            migrationBuilder.DropIndex(
                name: "IX_Components_FacilityID",
                table: "Components");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterials_FacilityID",
                table: "BillOfMaterials");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_FacilityID",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "ProductionPlans");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Costs");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "FacilityID",
                table: "ActivityLogs");
        }
    }
}

