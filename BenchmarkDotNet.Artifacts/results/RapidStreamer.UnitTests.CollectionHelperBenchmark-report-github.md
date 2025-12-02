```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7171)
11th Gen Intel Core i7-1185G7 3.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 8.0.22 (8.0.2225.52707), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.22 (8.0.2225.52707), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method              | Mean      | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated |
|-------------------- |----------:|---------:|---------:|---------:|---------:|---------:|----------:|
| Filter_Array        | 230.27 μs | 4.597 μs | 9.895 μs | 124.7559 | 124.7559 | 124.7559 |  524738 B |
| Filter_List         | 330.64 μs | 3.041 μs | 2.696 μs | 249.5117 | 249.5117 | 249.5117 |  924804 B |
| Convert_Array       | 161.01 μs | 3.110 μs | 6.066 μs | 124.7559 | 124.7559 | 124.7559 |  400066 B |
| ForEach_Array       | 195.91 μs | 3.841 μs | 4.423 μs |        - |        - |        - |     176 B |
| ForEach_List        | 183.50 μs | 1.558 μs | 1.458 μs |        - |        - |        - |      88 B |
| Linq_Filter_Array   | 206.29 μs | 4.074 μs | 8.856 μs |  99.8535 |  99.8535 |  99.8535 |  462922 B |
| Linq_Convert_Array  | 153.68 μs | 2.796 μs | 4.185 μs | 124.7559 | 124.7559 | 124.7559 |  400114 B |
| Linq_ForEach_Array  |  41.88 μs | 0.499 μs | 0.417 μs |        - |        - |        - |         - |
| ForEach_LinkedArray | 233.99 μs | 4.596 μs | 9.071 μs | 186.5234 | 168.2129 | 167.4805 |  724978 B |
