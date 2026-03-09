using BitNet.Interop;

// ============================================================================
// BitNet C# Wrapper — Sample Application
// ============================================================================
// Usage:
//   dotnet run -- <model-path> [prompt]
//
// Examples:
//   dotnet run -- models/BitNet-b1.58-2B-4T/ggml-model-i2_s.gguf
//   dotnet run -- models/BitNet-b1.58-2B-4T/ggml-model-i2_s.gguf "Explain quantum computing"
// ============================================================================

if (args.Length < 1)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("BitNet C# Wrapper — Sample Application");
    Console.WriteLine("Usage: BitNet.Sample <model-path> [prompt]");
    Console.ResetColor();
    return 1;
}

var modelPath = args[0];
var prompt = args.Length > 1 ? args[1] : "You are a helpful assistant";

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║              BitNet C# Wrapper — Sample                      ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

try
{
    // 1. Initialize the backend
    Console.Write("Initializing backend... ");
    BitNetModel.InitBackend();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("OK");
    Console.ResetColor();

    // 2. Print system info
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"System: {BitNetModel.SystemInfo}");
    Console.WriteLine($"MMap: {BitNetModel.SupportsMmap} | MLock: {BitNetModel.SupportsMlock} | GPU Offload: {BitNetModel.SupportsGpuOffload}");
    Console.ResetColor();
    Console.WriteLine();

    // 3. Load the model
    Console.Write($"Loading model: {modelPath} ... ");
    using var model = new BitNetModel(modelPath, new BitNetModelParams
    {
        GpuLayers = 0,  // CPU inference
    }, new BitNetContextParams
    {
        ContextSize = 2048,
        Threads = Environment.ProcessorCount,
    });
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("OK");
    Console.ResetColor();

    // 4. Print model info
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("── Model Info ─────────────────────────────────────────────");
    Console.ResetColor();
    Console.WriteLine($"  Description:      {model.Description}");
    Console.WriteLine($"  Vocab Size:       {model.VocabSize:N0}");
    Console.WriteLine($"  Embedding Size:   {model.EmbeddingSize:N0}");
    Console.WriteLine($"  Layers:           {model.LayerCount}");
    Console.WriteLine($"  Heads:            {model.HeadCount}");
    Console.WriteLine($"  Train Context:    {model.TrainingContextLength:N0}");
    Console.WriteLine($"  Active Context:   {model.ContextSize:N0}");
    Console.WriteLine($"  Model Size:       {model.ModelSizeBytes / 1024.0 / 1024.0:F1} MB");
    Console.WriteLine($"  Parameters:       {model.ParameterCount / 1_000_000.0:F1}M");
    Console.WriteLine($"  BOS Token:        {model.BosToken}");
    Console.WriteLine($"  EOS Token:        {model.EosToken}");
    Console.WriteLine();

    // 5. Tokenization demo
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("── Tokenization Demo ──────────────────────────────────────");
    Console.ResetColor();
    var sampleText = "Hello, BitNet!";
    var tokens = model.Tokenize(sampleText);
    Console.WriteLine($"  Text: \"{sampleText}\"");
    Console.Write("  Tokens: [");
    Console.Write(string.Join(", ", tokens));
    Console.WriteLine("]");
    var roundTrip = model.Detokenize(tokens);
    Console.WriteLine($"  Round-trip: \"{roundTrip}\"");
    Console.WriteLine();

    // 6. Streaming generation
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("── Generation (streaming) ─────────────────────────────────");
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"  Prompt: \"{prompt}\"");
    Console.ResetColor();
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.White;
    foreach (var piece in model.GenerateTokens(prompt, maxTokens: 256, temperature: 0.8f, topK: 40, topP: 0.95f))
    {
        Console.Write(piece);
    }
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine();

    // 7. Performance stats
    var perf = model.GetPerformanceData();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("── Performance ────────────────────────────────────────────");
    Console.ResetColor();
    Console.WriteLine($"  Prompt eval:  {perf.NPEval} tokens in {perf.TPEvalMs:F1} ms ({(perf.NPEval > 0 ? perf.NPEval / (perf.TPEvalMs / 1000.0) : 0):F1} t/s)");
    Console.WriteLine($"  Generation:   {perf.NEval} tokens in {perf.TEvalMs:F1} ms ({(perf.NEval > 0 ? perf.NEval / (perf.TEvalMs / 1000.0) : 0):F1} t/s)");
    Console.WriteLine($"  Model load:   {perf.TLoadMs:F1} ms");
    Console.WriteLine();

    // 8. Cleanup
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Done! Model disposed automatically.");
    Console.ResetColor();

    BitNetModel.FreeBackend();
    return 0;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\nError: {ex.Message}");
    Console.ResetColor();
    return 1;
}
