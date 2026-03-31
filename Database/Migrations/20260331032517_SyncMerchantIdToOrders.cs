using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SyncMerchantIdToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropIndex(
            //     name: "UQ__Users__536C85E473704BEA",
            //     table: "Users");
            //
            // migrationBuilder.DropIndex(
            //     name: "UQ__Users__A9D105342B30EDB2",
            //     table: "Users");
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "Username",
            //     table: "Users",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(50)",
            //     oldMaxLength: 50);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "Email",
            //     table: "Users",
            //     type: "nvarchar(100)",
            //     maxLength: 100,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(100)",
            //     oldMaxLength: 100);
            //
            // migrationBuilder.AddColumn<Guid>(
            //     name: "MerchantId",
            //     table: "Orders",
            //     type: "uniqueidentifier",
            //     nullable: false,
            //     defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "Name",
            //     table: "Merchants",
            //     type: "nvarchar(100)",
            //     maxLength: 100,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(100)",
            //     oldMaxLength: 100);
            //
            // migrationBuilder.CreateIndex(
            //     name: "UQ__Users__536C85E473704BEA",
            //     table: "Users",
            //     column: "Username",
            //     unique: true,
            //     filter: "[Username] IS NOT NULL");
            //
            // migrationBuilder.CreateIndex(
            //     name: "UQ__Users__A9D105342B30EDB2",
            //     table: "Users",
            //     column: "Email",
            //     unique: true,
            //     filter: "[Email] IS NOT NULL");
            //
            // migrationBuilder.CreateIndex(
            //     name: "IX_Orders_MerchantId",
            //     table: "Orders",
            //     column: "MerchantId",
            //     filter: "[DeletedAt] IS NULL");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_Orders_Merchants_MerchantId",
            //     table: "Orders",
            //     column: "MerchantId",
            //     principalTable: "Merchants",
            //     principalColumn: "Id",
            //     onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Orders_Merchants_MerchantId",
            //     table: "Orders");
            //
            // migrationBuilder.DropIndex(
            //     name: "UQ__Users__536C85E473704BEA",
            //     table: "Users");
            //
            // migrationBuilder.DropIndex(
            //     name: "UQ__Users__A9D105342B30EDB2",
            //     table: "Users");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_Orders_MerchantId",
            //     table: "Orders");
            //
            // migrationBuilder.DropColumn(
            //     name: "MerchantId",
            //     table: "Orders");
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "Username",
            //     table: "Users",
            //     type: "nvarchar(50)",
            //     maxLength: 50,
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(50)",
            //     oldMaxLength: 50,
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "Email",
            //     table: "Users",
            //     type: "nvarchar(100)",
            //     maxLength: 100,
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(100)",
            //     oldMaxLength: 100,
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "Name",
            //     table: "Merchants",
            //     type: "nvarchar(100)",
            //     maxLength: 100,
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(100)",
            //     oldMaxLength: 100,
            //     oldNullable: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "UQ__Users__536C85E473704BEA",
            //     table: "Users",
            //     column: "Username",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "UQ__Users__A9D105342B30EDB2",
            //     table: "Users",
            //     column: "Email",
            //     unique: true);
        }
    }
}
