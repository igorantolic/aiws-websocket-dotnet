using AiWs.Api.Extension;
using AiWs.Api.WebSockets;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();

//+WS
builder.Services.AddSingleton<MessagingService>();
builder.Services.AddTransient<WebSocketAdapter>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("X-Connection-Id"));
});
//-WS
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.AiUseSwagger();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
//app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.AiUseWebsockets();//+WS

app.Run();