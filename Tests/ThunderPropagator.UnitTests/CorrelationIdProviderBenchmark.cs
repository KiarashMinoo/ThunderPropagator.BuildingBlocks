using BenchmarkDotNet.Attributes;
using ThunderPropagator.BuildingBlocks.Application.CorrelationId;

namespace ThunderPropagator.UnitTests;

[MemoryDiagnoser]
public class CorrelationIdProviderBenchmark
{
    private readonly BenchmarkInput _input = new();

    [GlobalSetup]
    public void Setup()
    {
        // Warm up the type-segment cache so [Benchmark] measures the steady-state hot path.
        _ = _input.GenerateCorrelationId();
    }

    [Benchmark]
    public string GenerateCorrelationId()
    {
        return _input.GenerateCorrelationId();
    }

    private sealed class BenchmarkInput { }
}
