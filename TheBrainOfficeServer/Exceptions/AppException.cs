namespace TheBrainOfficeServer.Exceptions
{
    public abstract class AppException : Exception
    {
        public string UserMessage { get; }
        public string DebugContext { get; }

        protected AppException(string userMessage, string debugContext, Exception? innerException = null) : base(userMessage, innerException)
        {
            UserMessage = userMessage;
            DebugContext = debugContext;
        }
    }

    public class RepositoryException : AppException
    {
        public RepositoryException(string operationDescription, string debugDetails, Exception? innerException = null) : base
            ($"Ошибка при выполнении операции: {operationDescription}", $"Repository error: {debugDetails}", innerException){}
    }
}