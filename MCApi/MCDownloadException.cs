namespace MCApi
{

    [Serializable]
    public class MCDownloadException : MCApiException
    {
        public MCDownloadException() { }
        public MCDownloadException(string message) : base(message) { }
        public MCDownloadException(string message, Exception inner) : base(message, inner) { }
        protected MCDownloadException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
