namespace Mayfly.Terminal.Commands;

internal sealed class ChecksumMismatchException(string message) : Exception(message);
