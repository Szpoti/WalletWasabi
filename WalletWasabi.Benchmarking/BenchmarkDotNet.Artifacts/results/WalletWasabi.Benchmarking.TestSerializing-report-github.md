```

BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2428/22H2/2022Update/SunValley2)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 7.0.401
  [Host]     : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2


```
| Method                         | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------------- |----------:|---------:|---------:|-------:|----------:|
| TestMicrosoftDeserializeBig    | 294.76 μs | 5.247 μs | 4.097 μs | 2.4414 |  15.83 KB |
| TestNewtonsoftDeserializeBig   | 286.92 μs | 5.588 μs | 5.227 μs | 2.9297 |  18.59 KB |
| TestMicrosoftSerializeBig      | 195.62 μs | 2.798 μs | 2.618 μs | 2.1973 |  14.17 KB |
| TestNewtonsoftSerializeBig     | 198.27 μs | 2.563 μs | 2.397 μs | 2.9297 |  19.38 KB |
| TestMicrosoftSerializeSmall    |  48.52 μs | 0.449 μs | 0.420 μs | 0.5493 |   3.59 KB |
| TestNewtonsoftSerializeSmall   |  49.14 μs | 0.284 μs | 0.237 μs | 0.9155 |   5.66 KB |
| TestMicrosoftDeserializeSmall  |  70.68 μs | 0.462 μs | 0.386 μs | 0.6104 |   3.98 KB |
| TestNewtonsoftDeserializeSmall |  71.92 μs | 0.710 μs | 0.629 μs | 1.0986 |   6.74 KB |
