using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications
{
    public
#if !DEBUG
        sealed
#endif
        class TelemetryTests
    {
        [Fact]
        public void Version_HasNonEmptyDefault()
        {
            Assert.NotEmpty(Telemetry.Version);
        }

        [Fact]
        public void Configure_IsIdempotent_SecondCallDoesNotChangeVersion()
        {
            // Capture the version after the first Configure wins (may already be set by a prior
            // test or app startup — we do not control which Configure call wins the race).
            Telemetry.Configure("version-first");
            var versionAfterFirst = Telemetry.Version;

            // A second call with a different value must have no effect.
            Telemetry.Configure("version-second");

            Assert.Equal(versionAfterFirst, Telemetry.Version);
            Assert.NotEqual("version-second", Telemetry.Version);
        }

        [Fact]
        public void Configure_VersionDoesNotDrift_AcrossRepeatedCalls()
        {
            // Establish a baseline — whichever Configure wins the one-time slot, capture it.
            Telemetry.Configure("baseline");
            var baseline = Telemetry.Version;

            // All subsequent calls must be no-ops regardless of the value supplied.
            for (var i = 0; i < 10; i++)
                Telemetry.Configure($"drift-attempt-{i}");

            Assert.Equal(baseline, Telemetry.Version);
        }
    }
}
