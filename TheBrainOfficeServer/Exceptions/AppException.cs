namespace TheBrainOfficeServer.Exceptions;

public abstract class AppException(string userMessage, string debugContext, Exception? innerException = null)
    : Exception(userMessage, innerException)
{
    public string UserMessage { get; } = userMessage;
    public string DebugContext { get; } = debugContext;
}

public class RepositoryException(string operationDescription, string debugDetails, Exception? innerException = null)
    : AppException($"Ошибка при выполнении операции: {operationDescription}", $"Repository error: {debugDetails}",
        innerException);