using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PraktiskaisDarbs3.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS Items (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    CategoryId INTEGER NOT NULL,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
                );
                
                CREATE TABLE IF NOT EXISTS Orders (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ItemId INTEGER NOT NULL,
                    Amount INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ItemId) REFERENCES Items(Id) ON DELETE CASCADE
                );
                
                CREATE INDEX IF NOT EXISTS IX_Items_CategoryId ON Items(CategoryId);
                CREATE INDEX IF NOT EXISTS IX_Orders_ItemId ON Orders(ItemId);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
