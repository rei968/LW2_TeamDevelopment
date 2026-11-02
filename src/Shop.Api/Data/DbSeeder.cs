using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Api.Data.Entities;
using Shop.Shared.Enums;

namespace Shop.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database exists
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles
        string[] roles = [UserRole.Admin, UserRole.Customer];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Default Admin User
        var adminEmail = "admin@store.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Ярослав (Адміністратор)",
                EmailConfirmed = true,
                Address = "м. Київ, вул. Велика Васильківська, 5"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRole.Admin);
            }
        }

        // 3. Seed Test Customer User
        var customerEmail = "customer@store.com";
        var customerUser = await userManager.FindByEmailAsync(customerEmail);
        if (customerUser == null)
        {
            customerUser = new ApplicationUser
            {
                UserName = customerEmail,
                Email = customerEmail,
                FullName = "Олексій Покупець",
                EmailConfirmed = true,
                Address = "м. Львів, пр. Свободи, 12"
            };

            var result = await userManager.CreateAsync(customerUser, "Customer123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, UserRole.Customer);
            }
        }

        // 4. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Ноутбуки та комп'ютери", Description = "Ноутбуки, ультрабуки, системні блоки та робочі станції" },
                new() { Name = "Смартфони та планшети", Description = "Флагмани, бюджетні смартфони, аксесуари" },
                new() { Name = "Аудіо та навушники", Description = "Бездротові навушники, акустичні системи, гарнітури" },
                new() { Name = "Монітори та периферія", Description = "Ігрові монітори, клавіатури, миші, вебкамери" },
                new() { Name = "Розумні гаджети", Description = "Смарт-годинники, фітнес-браслети, розумний дім" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 5. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var catLaptops = await context.Categories.FirstAsync(c => c.Name.Contains("Ноутбуки"));
            var catPhones = await context.Categories.FirstAsync(c => c.Name.Contains("Смартфони"));
            var catAudio = await context.Categories.FirstAsync(c => c.Name.Contains("Аудіо"));
            var catMonitors = await context.Categories.FirstAsync(c => c.Name.Contains("Монітори"));
            var catGadgets = await context.Categories.FirstAsync(c => c.Name.Contains("Розумні"));

            var products = new List<Product>
            {
                new()
                {
                    Name = "Apple MacBook Pro 16\" M3 Pro (18GB/512GB)",
                    Description = "Професійний ноутбук для складних задач з дисплеєм Liquid Retina XDR та чіпом M3 Pro.",
                    Price = 98999m,
                    StockQuantity = 12,
                    Brand = "Apple",
                    ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500",
                    CategoryId = catLaptops.Id
                },
                new()
                {
                    Name = "Lenovo Legion Pro 5 16IRX8",
                    Description = "Потужний ігровий ноутбук з процесором Intel Core i7-13700HX та відеокартою RTX 4070.",
                    Price = 64999m,
                    StockQuantity = 3, // Low stock alert!
                    Brand = "Lenovo",
                    ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=500",
                    CategoryId = catLaptops.Id
                },
                new()
                {
                    Name = "ASUS Zenbook 14 OLED UX3405",
                    Description = "Ультратонкий преміальний ноутбук з вражаючим 3K OLED-дисплеєм 120 Гц.",
                    Price = 52499m,
                    StockQuantity = 8,
                    Brand = "ASUS",
                    ImageUrl = "https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=500",
                    CategoryId = catLaptops.Id
                },
                new()
                {
                    Name = "Apple iPhone 16 Pro Max 256GB Black Titanium",
                    Description = "Флагманський смартфон з титановим корпусом, кнопкою Camera Control та чіпом A18 Pro.",
                    Price = 59999m,
                    StockQuantity = 15,
                    Brand = "Apple",
                    ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500",
                    CategoryId = catPhones.Id
                },
                new()
                {
                    Name = "Samsung Galaxy S24 Ultra 12/512GB Titanium Gray",
                    Description = "Смартфон з підтримкою Galaxy AI, вбудованим пером S-Pen та 200 Мп камерою.",
                    Price = 54999m,
                    StockQuantity = 2, // Low stock alert!
                    Brand = "Samsung",
                    ImageUrl = "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=500",
                    CategoryId = catPhones.Id
                },
                new()
                {
                    Name = "Google Pixel 9 Pro 128GB Obsidian",
                    Description = "Найкраща камера для мобільної фотографії з передовим чіпом Google Tensor G4.",
                    Price = 41999m,
                    StockQuantity = 6,
                    Brand = "Google",
                    ImageUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500",
                    CategoryId = catPhones.Id
                },
                new()
                {
                    Name = "Sony WH-1000XM5 Black",
                    Description = "Преміальні бездротові навушники з найкращим у класі активним шумозаглушенням.",
                    Price = 14999m,
                    StockQuantity = 20,
                    Brand = "Sony",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500",
                    CategoryId = catAudio.Id
                },
                new()
                {
                    Name = "Apple AirPods Pro (2nd Gen) with USB-C",
                    Description = "Навушники-вкладиші з адаптивним аудіо, просторовим звуком та футляром MagSafe.",
                    Price = 9999m,
                    StockQuantity = 18,
                    Brand = "Apple",
                    ImageUrl = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=500",
                    CategoryId = catAudio.Id
                },
                new()
                {
                    Name = "Dell UltraSharp 27 4K U2723QE",
                    Description = "Професійний 4K IPS Black монітор з точним передаванням кольорів та USB-C концентратором.",
                    Price = 24999m,
                    StockQuantity = 4, // Low stock alert!
                    Brand = "Dell",
                    ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=500",
                    CategoryId = catMonitors.Id
                },
                new()
                {
                    Name = "Logitech MX Master 3S Wireless Mouse",
                    Description = "Ергономічна бездротова миша з тихою роботою та електромагнітним колесом прокрутки MagSpeed.",
                    Price = 4399m,
                    StockQuantity = 35,
                    Brand = "Logitech",
                    ImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=500",
                    CategoryId = catMonitors.Id
                },
                new()
                {
                    Name = "Samsung Galaxy Watch Ultra 47mm",
                    Description = "Захищений смарт-годинник для екстремальних умов з титановим безелем та точним GPS.",
                    Price = 26999m,
                    StockQuantity = 5,
                    Brand = "Samsung",
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500",
                    CategoryId = catGadgets.Id
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // 6. Seed Sample Orders for Admin Testing
        if (!await context.Orders.AnyAsync())
        {
            var pMacbook = await context.Products.FirstOrDefaultAsync(p => p.Brand == "Apple" && p.Price > 90000);
            var pSony = await context.Products.FirstOrDefaultAsync(p => p.Brand == "Sony");
            var pMouse = await context.Products.FirstOrDefaultAsync(p => p.Brand == "Logitech");

            if (pMacbook != null && pSony != null && pMouse != null)
            {
                var order1 = new Order
                {
                    OrderNumber = "ORD-20261102-001",
                    UserId = customerUser?.Id,
                    CustomerEmail = "customer@store.com",
                    CustomerName = "Олексій Покупець",
                    CustomerPhone = "+380501234567",
                    ShippingAddress = "м. Львів, пр. Свободи, 12, кв. 45",
                    Status = OrderStatus.New,
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    TotalAmount = pSony.Price + (pMouse.Price * 2),
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = pSony.Id, UnitPrice = pSony.Price, Quantity = 1 },
                        new() { ProductId = pMouse.Id, UnitPrice = pMouse.Price, Quantity = 2 }
                    }
                };

                var order2 = new Order
                {
                    OrderNumber = "ORD-20261102-002",
                    UserId = customerUser?.Id,
                    CustomerEmail = "customer@store.com",
                    CustomerName = "Олексій Покупець",
                    CustomerPhone = "+380501234567",
                    ShippingAddress = "м. Львів, пр. Свободи, 12, кв. 45",
                    Status = OrderStatus.Processing,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    TotalAmount = pMacbook.Price,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = pMacbook.Id, UnitPrice = pMacbook.Price, Quantity = 1 }
                    }
                };

                var order3 = new Order
                {
                    OrderNumber = "ORD-20261101-003",
                    UserId = null,
                    CustomerEmail = "guest.buyer@gmail.com",
                    CustomerName = "Іван Мельник",
                    CustomerPhone = "+380679876543",
                    ShippingAddress = "м. Одеса, вул. Дерибасівська, 10",
                    Status = OrderStatus.Completed,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    TotalAmount = pSony.Price,
                    OrderItems = new List<OrderItem>
                    {
                        new() { ProductId = pSony.Id, UnitPrice = pSony.Price, Quantity = 1 }
                    }
                };

                await context.Orders.AddRangeAsync(order1, order2, order3);
                await context.SaveChangesAsync();
            }
        }

        // 7. Seed Sample Reviews
        if (!await context.Reviews.AnyAsync())
        {
            var pSony = await context.Products.FirstOrDefaultAsync(p => p.Brand == "Sony");
            if (pSony != null)
            {
                var review1 = new Review
                {
                    ProductId = pSony.Id,
                    UserId = customerUser?.Id,
                    Rating = 5,
                    Comment = "Найкраще шумозаглушення, яке я коли-небудь тестував. Звук чистий, батарея тримає довго!",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                };

                await context.Reviews.AddAsync(review1);
                await context.SaveChangesAsync();
            }
        }
    }
}
