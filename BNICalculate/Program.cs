using Serilog;
using System.Text;

namespace BNICalculate;

public class Program
{
    public static void Main(string[] args)
    {
        // 設定 Serilog (T007)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/currency-.txt", 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        try
        {
            Log.Information("應用程式啟動中...");

            var builder = WebApplication.CreateBuilder(args);

            // 使用 Serilog 作為日誌提供者
            builder.Host.UseSerilog();

            // 註冊 Big5 編碼提供者 (T008)
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // 註冊 IMemoryCache (T010 - 已存在，保留)
            builder.Services.AddMemoryCache();

            // 註冊 HttpClient 用於台銀 API (T009)
            builder.Services.AddHttpClient("TaiwanBankApi", client =>
            {
                client.BaseAddress = new Uri("https://rate.bot.com.tw");
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            // 註冊番茄鐘服務
            builder.Services.AddScoped<Services.PomodoroDataService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "應用程式啟動失敗");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}