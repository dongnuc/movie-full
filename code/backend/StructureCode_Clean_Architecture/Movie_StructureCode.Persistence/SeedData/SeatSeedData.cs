using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Persistence.SeedData
{
    /// <summary>
    /// Generates seats for Room1 (8 rows × 10 cols).
    /// Row naming: A–H  |  A–E ? Standard, F-G ? VIP, H ? Couple
    /// </summary>
    public static class SeatSeedData
    {
        private static readonly DateTime _seedDate =
            new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static IEnumerable<Seat> GetSeedData()
        {
            var seats = new List<Seat>();

            // ?? Room 1: 8 rows (A-H) × 10 cols ?????????????????????????????
            GenerateSeatsForRoom(
                seats,
                roomId: RoomSeedData.Room1Id,
                totalRows: 8,
                totalCols: 10,
                // row index 0-4 ? Standard, 5-6 ? VIP, 7 ? Couple
                seatTypeSelector: rowIndex => rowIndex switch
                {
                    7 => SeatTypeSeedData.CoupleId,
                    5 or 6 => SeatTypeSeedData.VipId,
                    _ => SeatTypeSeedData.StandardId
                }
            );

            // ?? Room 2: 6 rows (A-F) × 10 cols ?????????????????????????????
            GenerateSeatsForRoom(
                seats,
                roomId: RoomSeedData.Room2Id,
                totalRows: 6,
                totalCols: 10,
                seatTypeSelector: rowIndex => rowIndex switch
                {
                    5 => SeatTypeSeedData.CoupleId,
                    4 => SeatTypeSeedData.VipId,
                    _ => SeatTypeSeedData.StandardId
                }
            );

            // ?? Room 4 (Theater 2): 8 rows × 10 cols ????????????????????????
            GenerateSeatsForRoom(
                seats,
                roomId: RoomSeedData.Room4Id,
                totalRows: 8,
                totalCols: 10,
                seatTypeSelector: rowIndex => rowIndex switch
                {
                    7 => SeatTypeSeedData.CoupleId,
                    5 or 6 => SeatTypeSeedData.VipId,
                    _ => SeatTypeSeedData.StandardId
                }
            );

            return seats;
        }

        // ?? helper ??????????????????????????????????????????????????????????
        private static void GenerateSeatsForRoom(
            List<Seat> seats,
            Guid roomId,
            int totalRows,
            int totalCols,
            Func<int, Guid> seatTypeSelector)
        {
            for (int row = 0; row < totalRows; row++)
            {
                string rowName = ((char)('A' + row)).ToString();

                for (int col = 1; col <= totalCols; col++)
                {
                    seats.Add(Seat.Create(
                        id: Guid.NewGuid(),
                        rowIndex: row + 1,
                        colIndex: col,
                        rowName: rowName,
                        seatNumber: col,
                        groupId: null,
                        seatTypeId: seatTypeSelector(row),
                        roomId: roomId,
                        isActive: true
                    ));
                }
            }
        }
    }
}

