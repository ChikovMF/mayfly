namespace Mayfly.Terminal.Commands;

internal sealed class AccessDeniedException(string message) : Exception(message);
