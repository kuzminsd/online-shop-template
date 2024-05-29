using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Persistence;

public class DbMaintenanceService(OnlineShopDbContext dbContext)
{
    public void CleanDb()
    {
        dbContext.Database.ExecuteSql($"""
                                       TRUNCATE TABLE "Orders" CASCADE;
                                       """);
    }

    public void Migrate()
    {
        dbContext.Database.Migrate();
    }
}