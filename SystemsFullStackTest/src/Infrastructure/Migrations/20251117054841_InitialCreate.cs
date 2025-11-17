using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Canteens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canteens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    WalletBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CanteenSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanteenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    CutoffTime = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanteenSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanteenSchedules_Canteens_CanteenId",
                        column: x => x.CanteenId,
                        principalTable: "Canteens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CanteenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DailyStockCount = table.Column<int>(type: "INTEGER", nullable: true),
                    AllergenTags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Canteens_CanteenId",
                        column: x => x.CanteenId,
                        principalTable: "Canteens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Allergen = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CanteenId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FulfilmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Canteens_CanteenId",
                        column: x => x.CanteenId,
                        principalTable: "Canteens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanteenSchedules_CanteenId_DayOfWeek",
                table: "CanteenSchedules",
                columns: new[] { "CanteenId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CanteenId",
                table: "MenuItems",
                column: "CanteenId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CanteenId",
                table: "Orders",
                column: "CanteenId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdempotencyKey",
                table: "Orders",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ParentId",
                table: "Orders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StudentId",
                table: "Orders",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Parents_Email",
                table: "Parents",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentId",
                table: "Students",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanteenSchedules");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Canteens");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Parents");
        }
    }
}
