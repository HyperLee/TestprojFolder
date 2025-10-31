namespace BNICalculate.Models;

/// <summary>
/// 資料格式錯誤例外（CSV/JSON 格式錯誤）
/// </summary>
public class DataFormatException : Exception
{
    public DataFormatException()
    {
    }

    public DataFormatException(string message) : base(message)
    {
    }

    public DataFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
