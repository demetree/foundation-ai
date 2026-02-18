using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Foundation.Controllers;
using Foundation.Security;
using BMC.AI;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// AI-powered endpoints for semantic search and RAG chat over BMC's LEGO data.
    ///
    /// Provides:
    ///   - Semantic part search (natural language → ranked parts)
    ///   - Semantic set search  (natural language → ranked sets)
    ///   - RAG chat             (question → grounded answer with sources)
    ///   - Index trigger        (admin-only: re-indexes parts/sets into the vector store)
    ///
    /// All endpoints require BMC read permission. Index requires write permission.
    /// </summary>
    public class AiController : SecureWebAPIController
    {
        public const int READ_PERMISSION_LEVEL_REQUIRED = 1;
        public const int WRITE_PERMISSION_LEVEL_REQUIRED = 2;

        private readonly IBmcAiService _aiService;
        private readonly BmcSearchIndex _searchIndex;
        private readonly ILogger<AiController> _logger;

        public AiController(
            IBmcAiService aiService,
            BmcSearchIndex searchIndex,
            ILogger<AiController> logger
        ) : base("BMC", "AI")
        {
            _aiService = aiService;
            _searchIndex = searchIndex;
            _logger = logger;
        }


        #region DTOs

        /// <summary>
        /// Search result DTO returned to the client.
        /// </summary>
        public class AiSearchResultDto
        {
            public string id { get; set; }
            public float score { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string category { get; set; }
            public string year { get; set; }
        }

        /// <summary>
        /// Chat request body.
        /// </summary>
        public class AiChatRequest
        {
            public string question { get; set; }
        }

        /// <summary>
        /// Chat response DTO.
        /// </summary>
        public class AiChatResponseDto
        {
            public string answer { get; set; }
            public List<AiChatSourceDto> sources { get; set; }
        }

        /// <summary>
        /// Source reference in a chat response.
        /// </summary>
        public class AiChatSourceDto
        {
            public string docId { get; set; }
            public string excerpt { get; set; }
            public float score { get; set; }
        }

        /// <summary>
        /// Index result DTO.
        /// </summary>
        public class AiIndexResultDto
        {
            public bool success { get; set; }
            public string message { get; set; }
        }

        #endregion


        /// <summary>
        /// GET /api/ai/search/parts?query=...&amp;topK=10
        ///
        /// Semantic search over all indexed LEGO parts.
        /// Returns the top-K most semantically similar parts to the query.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ai/search/parts")]
        public async Task<IActionResult> SearchParts(
            string query,
            int topK = 10,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }

            if (topK < 1) topK = 1;
            if (topK > 100) topK = 100;

            try
            {
                var results = await _aiService.SearchPartsAsync(query, topK, cancellationToken);

                var dtos = results.Select(r => new AiSearchResultDto
                {
                    id = r.Id,
                    score = r.Score,
                    name = r.Name,
                    type = "part",
                    category = r.Category,
                    year = r.Year
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI parts search for query: {Query}", query);
                return StatusCode(500, "AI search failed. Please try again.");
            }
        }


        /// <summary>
        /// GET /api/ai/search/sets?query=...&amp;topK=10
        ///
        /// Semantic search over all indexed LEGO sets.
        /// Returns the top-K most semantically similar sets to the query.
        /// </summary>
        [HttpGet]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ai/search/sets")]
        public async Task<IActionResult> SearchSets(
            string query,
            int topK = 10,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }

            if (topK < 1) topK = 1;
            if (topK > 100) topK = 100;

            try
            {
                var results = await _aiService.SearchSetsAsync(query, topK, cancellationToken);

                var dtos = results.Select(r => new AiSearchResultDto
                {
                    id = r.Id,
                    score = r.Score,
                    name = r.Name,
                    type = "set",
                    category = r.Category,
                    year = r.Year
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI sets search for query: {Query}", query);
                return StatusCode(500, "AI search failed. Please try again.");
            }
        }


        /// <summary>
        /// POST /api/ai/chat
        ///
        /// RAG-powered chat: asks a question about LEGO parts, sets, or building techniques.
        /// The AI grounds its answer in the indexed BMC data.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.TwoPerSecond, Scope = RateLimitScope.PerUser)]
        [Route("api/ai/chat")]
        public async Task<IActionResult> Chat(
            [FromBody] AiChatRequest request,
            CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.question))
            {
                return BadRequest("Question is required.");
            }

            try
            {
                var response = await _aiService.ChatAsync(request.question, cancellationToken);

                var dto = new AiChatResponseDto
                {
                    answer = response.Answer,
                    sources = response.Sources?.Select(s => new AiChatSourceDto
                    {
                        docId = s.DocId,
                        excerpt = s.Excerpt,
                        score = s.Score
                    }).ToList() ?? new List<AiChatSourceDto>()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI chat for question: {Question}", request.question);
                return StatusCode(500, "AI chat failed. Please try again.");
            }
        }


        /// <summary>
        /// POST /api/ai/index
        ///
        /// Admin endpoint: triggers a full re-index of parts and sets into the vector store.
        /// This should be run after importing new data from Rebrickable.
        /// Requires write permission.
        /// </summary>
        [HttpPost]
        [RateLimit(RateLimitOption.OnePerMinute, Scope = RateLimitScope.Global)]
        [Route("api/ai/index")]
        public async Task<IActionResult> IndexData(CancellationToken cancellationToken = default)
        {
            if (await DoesUserHaveWritePrivilegeSecurityCheckAsync(WRITE_PERMISSION_LEVEL_REQUIRED, cancellationToken) == false)
            {
                return Forbid();
            }

            try
            {
                _logger.LogInformation("AI index triggered by user.");
                await _searchIndex.IndexAllAsync(ct: cancellationToken);

                return Ok(new AiIndexResultDto
                {
                    success = true,
                    message = "Index rebuild complete."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI index rebuild.");
                return StatusCode(500, new AiIndexResultDto
                {
                    success = false,
                    message = "Index rebuild failed. Check server logs."
                });
            }
        }
    }
}
