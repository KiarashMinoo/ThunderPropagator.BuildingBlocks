namespace ThunderPropagator.BuildingBlocks.Application.Enums
{
    public enum DataType
    {
        String = 1,

        /// <summary>
        /// C# = Long
        /// JS = Number
        /// Ex: 10000000000000000
        /// </summary>
        Number,

        /// <summary>
        /// Ex: 100000000000.01
        /// </summary>
        Decimal,

        /// <summary>
        /// Ex: 12%
        /// </summary>
        Percent,

        /// <summary>
        /// Ex: 100,000,000
        /// </summary>
        Currency,

        /// <summary>
        /// Ex: 2024-01-01T12:00:00.000Z
        /// </summary>
        DateTime,

        /// <summary>
        /// Ex: 2024-01-01
        /// </summary>
        Date,

        /// <summary>
        /// Ex: 12:00:00.000
        /// </summary>
        Time,

        /// <summary>
        /// True/False
        /// </summary>
        Boolean,
        Enum,
        Json
    }
}