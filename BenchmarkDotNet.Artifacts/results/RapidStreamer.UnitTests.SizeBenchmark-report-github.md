```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7171)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 10.0.0 (10.0.25.52411), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                   | Mean         | Error      | StdDev      | Gen0     | Gen1     | Gen2     | Allocated |
|------------------------- |-------------:|-----------:|------------:|---------:|---------:|---------:|----------:|
| CalculateLargeList       |     1.581 μs |  0.0300 μs |   0.0308 μs |   0.1335 |   0.1297 |        - |     856 B |
| CalculateLargeArray      |     1.355 μs |  0.0264 μs |   0.0314 μs |   0.0973 |   0.0954 |        - |     616 B |
| CalculateLargeDict       | 4,576.342 μs | 89.8047 μs | 164.2131 μs | 539.0625 | 531.2500 | 328.1250 | 2624522 B |
| CalculateLargeObject     | 4,267.975 μs | 79.4148 μs |  77.9959 μs | 539.0625 | 531.2500 | 328.1250 | 2625153 B |
| CalculateLargeObjectList | 2,338.048 μs | 45.9970 μs |  68.8462 μs | 429.6875 | 425.7813 | 328.1250 | 1904536 B |
