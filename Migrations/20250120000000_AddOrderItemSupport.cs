using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PraktiskaisDarbs3.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS OrderItems (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    ItemId INTEGER NOT NULL,
                    Amount INTEGER NOT NULL
                );
            ");

            migrationBuilder.Sql(@"
                INSERT INTO OrderItems (OrderId, ItemId, Amount)
                SELECT o.Id, o.ItemId, o.Amount
                FROM Orders o
                WHERE EXISTS (
                    SELECT 1 FROM pragma_table_info('Orders') WHERE name = 'ItemId'
                )
                AND NOT EXISTS (
                    SELECT 1 FROM OrderItems oi WHERE oi.OrderId = o.Id
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_OrderItems_OrderId ON OrderItems(OrderId);
                CREATE INDEX IF NOT EXISTS IX_OrderItems_ItemId ON OrderItems(ItemId);
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS Orders_new (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                
                INSERT INTO Orders_new (Id, CreatedAt)
                SELECT Id, 
                       COALESCE(CreatedAt, datetime('now')) as CreatedAt
                FROM Orders
                WHERE EXISTS (
                    SELECT 1 FROM pragma_table_info('Orders') WHERE name = 'ItemId'
                )
                AND NOT EXISTS (
                    SELECT 1 FROM Orders_new WHERE Orders_new.Id = Orders.Id
                );
                
                DROP TABLE IF EXISTS Orders_old;
                
                ALTER TABLE Orders RENAME TO Orders_old;
                ALTER TABLE Orders_new RENAME TO Orders;
                DROP TABLE IF EXISTS Orders_old;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE Orders
                SET ItemId = (
                    SELECT ItemId 
                    FROM OrderItems 
                    WHERE OrderItems.OrderId = Orders.Id 
                    LIMIT 1
                ),
                Amount = (
                    SELECT Amount 
                    FROM OrderItems 
                    WHERE OrderItems.OrderId = Orders.Id 
                    LIMIT 1
                )
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ItemId",
                table: "Orders",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Items_ItemId",
                table: "Orders",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropTable(
                name: "OrderItems");
        }
    }
}
