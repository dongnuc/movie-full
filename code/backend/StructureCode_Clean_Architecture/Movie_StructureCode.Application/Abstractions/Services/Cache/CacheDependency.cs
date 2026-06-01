namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public class CacheDependency
    {
        public string Entity { get; set; } = string.Empty;
        public bool IsGlobal { get; set; }
        public Func<object, Guid?>? IdSelector { get; set; } 
    }
}
