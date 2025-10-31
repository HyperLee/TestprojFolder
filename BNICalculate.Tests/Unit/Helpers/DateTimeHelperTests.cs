using BNICalculate.Helpers;

namespace BNICalculate.Tests.Unit.Helpers;

/// <summary>
/// DateTimeHelper 測試
/// </summary>
public class DateTimeHelperTests
{
    [Fact]
    public void GetTaiwanTime_Should_ReturnUtcPlus8()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;

        // Act
        var taiwanTime = DateTimeHelper.GetTaiwanTime();

        // Assert
        // 台灣時間應該是 UTC+8
        var expectedTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, 
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei"));
        
        // 允許1秒的誤差
        Assert.True(Math.Abs((taiwanTime - expectedTime).TotalSeconds) < 1);
    }

    [Fact]
    public void GetChineseDateString_Should_ReturnCorrectFormat()
    {
        // Arrange
        var testDate = new DateTime(2025, 11, 1, 14, 30, 0); // 2025年11月1日 14:30:00 (星期六)

        // Act
        var result = DateTimeHelper.GetChineseDateString(testDate);

        // Assert
        Assert.Contains("2025年11月1日", result);
        Assert.Contains("(", result);
        Assert.Contains(")", result);
        // 應該包含星期幾
        Assert.Matches(@"\([一二三四五六日]\)", result);
    }

    [Theory]
    [InlineData(2025, 11, 1)] // 星期六
    [InlineData(2025, 11, 2)] // 星期日
    [InlineData(2025, 11, 3)] // 星期一
    public void GetChineseDateString_Should_IncludeDayOfWeek(int year, int month, int day)
    {
        // Arrange
        var testDate = new DateTime(year, month, day);

        // Act
        var result = DateTimeHelper.GetChineseDateString(testDate);

        // Assert
        Assert.Contains($"{year}年{month}月{day}日", result);
        Assert.Matches(@"\([一二三四五六日]\)", result);
    }

    [Fact]
    public void GetTimeString_Should_Return24HourFormat()
    {
        // Arrange
        var testTime1 = new DateTime(2025, 11, 1, 14, 30, 0); // 下午2:30
        var testTime2 = new DateTime(2025, 11, 1, 9, 15, 30);  // 上午9:15:30

        // Act
        var result1 = DateTimeHelper.GetTimeString(testTime1);
        var result2 = DateTimeHelper.GetTimeString(testTime2);

        // Assert
        Assert.Equal("14:30:00", result1);
        Assert.Equal("09:15:30", result2);
        // 不應該包含 AM/PM
        Assert.DoesNotContain("AM", result1);
        Assert.DoesNotContain("PM", result1);
        Assert.DoesNotContain("上午", result1);
        Assert.DoesNotContain("下午", result1);
    }

    [Theory]
    [InlineData(0, 0, 0, "00:00:00")]
    [InlineData(23, 59, 59, "23:59:59")]
    [InlineData(12, 0, 0, "12:00:00")]
    public void GetTimeString_Should_HandleVariousTimes(int hour, int minute, int second, string expected)
    {
        // Arrange
        var testTime = new DateTime(2025, 11, 1, hour, minute, second);

        // Act
        var result = DateTimeHelper.GetTimeString(testTime);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFullDateTimeString_Should_CombineDateAndTime()
    {
        // Arrange
        var testDateTime = new DateTime(2025, 11, 1, 14, 30, 0);

        // Act
        var result = DateTimeHelper.GetFullDateTimeString(testDateTime);

        // Assert
        Assert.Contains("2025年11月1日", result);
        Assert.Contains("14:30:00", result);
        Assert.Matches(@"\([一二三四五六日]\)", result);
    }
}
