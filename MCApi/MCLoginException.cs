namespace MCApi;

[Serializable]
public class MCLoginException : MCApiException
{
    public MCLoginException() { }
    public MCLoginException(string message) : base(message) { }
    public MCLoginException(string message, Exception inner) : base(message, inner) { }
    protected MCLoginException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
