# Semantic Search versus Keyword Search

For most of the history of full-text search, the dominant technique has been keyword matching with statistical relevance ranking -- BM25 being the canonical algorithm. The user types a query, the system tokenises it, and documents that contain those tokens (with appropriate weighting for term frequency, inverse document frequency, and field boosts) come back ranked. This works remarkably well for many tasks. It is fast, deterministic, explainable, and comparatively easy to operate.

Semantic search, in contrast, embeds queries and documents into a high-dimensional vector space where geometric proximity reflects meaning rather than literal word overlap. A user query for "fluffy farm animals from the Andes" should retrieve documents about llamas and alpacas even though those exact words do not appear in the query and the words from the query may not appear in the documents. This is enabled by neural embedding models trained on vast amounts of text to map semantically similar inputs to nearby points in vector space.

## When each one wins

Keyword search outperforms semantic search when the user knows exactly what they want and types the right terms. Looking up an error code, finding a specific file by name, or searching legal documents for a defined term are all jobs that vector embeddings would make worse, not better -- because the deterministic match is the right answer.

Semantic search wins when there is a vocabulary mismatch between the user and the corpus. A help desk where users describe symptoms in everyday language but the knowledge base is written by engineers in technical terms is the canonical example. Onboarding a new employee who has not yet learned the internal jargon. Searching across translated content. Retrieving prior support tickets that describe the same underlying issue with totally different surface wording.

## Hybrid

Production systems increasingly use both, with the keyword and vector results either fused at retrieval time or re-ranked after retrieval. Combining the two captures the strengths of each -- keyword precision on exact matches and semantic recall on paraphrases -- and is generally more robust than either alone for general-purpose search over messy content.

## Cost and latency

Keyword indexes are cheap. An inverted index is small relative to the corpus, fast to build, and incremental updates are straightforward. Vector indexes are more expensive on every dimension: the embeddings themselves are 384 to 4096 floating-point numbers per chunk, the indexes use approximate-nearest-neighbour structures (HNSW, IVF, ScaNN) that are not trivial to keep current under high write throughput, and the embedding model has to actually run for every new document and every query.

For applications where the corpus is updated rarely and queried often, this cost asymmetry is fine. For high-write workloads or extremely large corpora, the operational considerations get harder, and the canonical answer is to keep keyword search as the primary index and use vector search as a re-ranking layer on top of the top-N keyword candidates.
