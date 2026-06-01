namespace Movie_StructureCode.Contract.Abstractions.Command
{
    public class RedisSettings
    {
        public string Connection { get; set; } = default!;
        public string InstanceName { get; set; } = default!;
        public int DefaultDatabase { get; set; }
    }
}
