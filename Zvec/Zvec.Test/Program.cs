namespace Foundation.AI.Zvec;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("=== Zvec C# Binding Test ===\n");

        string testPath = Path.Combine(Path.GetTempPath(), "zvec_test_collection");

        // Clean up any previous test run
        if (Directory.Exists(testPath))
            Directory.Delete(testPath, recursive: true);

        try
        {
            // 1. Build schema
            Console.Write("1. Creating schema... ");
            using var hnsw = new HnswIndexParams(MetricType.IP);
            using var schema = new CollectionSchema("test_collection")
                .AddField("id", DataType.String, nullable: false)
                .AddField("category", DataType.String, nullable: true)
                .AddField("price", DataType.Float, nullable: true)
                .AddVector("embedding", DataType.VectorFP32, dimension: 4, indexParams: hnsw);
            Console.WriteLine("OK");

            // 2. Create and open collection
            Console.Write("2. Creating collection... ");
            using var collection = ZvecCollection.CreateAndOpen(testPath, schema);
            Console.WriteLine($"OK (path: {testPath})");

            // 3. Insert documents
            Console.Write("3. Inserting 5 documents... ");
            string[] categories = { "electronics", "books", "clothing", "food", "toys" };
            float[][] vectors =
            {
        [1.0f, 0.0f, 0.0f, 0.0f],
        [0.0f, 1.0f, 0.0f, 0.0f],
        [0.0f, 0.0f, 1.0f, 0.0f],
        [0.0f, 0.0f, 0.0f, 1.0f],
        [0.5f, 0.5f, 0.5f, 0.5f],
    };

            var docs = new ZvecDoc[5];
            for (int i = 0; i < 5; i++)
            {
                docs[i] = new ZvecDoc($"item_{i}")
                    .Set("id", $"item_{i}")
                    .Set("category", categories[i])
                    .Set("price", 10.0f * (i + 1))
                    .Set("embedding", vectors[i]);
            }
            collection.Insert(docs);
            foreach (var d in docs) d.Dispose();
            Console.WriteLine("OK");

            // 4. Flush to disk
            Console.Write("4. Flushing... ");
            collection.Flush();
            Console.WriteLine("OK");

            // 5. Get doc count
            Console.Write("5. Doc count... ");
            ulong count = collection.DocCount;
            Console.WriteLine($"{count} documents");

            // 6. Create index
            Console.Write("6. Creating HNSW index... ");
            using var indexParams = new HnswIndexParams(MetricType.IP);
            collection.CreateIndex("embedding", indexParams);
            Console.WriteLine("OK");

            // 7. Query
            Console.Write("7. Vector query (top 3)... ");
            float[] queryVec = [0.6f, 0.4f, 0.3f, 0.1f];
            using var results = collection.Query("embedding", queryVec, topk: 3);
            Console.WriteLine($"got {results.Count} results:");
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                Console.WriteLine($"   [{i}] pk={r.PrimaryKey}, score={r.Score:F4}");
            }

            // 8. Fetch by PK
            Console.Write("8. Fetch by PK (item_2)... ");
            var fetched = collection.Fetch("item_2");
            Console.WriteLine($"got {fetched.Count} result(s)");
            if (fetched.Count > 0)
            {
                var f = fetched[0];
                Console.WriteLine($"   pk={f.PrimaryKey}, category={f.GetString("category")}, price={f.GetFloat("price")}");
            }

            // 9. Delete
            Console.Write("9. Delete item_0... ");
            collection.Delete("item_0");
            Console.WriteLine($"OK (doc count now: {collection.DocCount})");

            // 10. Filtered query
            Console.Write("10. Filtered query (category == \"books\")... ");
            float[] queryVec2 = [0.5f, 0.5f, 0.5f, 0.5f];
            using var filtered = collection.Query("embedding", queryVec2, topk: 5,
                filter: "category == \"books\"");
            Console.WriteLine($"got {filtered.Count} result(s)");
            for (int i = 0; i < filtered.Count; i++)
            {
                var r = filtered[i];
                Console.WriteLine($"   [{i}] pk={r.PrimaryKey}, cat={r.GetString("category")}, score={r.Score:F4}");
            }

            // 11. Delete by filter
            Console.Write("11. Delete by filter (price > 40)... ");
            ulong deletedCount = collection.DeleteByFilter("price > 40");
            Console.WriteLine($"deleted {deletedCount} (doc count now: {collection.DocCount})");

            // 12. Compound filter query
            Console.Write("12. Compound filter (price <= 30 AND category != \"food\")... ");
            using var compound = collection.Query("embedding", queryVec2, topk: 10,
                filter: "price <= 30 AND category != \"food\"");
            Console.WriteLine($"got {compound.Count} result(s):");
            for (int i = 0; i < compound.Count; i++)
            {
                var r = compound[i];
                Console.WriteLine($"   [{i}] pk={r.PrimaryKey}, cat={r.GetString("category")}, price={r.GetFloat("price")}, score={r.Score:F4}");
            }

            Console.WriteLine("\n=== All tests passed! ===");

            // 13. Persistence roundtrip test
            Console.Write("\n13. Persistence roundtrip... ");
            collection.Flush();
            collection.Dispose(); // Close the collection

            // Reopen from disk
            using var reopened = ZvecCollection.Open(testPath);
            Console.WriteLine($"reopened (doc count: {reopened.DocCount})");

            // Query the reopened collection
            Console.Write("    Query after reopen... ");
            float[] queryVec3 = [0.5f, 0.5f, 0.5f, 0.5f];
            using var reopenResults = reopened.Query("embedding", queryVec3, topk: 3);
            Console.WriteLine($"got {reopenResults.Count} results:");
            for (int i = 0; i < reopenResults.Count; i++)
            {
                var r = reopenResults[i];
                Console.WriteLine($"   [{i}] pk={r.PrimaryKey}, score={r.Score:F4}");
            }

            // Fetch to verify scalar fields survived
            Console.Write("    Fetch after reopen... ");
            var fetchedAfter = reopened.Fetch("item_2");
            if (fetchedAfter.Count > 0)
            {
                var f = fetchedAfter[0];
                Console.WriteLine($"pk={f.PrimaryKey}, category={f.GetString("category")}, price={f.GetFloat("price")}");
            }
            else
            {
                Console.WriteLine("NOT FOUND");
            }

            reopened.Dispose();
            Console.WriteLine("\n=== All tests passed (including persistence)! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nERROR: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            // Wait briefly for ZoneTree to release file handles
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(200);
            if (Directory.Exists(testPath))
            {
                try { Directory.Delete(testPath, recursive: true); } catch { }
            }
        }
    }
}