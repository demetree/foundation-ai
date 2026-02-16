namespace Foundation.AI.Zvec.Interop
{
    /// <summary>
    /// Exception thrown when a zvec native operation fails.
    /// </summary>
    public class ZvecException : Exception
    {
        public int ErrorCode { get; }

        public ZvecException(int errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ZvecException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

}