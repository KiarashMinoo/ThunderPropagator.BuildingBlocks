namespace ThunderPropagator.BuildingBlocks.Application.Enums
{
    [Flags]
    public enum RecoveryStorage
    {
        None = 0,
        Redis = 1,
        MongoDb = 2,
        Postgresql = 4,
    }
}