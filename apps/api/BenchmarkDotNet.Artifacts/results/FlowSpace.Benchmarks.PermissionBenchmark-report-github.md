```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.5.1 (25F80) [Darwin 25.5.0]
Apple M5 Pro, 1 CPU, 15 logical and 15 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), Arm64 RyuJIT armv8.0-a


```
| Method                  | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| FirstCall_DbLookup      | 6,364.8 ns | 125.17 ns | 249.98 ns |  1.00 |    0.05 | 1.4648 |   12464 B |        1.00 |
| SubsequentCall_CacheHit |   106.3 ns |   1.56 ns |   1.39 ns |  0.02 |    0.00 | 0.0678 |     568 B |        0.05 |
