using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stocki.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockPriceSubscriptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockPriceSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Ticker = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    LastNotificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPriceSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockPriceSubscriptions_DiscordId_Ticker",
                table: "StockPriceSubscriptions",
                columns: new[] { "DiscordId", "Ticker" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockPriceSubscriptions");
        }
    }
}
