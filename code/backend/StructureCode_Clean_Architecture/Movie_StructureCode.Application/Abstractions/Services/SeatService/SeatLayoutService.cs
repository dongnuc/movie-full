using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Domain.Entities;

namespace Movie_StructureCode.Application.Abstractions.Services.SeatService
{
    public class SeatLayoutService : ISeatLayoutService
    {
        public void RecalculateSeatNumbers(List<Seat> rowSeats, Guid coupleSeatTypeId)
        {
            if (rowSeats == null || !rowSeats.Any())
                return;

            // đảm bảo đúng thứ tự trái → phải
            var orderedSeats = rowSeats
                .OrderBy(s => s.ColIndex)
                .ToList();

            int displayNumber = 1;

            for (int i = 0; i < orderedSeats.Count; i++)
            {
                var currentSeat = orderedSeats[i];

                var isCouple = currentSeat.SeatTypeId == coupleSeatTypeId;

                if (isCouple)
                {
                    // kiểm tra ghế kế bên có cùng loại không
                    if (i + 1 < orderedSeats.Count &&
                        orderedSeats[i + 1].SeatTypeId == coupleSeatTypeId)
                    {
                        // gán cùng SeatNumber cho 2 ghế
                        currentSeat.UpdateSeatNumber(displayNumber);
                        orderedSeats[i + 1].UpdateSeatNumber(displayNumber);

                        i++; // skip ghế tiếp theo vì đã xử lý
                    }
                    else
                    {
                        // ❗ edge case: ghế couple bị lẻ (data lỗi)
                        currentSeat.UpdateSeatNumber(displayNumber);
                    }

                    displayNumber++;
                }
                else
                {
                    // ghế thường → mỗi ghế 1 số
                    currentSeat.UpdateSeatNumber(displayNumber);
                    displayNumber++;
                }
            }
        }
    }
}
