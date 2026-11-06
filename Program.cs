using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ۱. اضافه کردن کلاینت HTTP برای دانلود جیسون از سرور دیگر
builder.Services.AddHttpClient();

var app = builder.Build();

// ۲. اندپوینت داخلی که نقش پروکسی را دارد و جیسون را می‌گیرد
app.MapGet("/openapi.json", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();

    // در صورت نیاز به هدر User-Agent برای عبور از کلودفلر/فایروال
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

    var targetUrl = "https://malitor.anbaronline.ir/swagger/v1/swagger.json";

    try
    {
        var response = await client.GetAsync(targetUrl);
        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem($"خطا در دریافت سواگر: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return Results.Content(json, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"ارتباط با سرور برقرار نشد: {ex.Message}");
    }
});

// ۳. اتصال اسکالر به اندپوینت داخلی
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Malitor API Documentation")
        .WithTheme(ScalarTheme.Purple)
        .WithOpenApiRoutePattern("/openapi.json"); // مسیر نسبی پروکسی داخلی
});

// ریدایرکت صفحه اصلی
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.Run();