namespace BitNet.Interop;

/// <summary>
/// A chat message with role and content, used for chat template formatting
/// within the BitNet.Interop layer.
/// </summary>
/// <param name="Role">The role of the message author (e.g. "system", "user", "assistant").</param>
/// <param name="Content">The text content of the message.</param>
public record BitNetChatMessage(string Role, string Content);
