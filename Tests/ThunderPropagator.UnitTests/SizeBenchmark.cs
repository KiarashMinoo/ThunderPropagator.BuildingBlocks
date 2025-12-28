using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.UnitTests;

[MemoryDiagnoser]
public class SizeBenchmark
{
    private List<int>? _largeList;
    private int[]? _largeArray;
    private Dictionary<string, int>? _largeDict;
    private LargeObject? _largeObject;
    private List<TestObject>? _largeObjectList;

    public class LargeObject
    {
        public List<int> LargeList { get; set; }
        public int[] LargeArray { get; set; }
        public Dictionary<string, int> LargeDict { get; set; }
    }

    public class TestObject
    {
        public int Value { get; set; }
        public string? Name { get; set; }
    }

    [GlobalSetup]
    public void Setup()
    {
        _largeList = new List<int>(100000);
        for (int i = 0; i < 100000; i++)
        {
            _largeList.Add(i);
        }
        _largeArray = _largeList.ToArray();
        _largeDict = new Dictionary<string, int>(10000);
        for (int i = 0; i < 10000; i++)
        {
            _largeDict[i.ToString()] = i;
        }
        _largeObject = new LargeObject
        {
            LargeList = _largeList,
            LargeArray = _largeArray,
            LargeDict = _largeDict
        };
        _largeObjectList = new List<TestObject>(10000);
        for (int i = 0; i < 10000; i++)
        {
            _largeObjectList.Add(new TestObject { Value = i, Name = $"Name{i}" });
        }
    }

    [Benchmark]
    public async Task<long> CalculateLargeList()
    {
        return await Size.Calculate(_largeList!);
    }

    [Benchmark]
    public async Task<long> CalculateLargeArray()
    {
        return await Size.Calculate(_largeArray!);
    }

    [Benchmark]
    public async Task<long> CalculateLargeDict()
    {
        return await Size.Calculate(_largeDict!);
    }

    [Benchmark]
    public async Task<long> CalculateLargeObject()
    {
        return await Size.Calculate(_largeObject!);
    }

    [Benchmark]
    public async Task<long> CalculateLargeObjectList()
    {
        return await Size.Calculate(_largeObjectList!);
    }
}