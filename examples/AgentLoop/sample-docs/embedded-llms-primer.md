# A Primer on Embedded LLMs

Embedded large language models -- LLMs that run inside your application process rather than behind a remote API -- have become genuinely practical for a wide range of workloads since 2024, driven by aggressive quantisation research and a Cambrian explosion of small models in the 1-to-8 billion parameter range. This primer covers the four concepts you actually have to understand before integrating one into a .NET application.

## Quantisation

A model published with full 16-bit floating-point weights consumes two bytes per parameter, so a 7-billion-parameter network weighs 14 GB on disk and in memory. Quantisation reduces the bit width of each weight, sometimes dramatically. The most common levels are INT8 (one byte per parameter, half the size, virtually no quality loss), INT4 (half a byte per parameter, four times smaller than the original, modest quality loss), and below that INT3 and INT2 with progressively rougher trade-offs. The cutting edge is BitNet, where each weight is constrained to one of three values (-1, 0, +1) at training time, allowing for inference kernels that replace floating-point multiply with addition and selection.

A 4-bit-quantised 7-billion-parameter model fits in roughly 4 GB of memory and runs on a mid-range laptop CPU at conversational speed. That is the regime in which embedded LLMs become practically interesting.

## File formats

The two formats you will encounter in practice are GGUF (the flat binary format used by llama.cpp and its many bindings) and ONNX (the broader interchange format used by ONNX Runtime). GGUF is purpose-built for quantised LLM inference and tends to be the first format new community quantisations are published in. ONNX is a more general format that also works for vision models, classical ML, and embeddings; the GenAI extension to ONNX Runtime adds the autoregressive token-generation loop and KV-cache management that LLMs require.

For most .NET applications, ONNX via the official Microsoft.ML.OnnxRuntimeGenAI package is the path of least resistance, because the runtime ships with first-class managed bindings and a good selection of pre-converted models on HuggingFace. GGUF via LLamaSharp is the alternative for projects that want the broader llama.cpp model ecosystem.

## Tokens and context windows

LLMs see text as a sequence of tokens, not characters or words. A token is a sub-word unit produced by the model's tokeniser; English text averages roughly four characters per token. The context window is the maximum number of tokens the model can hold in attention at once, including both the prompt and the generated reply. Smaller embedded models typically have 4-K to 32-K context windows; the latest are pushing to 128-K and beyond.

Why this matters in practice: chunking a long document into pieces small enough to fit in the context window -- with enough overlap to preserve sentence boundaries -- is the central design decision in RAG systems. Too-large chunks waste retrieval relevance; too-small chunks lose semantic coherence.

## Retrieval-augmented generation

RAG is the pattern of stuffing relevant document excerpts into the model's prompt at query time, rather than fine-tuning the model on those documents. It became dominant because it is dramatically simpler than fine-tuning, gives source citations for free, and adapts immediately to new content without retraining. The plumbing is straightforward: chunk the documents, embed each chunk into a fixed-length vector, store the vectors, and at query time embed the question and find the most similar chunks. Foundation.AI's `IRagService` packages exactly this flow.
