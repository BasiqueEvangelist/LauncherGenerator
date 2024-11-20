namespace MCApi;

[Serializable]
public class MCDownloadException : MCApiException
{
    public MCDownloadException() { }
    
    public MCDownloadException(string message) : base(message) { }
    
    public MCDownloadException(string message, Exception inner) : base(message, inner) { }
}
