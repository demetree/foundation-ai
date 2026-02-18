using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BMC.AI;

namespace Foundation.BMC.Controllers.WebAPI
{
    /// <summary>
    /// SignalR hub for streaming AI chat responses token-by-token.
    ///
    /// The client invokes <see cref="SendMessage"/> with a question,
    /// and receives tokens via the "ReceiveToken" event as they are generated.
    /// When generation is complete, "ChatComplete" is sent.
    ///
    /// Uses bearer token authentication via the SignalR accessTokenFactory.
    /// </summary>
    [Authorize]
    public class AiChatHub : Hub
    {
        private readonly IBmcAiService _aiService;
        private readonly ILogger<AiChatHub> _logger;

        public AiChatHub(IBmcAiService aiService, ILogger<AiChatHub> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Client invokes this to start a streaming chat session.
        /// Tokens are pushed back via "ReceiveToken" as they arrive.
        /// "ChatComplete" is sent when the response is fully generated.
        /// </summary>
        public async Task SendMessage(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                await Clients.Caller.SendAsync("ChatError", "Question cannot be empty.");
                return;
            }

            try
            {
                _logger.LogInformation("AI chat stream started for connection {ConnectionId}", Context.ConnectionId);

                await foreach (var token in _aiService.ChatStreamAsync(question, Context.ConnectionAborted))
                {
                    await Clients.Caller.SendAsync("ReceiveToken", token, Context.ConnectionAborted);
                }

                await Clients.Caller.SendAsync("ChatComplete", Context.ConnectionAborted);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AI chat stream cancelled for connection {ConnectionId}", Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI chat stream error for connection {ConnectionId}", Context.ConnectionId);
                await Clients.Caller.SendAsync("ChatError", $"AI error: {ex.Message}");
            }
        }
    }
}
