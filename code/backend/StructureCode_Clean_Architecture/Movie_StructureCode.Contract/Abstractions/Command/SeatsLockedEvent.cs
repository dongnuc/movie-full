namespace Movie_StructureCode.Contract.Abstractions.Command
{
   public sealed class SeatsLockedEvent
    {
        public Guid ShowingId { get; init; }
        public List<Guid> SeatIds { get; init; } = [];
        public string LockedBy { get; init; } = default!;
    }

    public sealed class SeatsUnlockedEvent
    {
        public Guid ShowingId { get; init; }
        public List<Guid> SeatIds { get; init; } = [];
    }
}
