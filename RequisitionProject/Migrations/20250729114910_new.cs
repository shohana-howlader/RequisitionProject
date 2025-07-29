using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RequisitionProject.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequisitionItems_Products_ProductId",
                table: "RequisitionItems");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "RequisitionItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "RequisitionItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "RequisitionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "RequisitionApprovals",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionItems_ProductId1",
                table: "RequisitionItems",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitionItems_Products_ProductId",
                table: "RequisitionItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitionItems_Products_ProductId1",
                table: "RequisitionItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequisitionItems_Products_ProductId",
                table: "RequisitionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RequisitionItems_Products_ProductId1",
                table: "RequisitionItems");

            migrationBuilder.DropIndex(
                name: "IX_RequisitionItems_ProductId1",
                table: "RequisitionItems");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "RequisitionItems");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "RequisitionItems",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Purpose",
                table: "RequisitionItems",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "RequisitionApprovals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Description", "IsActive", "Name", "Unit", "UnitPrice" },
                values: new object[,]
                {
                    { 1, "High-performance laptop", true, "Laptop", "Piece", 1000.00m },
                    { 2, "Wireless optical mouse", true, "Mouse", "Piece", 25.00m },
                    { 3, "Mechanical keyboard", true, "Keyboard", "Piece", 75.00m },
                    { 4, "24-inch LED monitor", true, "Monitor", "Piece", 300.00m },
                    { 5, "A4 size paper ream", true, "Printer Paper", "Ream", 10.00m }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitionItems_Products_ProductId",
                table: "RequisitionItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
