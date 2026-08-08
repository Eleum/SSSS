using StoneShardSaveCheat.Services;
using StoneShardSaveScumming.Config;

namespace StoneShardSaveScumming
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

            builder.Services.Configure<SettingsConfig>(builder.Configuration.GetSection(SettingsConfig.SectionKey));

            // Add services to the container.
            builder.Services.AddHostedService<CharacterMonitorService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            }

            app.MapGet("/hi", () => "Hi :)");

            app.Run();
        }
    }
}
