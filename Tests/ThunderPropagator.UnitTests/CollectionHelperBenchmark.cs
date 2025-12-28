using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.UnitTests;

[MemoryDiagnoser]
public class CollectionHelperBenchmark
{
    private int[]? _largeArray;
    private List<int>? _largeList;

    [GlobalSetup]
    public void Setup()
    {
        _largeArray = new int[100000];
        for (int i = 0; i < 100000; i++)
        {
            _largeArray[i] = i;
        }
        _largeList = new List<int>(_largeArray);
    }

    [Benchmark]
    public void Filter_Array()
    {
        var result = _largeArray!.Filter(x => x % 2 == 0);
        // Consume result to avoid dead code elimination
        _ = result.Count;
    }

    [Benchmark]
    public void Filter_List()
    {
        var result = _largeList!.Filter(x => x % 2 == 0);
        _ = result.Count;
    }

    [Benchmark]
    public void Convert_Array()
    {
        var result = _largeArray!.Convert(x => x * 2);
        _ = result!.Length;
    }

    [Benchmark]
    public void ForEach_Array()
    {
        int sum = 0;
        _largeArray!.ForEach(x => sum += x);
        _ = sum;
    }

    [Benchmark]
    public void ForEach_List()
    {
        int sum = 0;
        _largeList!.ForEach(x => sum += x);
        _ = sum;
    }

    [Benchmark]
    public void Linq_Filter_Array()
    {
        var result = _largeArray!.Where(x => x % 2 == 0).ToArray();
        _ = result.Length;
    }

    [Benchmark]
    public void Linq_Convert_Array()
    {
        var result = _largeArray!.Select(x => x * 2).ToArray();
        _ = result.Length;
    }

    [Benchmark]
    public void Linq_ForEach_Array()
    {
        int sum = 0;
        foreach (var x in _largeArray!)
        {
            sum += x;
        }
        _ = sum;
    }

    [Benchmark]
    public void ForEach_LinkedArray()
    {
        var linkedArray = _largeArray!.Filter(x => x % 2 == 0);
        int sum = 0;
        linkedArray.ForEach(x => sum += x);
        _ = sum;
    }
}