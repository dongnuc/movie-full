namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public class CacheDependency
    {
        public string Entity { get; set; } = string.Empty;
        // IsGlobal = true mean all cache relate entity cache
        // will be removed must be get data from db
        // using for getAll will be mark = true(getALlRooom)
        // mark = false when use IdSelector(getRoomById,updateRoomById,deleteRoomById)
        public bool IsGlobal { get; set; }
        // IdSelector = null mean all cache relate entity cache
        public Func<object, Guid?>? IdSelector { get; set; }
    }
}
