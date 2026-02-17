// Copyright 2025-present the zvec project — Pure C# Engine
// Benchmark suite — measures insert throughput, query latency, memory, and quantization

using System.Diagnostics;
using Foundation.AI.Zvec;
using Foundation.AI.Zvec.Engine.Math;

namespace Foundation.AI.Zvec;

internal class Program
{
    static void Main()
    {
        const int DIM = 128;
        const int DOC_COUNT = 10_000;
        const int QUERY_COUNT = 100;
        const int TOPK = 10;

        var testPath = Path.Combine(Path.GetTempPath(), $"zvec_bench_{Guid.NewGuid():N}");
        var rng = new Random(42);

        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║     Zvec Pure C# Engine — Benchmark Suite    ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.WriteLine($"  Vectors: {DOC_COUNT:N0} × {DIM}D FP32");
        Console.WriteLine($"  Queries: {QUERY_COUNT} × top-{TOPK}");
        Console.WriteLine();

        try
        {
            // ── 1. Schema & collection creation ────────────────────────────

            var schema = new CollectionSchema("bench_collection")
                .AddField("id", DataType.String)
                .AddVector("embedding", DataType.VectorFP32, (uint)DIM,
                    new HnswIndexParams(Zvec.MetricType.IP, m: 16, efConstruction: 200));

            using var collection = ZvecCollection.CreateAndOpen(testPath, schema);

            // ── 2. Generate random vectors ─────────────────────────────────

            Console.Write("Generating vectors... ");
            var vectors = new float[DOC_COUNT][];
            for (int i = 0; i < DOC_COUNT; i++)
            {
                vectors[i] = new float[DIM];
                for (int d = 0; d < DIM; d++)
                    vectors[i][d] = (float)(rng.NextDouble() * 2 - 1);

                // Normalize for cosine/IP
                float norm = 0;
                for (int d = 0; d < DIM; d++) norm += vectors[i][d] * vectors[i][d];
                norm = MathF.Sqrt(norm);
                if (norm > 0)
                    for (int d = 0; d < DIM; d++) vectors[i][d] /= norm;
            }
            Console.WriteLine("OK");

            // ── 3. Insert benchmark ────────────────────────────────────────

            Console.Write($"Inserting {DOC_COUNT:N0} documents... ");
            var sw = Stopwatch.StartNew();

            // Batch insert in chunks of 1000
            const int batchSize = 1000;
            for (int batch = 0; batch < DOC_COUNT; batch += batchSize)
            {
                int end = System.Math.Min(batch + batchSize, DOC_COUNT);
                var docs = new ZvecDoc[end - batch];
                for (int i = batch; i < end; i++)
                {
                    docs[i - batch] = new ZvecDoc($"doc_{i}")
                        .Set("id", $"doc_{i}")
                        .Set("embedding", vectors[i]);
                }
                collection.Insert(docs);
            }

            sw.Stop();
            double insertMs = sw.Elapsed.TotalMilliseconds;
            double insertThroughput = DOC_COUNT / sw.Elapsed.TotalSeconds;
            Console.WriteLine($"{insertMs:F0}ms ({insertThroughput:F0} docs/sec)");

            // ── 4. Flush benchmark ─────────────────────────────────────────

            Console.Write("Flushing to disk... ");
            sw.Restart();
            collection.Flush();
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed.TotalMilliseconds:F0}ms");

            // ── 5. Query benchmark (HNSW) ──────────────────────────────────

            Console.Write($"Vector queries ({QUERY_COUNT}x top-{TOPK})... ");
            var queryVectors = new float[QUERY_COUNT][];
            for (int q = 0; q < QUERY_COUNT; q++)
            {
                queryVectors[q] = new float[DIM];
                for (int d = 0; d < DIM; d++)
                    queryVectors[q][d] = (float)(rng.NextDouble() * 2 - 1);
            }

            sw.Restart();
            int totalResults = 0;
            for (int q = 0; q < QUERY_COUNT; q++)
            {
                using var results = collection.Query("embedding", queryVectors[q], topk: TOPK);
                totalResults += results.Count;
            }
            sw.Stop();
            double queryMs = sw.Elapsed.TotalMilliseconds;
            double queryAvgUs = (queryMs / QUERY_COUNT) * 1000;
            Console.WriteLine($"{queryMs:F0}ms total, {queryAvgUs:F0}µs/query avg, {totalResults} total hits");

            // ── 6. Filtered query benchmark ────────────────────────────────

            Console.Write($"Filtered queries ({QUERY_COUNT}x)... ");
            sw.Restart();
            int filteredResults = 0;
            for (int q = 0; q < QUERY_COUNT; q++)
            {
                // Filter on id pattern (exercises filter parsing + eval)
                using var results = collection.Query("embedding", queryVectors[q],
                    topk: TOPK, filter: $"id != \"doc_{q}\"");
                filteredResults += results.Count;
            }
            sw.Stop();
            double filteredMs = sw.Elapsed.TotalMilliseconds;
            double filteredAvgUs = (filteredMs / QUERY_COUNT) * 1000;
            Console.WriteLine($"{filteredMs:F0}ms total, {filteredAvgUs:F0}µs/query avg");

            // ── 7. Fetch benchmark ─────────────────────────────────────────

            Console.Write($"Fetch by PK ({QUERY_COUNT}x)... ");
            sw.Restart();
            for (int q = 0; q < QUERY_COUNT; q++)
            {
                var fetched = collection.Fetch($"doc_{rng.Next(DOC_COUNT)}");
            }
            sw.Stop();
            Console.WriteLine($"{sw.Elapsed.TotalMilliseconds:F0}ms total, {(sw.Elapsed.TotalMilliseconds / QUERY_COUNT * 1000):F0}µs/fetch avg");

            // ── 8. Quantization benchmark ──────────────────────────────────

            Console.WriteLine();
            Console.WriteLine("── Quantization Roundtrip ──────────────────────");

            var testVec = vectors[0];

            // FP16
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var fp16 = Quantization.ToFP16(testVec);
                var back = Quantization.FromFP16(fp16);
            }
            sw.Stop();
            var fp16Result = Quantization.FromFP16(Quantization.ToFP16(testVec));
            float fp16Error = MaxAbsError(testVec, fp16Result);
            Console.WriteLine($"  FP16: {sw.Elapsed.TotalMilliseconds / 10:F1}µs/roundtrip, max error={fp16Error:E2}");

            // INT8
            var int8Cal = Quantization.CalibrateInt8(testVec);
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var q8 = Quantization.ToInt8(testVec, int8Cal);
                var back = Quantization.FromInt8(q8, int8Cal);
            }
            sw.Stop();
            var int8Result = Quantization.FromInt8(Quantization.ToInt8(testVec, int8Cal), int8Cal);
            float int8Error = MaxAbsError(testVec, int8Result);
            Console.WriteLine($"  INT8: {sw.Elapsed.TotalMilliseconds / 10:F1}µs/roundtrip, max error={int8Error:E2}");

            // INT4
            var int4Cal = Quantization.CalibrateInt4(testVec);
            sw.Restart();
            for (int i = 0; i < 10000; i++)
            {
                var q4 = Quantization.ToInt4(testVec, int4Cal);
                var back = Quantization.FromInt4(q4, testVec.Length, int4Cal);
            }
            sw.Stop();
            var int4Result = Quantization.FromInt4(Quantization.ToInt4(testVec, int4Cal), testVec.Length, int4Cal);
            float int4Error = MaxAbsError(testVec, int4Result);
            Console.WriteLine($"  INT4: {sw.Elapsed.TotalMilliseconds / 10:F1}µs/roundtrip, max error={int4Error:E2}");

            // Memory savings
            var (fp32Bytes, _, _) = Quantization.MemoryStats(DIM, DOC_COUNT, Zvec.Engine.Core.QuantizeType.Undefined);
            var (_, fp16Bytes, fp16Ratio) = Quantization.MemoryStats(DIM, DOC_COUNT, Zvec.Engine.Core.QuantizeType.FP16);
            var (_, int8Bytes, int8Ratio) = Quantization.MemoryStats(DIM, DOC_COUNT, Zvec.Engine.Core.QuantizeType.Int8);
            var (_, int4Bytes, int4Ratio) = Quantization.MemoryStats(DIM, DOC_COUNT, Zvec.Engine.Core.QuantizeType.Int4);

            Console.WriteLine();
            Console.WriteLine("── Memory Estimates ({0:N0} vectors × {1}D) ────────", DOC_COUNT, DIM);
            Console.WriteLine($"  FP32: {fp32Bytes / 1024.0 / 1024:F1} MB (baseline)");
            Console.WriteLine($"  FP16: {fp16Bytes / 1024.0 / 1024:F1} MB ({fp16Ratio:P0} of FP32)");
            Console.WriteLine($"  INT8: {int8Bytes / 1024.0 / 1024:F1} MB ({int8Ratio:P0} of FP32)");
            Console.WriteLine($"  INT4: {int4Bytes / 1024.0 / 1024:F1} MB ({int4Ratio:P0} of FP32)");

            // ── 8b. Quantized HNSW (INT8) end-to-end ───────────────────────

            Console.WriteLine();
            Console.WriteLine("── Quantized HNSW (INT8) ──────────────────────");

            var qPath = Path.Combine(Path.GetTempPath(), $"zvec_bench_q_{Guid.NewGuid():N}");
            var qSchema = new CollectionSchema("bench_quantized")
                .AddField("id", DataType.String)
                .AddVector("embedding", DataType.VectorFP32, (uint)DIM,
                    new HnswIndexParams(Zvec.MetricType.IP, m: 16, efConstruction: 200,
                        quantize: Zvec.QuantizeType.Int8));

            using (var qCol = ZvecCollection.CreateAndOpen(qPath, qSchema))
            {
                for (int batch = 0; batch < DOC_COUNT; batch += 1000)
                {
                    int end = System.Math.Min(batch + 1000, DOC_COUNT);
                    var docs = new ZvecDoc[end - batch];
                    for (int i = batch; i < end; i++)
                        docs[i - batch] = new ZvecDoc($"doc_{i}")
                            .Set("id", $"doc_{i}")
                            .Set("embedding", vectors[i]);
                    qCol.Insert(docs);
                }

                sw.Restart();
                int qHits = 0;
                for (int q = 0; q < QUERY_COUNT; q++)
                {
                    using var r = qCol.Query("embedding", queryVectors[q], topk: TOPK);
                    qHits += r.Count;
                }
                sw.Stop();
                double qAvgUs = (sw.Elapsed.TotalMilliseconds / QUERY_COUNT) * 1000;
                Console.WriteLine($"  INT8 Query: {sw.Elapsed.TotalMilliseconds:F0}ms total, {qAvgUs:F0}µs/query avg, {qHits} hits");
            }

            try { Directory.Delete(qPath, true); } catch { }


            Console.WriteLine();
            Console.Write("Persistence roundtrip... ");
            collection.Flush();
            collection.Dispose();

            sw.Restart();
            using var reopened = ZvecCollection.Open(testPath);
            sw.Stop();
            Console.WriteLine($"reopened in {sw.Elapsed.TotalMilliseconds:F0}ms (doc count: {reopened.DocCount})");

            // Verify query still works after reopen
            Console.Write("  Query after reopen... ");
            sw.Restart();
            using var reopenResults = reopened.Query("embedding", queryVectors[0], topk: TOPK);
            sw.Stop();
            Console.WriteLine($"got {reopenResults.Count} results in {sw.Elapsed.TotalMilliseconds:F1}ms");

            reopened.Dispose();

            // ── Summary ────────────────────────────────────────────────────

            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║              Summary                         ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine($"║  Insert:  {insertThroughput,8:F0} docs/sec              ║");
            Console.WriteLine($"║  Query:   {queryAvgUs,8:F0} µs/query (HNSW top-{TOPK})  ║");
            Console.WriteLine($"║  Fetch:   {(sw.Elapsed.TotalMilliseconds / QUERY_COUNT * 1000),8:F0} µs/fetch (by PK)        ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
            if (Directory.Exists(testPath))
                try { Directory.Delete(testPath, recursive: true); } catch { }
        }

        static float MaxAbsError(float[] original, float[] reconstructed)
        {
            float maxErr = 0;
            for (int i = 0; i < original.Length; i++)
            {
                float err = MathF.Abs(original[i] - reconstructed[i]);
                if (err > maxErr) maxErr = err;
            }
            return maxErr;
        }
    }
}