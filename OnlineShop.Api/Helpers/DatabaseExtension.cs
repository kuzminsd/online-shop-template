using OnlineShop.Persistence;

namespace OnlineShop.Api.Helpers;

public static class DatabaseExtension
{
    public static void PrepareDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var dbMaintenanceService = serviceScope.ServiceProvider.GetRequiredService<DbMaintenanceService>();
        dbMaintenanceService.Migrate();
        dbMaintenanceService.CleanDb();

    }
}