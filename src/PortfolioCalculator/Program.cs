namespace PortfolioCalculator
{
    using PortfolioCalculator.Middlewares;
    using Serilog;
    using Services.Implementatons;
    using Services.Interfaces;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient();
            builder.Services.AddSerilog();
            builder.Services.AddSession(optins =>
            {
                // TODO: The time can be made configurable.
                optins.IdleTimeout = TimeSpan.FromMinutes(30);
                optins.Cookie.HttpOnly = true;
                optins.Cookie.IsEssential = true;
            });

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Services.AddTransient<ICoinLoreApiService, CoinLoreApiService>();
            builder.Services.AddSingleton<IPortfolioService, PortfolioService>();
            builder.Services.AddSingleton<IFileService, FileService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseMiddleware<SessionMiddleware>();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
