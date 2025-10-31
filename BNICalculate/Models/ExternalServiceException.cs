namespace BNICalculate.Models;

/// <summary>
/// 外部服務呼叫失敗例外（台銀 API）
/// </summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException()
    {
    }

    public ExternalServiceException(string message) : base(message)
    {
    }

    public ExternalServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
