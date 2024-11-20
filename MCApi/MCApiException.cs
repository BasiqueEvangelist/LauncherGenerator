namespace MCApi;

[Serializable]
public class MCApiException : Exception
{
    public MCApiException() { }
    
    public MCApiException(string message) : base(message) { }
    
    public MCApiException(string message, Exception inner) : base(message, inner) { }
}