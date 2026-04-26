# .NET 10 Highlights for AI-Adjacent Workloads

.NET 10, released in late 2025, brought a number of features that are particularly useful for applications that embed AI capabilities. This is a brief tour of the ones that come up most often in practice.

## SLNX -- a new solution file format

The legacy .sln file format, with its nested GUIDs and per-project metadata, has been replaced as the default by SLNX, a small and human-readable XML format. `dotnet new sln` now produces a `.slnx` file by default. Both formats remain supported and interoperable, so existing solutions continue to work, but new projects can take advantage of the cleaner format. Visual Studio 17.10 and later, JetBrains Rider, and the dotnet command line all support SLNX natively.

## Improved JIT vector intrinsics

The RyuJIT compiler now has substantially better support for Vector256 and Vector512 intrinsics on x86_64, including AVX-512 dispatch on the increasingly common Sapphire Rapids and Zen 4 generations of server CPUs. For embedded vector databases such as Zvec, where similarity search is dominated by dot-product loops, this translates into measurable real-world throughput gains without any code changes -- the same managed C# kernels just compile to better native code.

## First-class HTTP client improvements

`HttpClient` gained native support for HTTP/3 over QUIC and improved pipelining behaviour. For applications that hit OpenAI-compatible endpoints (real OpenAI, Azure OpenAI, Ollama, LM Studio, vLLM, and others) this means lower per-request overhead and better behaviour under bursty load. Streaming completion responses arrive with less buffering and lower jitter.

## Microsoft.Extensions.AI

Microsoft introduced a set of provider-neutral abstractions for chat completion, embedding, and tool-calling under the `Microsoft.Extensions.AI` namespace. The library is still settling at the time of writing -- its abstractions are deliberately heavy on options-pattern configuration and tend to push application code toward an opinionated middleware shape. Whether it eventually becomes the .NET standard is an open question.

Foundation.AI predates Microsoft.Extensions.AI and takes a different design direction: smaller interfaces, less ceremony, opinionated defaults baked into the providers themselves. The two stacks are not in opposition -- they target different sweet spots -- but new projects evaluating .NET AI infrastructure now have a real choice.

## Native AOT for console and worker apps

Native ahead-of-time compilation became substantially more practical for plain console and worker applications, with much-improved trimming heuristics and far fewer reflection-trap warnings from the Microsoft.* libraries. For embedded LLM workloads this means you can publish a single self-contained executable -- including the embedded vector database, the embedding model loader, and the inference runtime -- with no separate .NET install required on the target machine.
