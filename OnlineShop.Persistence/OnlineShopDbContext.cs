using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Models;

namespace OnlineShop.Persistence;

public class OnlineShopDbContext(DbContextOptions<OnlineShopDbContext> options) : DbContext(options)
{
    public required DbSet<Order> Orders { get; set; }
    
    public required DbSet<Payment> Payments { get; set; }
}