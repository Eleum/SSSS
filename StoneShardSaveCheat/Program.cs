using StoneShardSaveCheat.Services;

namespace StoneShardSaveCheat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging with timestamps
            builder.Logging.AddSimpleConsole(o =>
            {
                o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
            });

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddHostedService<CharacterFolderMonitorService>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/hi", () => "Hi :)");

            app.Run();
        }
    }
}
