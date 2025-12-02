```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7171)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                   | Mean         | Error       | StdDev      | Gen0     | Gen1     | Gen2     | Allocated |
|------------------------- |-------------:|------------:|------------:|---------:|---------:|---------:|----------:|
| CalculateLargeList       |     1.665 μs |   0.0330 μs |   0.0463 μs |   0.1335 |   0.1297 |        - |     856 B |
| CalculateLargeArray      |     1.319 μs |   0.0229 μs |   0.0297 μs |   0.0954 |   0.0916 |        - |     616 B |
| CalculateLargeDict       | 5,161.396 μs | 102.7721 μs | 182.6773 μs | 539.0625 | 531.2500 | 328.1250 | 2624614 B |
| CalculateLargeObject     | 5,005.932 μs |  96.0548 μs | 128.2304 μs | 539.0625 | 531.2500 | 328.1250 | 2624932 B |
| CalculateLargeObjectList | 2,834.777 μs |  61.1124 μs | 171.3658 μs | 429.6875 | 425.7813 | 328.1250 | 1904486 B |
