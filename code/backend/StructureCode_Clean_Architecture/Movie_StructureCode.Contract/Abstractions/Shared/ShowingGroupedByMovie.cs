using System;
using System.Collections.Generic;

namespace Movie_StructureCode.Contract.Abstractions.Shared
{
    public class ShowingGroupedByMovie
    {
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int Duration { get; set; }
        public List<ShowingGroupItem> Showings { get; set; } = new();
    }

    public class ShowingGroupItem
    {
        public Guid Id { get; set; }
        public DateTime TimeStart { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
