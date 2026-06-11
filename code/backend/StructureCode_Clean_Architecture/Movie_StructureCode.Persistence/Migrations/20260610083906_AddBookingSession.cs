using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movie_StructureCode.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingSession",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShowingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Extend = table.Column<bool>(type: "bit", nullable: false),
                    GracePeriodEndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReconnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingSession_AppUser_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingSession_Showing_ShowingId",
                        column: x => x.ShowingId,
                        principalTable: "Showing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingSessionSeat",
                columns: table => new
                {
                    BookingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSessionSeat", x => new { x.BookingSessionId, x.SeatId });
                    table.ForeignKey(
                        name: "FK_BookingSessionSeat_BookingSession_BookingSessionId",
                        column: x => x.BookingSessionId,
                        principalTable: "BookingSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingSessionSeat_Seat_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingSession_AppUserId",
                table: "BookingSession",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSession_ShowingId",
                table: "BookingSession",
                column: "ShowingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSessionSeat_SeatId",
                table: "BookingSessionSeat",
                column: "SeatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingSessionSeat");

            migrationBuilder.DropTable(
                name: "BookingSession");
        }
    }
}
