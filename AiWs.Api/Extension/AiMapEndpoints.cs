//using Npgsql;

namespace AiWs.Api.Extension;

public static class AiMapEndpoints
{
    public static WebApplication AiUseSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }

}
