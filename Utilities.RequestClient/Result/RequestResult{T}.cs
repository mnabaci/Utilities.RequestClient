namespace Utilities.RequestClient.Result
{
    public class RequestResult<T> : RequestResult where T : class
    {
        public T Result { get; set; }
    }
}