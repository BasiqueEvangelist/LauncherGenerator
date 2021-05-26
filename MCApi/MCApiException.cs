namespace MCApi
{
    [System.Serializable]
    public class MCApiException : System.Exception
    {
        public MCApiException() { }
        public MCApiException(string message) : base(message) { }
        public MCApiException(string message, System.Exception inner) : base(message, inner) { }
        protected MCApiException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}