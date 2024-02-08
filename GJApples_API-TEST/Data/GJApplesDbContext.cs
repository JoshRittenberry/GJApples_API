using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GJApples_API_TEST.Models;
using Microsoft.AspNetCore.Identity;

namespace GJApples.Data;

public class GJApplesDbContext : IdentityDbContext<IdentityUser>
{
    private readonly IConfiguration _configuration;

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AppleVariety> AppleVarieties { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Tree> Trees { get; set; }
    public DbSet<TreeHarvestReport> TreeHarvestReports { get; set; }

    public GJApplesDbContext(DbContextOptions<GJApplesDbContext> context, IConfiguration config) : base(context)
    {
        _configuration = config;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole[]
        {
            new IdentityRole
            {
                Id = "c3aaeb97-d2ba-4a53-a521-4eea61e59b35",
                Name = "Admin",
                NormalizedName = "admin"
            },
            new IdentityRole
            {
                Id = "8b2b3a2d-62f6-4f2b-8b3d-45b6a1f3b5b4",
                Name = "Harvester",
                NormalizedName = "harvester"
            },
            new IdentityRole
            {
                Id = "f65f1f30-d0b1-4f59-a3c8-eb1f2e6757d3",
                Name = "OrderPicker",
                NormalizedName = "orderpicker"
            },
            new IdentityRole
            {
                Id = "d4f146bf-70c8-4d02-98ec-0b5f4b9d213f",
                Name = "Customer",
                NormalizedName = "customer"
            }
        });

        var passwordHasher = new PasswordHasher<IdentityUser>();

        modelBuilder.Entity<IdentityUser>().HasData(new IdentityUser[]
        {
            new IdentityUser
            {
                Id = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f",
                UserName = "Administrator",
                Email = "admin@gjapples.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["AdminPassword"])
            },
            new IdentityUser
            {
                Id = "8c3605d2-c0da-4592-8879-0c71dc3c02c4",
                UserName = "Josh",
                Email = "josh@gjapples.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["HarvesterPassword"])
            },
            new IdentityUser
            {
                Id = "3a64b2c1-7780-40f1-a393-8edb30c4b2ab",
                UserName = "Haley",
                Email = "haley@gjapples.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["HarvesterPassword"])
            },
            new IdentityUser
            {
                Id = "83aab5f4-67ba-4da9-940e-fef0ce8597bd",
                UserName = "Chris",
                Email = "chris@gjapples.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["OrderPickerPassword"])
            },
            new IdentityUser
            {
                Id = "03d8deac-3687-4274-82c1-e1d32392d2de",
                UserName = "Kyle",
                Email = "kyle@gjapples.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["OrderPickerPassword"])
            },
            new IdentityUser
            {
                Id = "c8c02266-41e6-414d-a1fc-14bbefef86a0",
                UserName = "Debbie",
                Email = "debbie@gmail.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["CustomerPassword"])
            },
            new IdentityUser
            {
                Id = "bc3a3871-4800-4061-8182-b965c9c109bc",
                UserName = "Aaron",
                Email = "aaron@yahoo.com",
                PasswordHash = passwordHasher.HashPassword(null, _configuration["CustomerPassword"])
            }
        });

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "c3aaeb97-d2ba-4a53-a521-4eea61e59b35",
                UserId = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f"
            },
            new IdentityUserRole<string>
            {
                RoleId = "8b2b3a2d-62f6-4f2b-8b3d-45b6a1f3b5b4",
                UserId = "8c3605d2-c0da-4592-8879-0c71dc3c02c4"
            },
            new IdentityUserRole<string>
            {
                RoleId = "8b2b3a2d-62f6-4f2b-8b3d-45b6a1f3b5b4",
                UserId = "3a64b2c1-7780-40f1-a393-8edb30c4b2ab"
            },
            new IdentityUserRole<string>
            {
                RoleId = "f65f1f30-d0b1-4f59-a3c8-eb1f2e6757d3",
                UserId = "83aab5f4-67ba-4da9-940e-fef0ce8597bd"
            },
            new IdentityUserRole<string>
            {
                RoleId = "f65f1f30-d0b1-4f59-a3c8-eb1f2e6757d3",
                UserId = "03d8deac-3687-4274-82c1-e1d32392d2de"
            },
            new IdentityUserRole<string>
            {
                RoleId = "d4f146bf-70c8-4d02-98ec-0b5f4b9d213f",
                UserId = "c8c02266-41e6-414d-a1fc-14bbefef86a0"
            },
            new IdentityUserRole<string>
            {
                RoleId = "d4f146bf-70c8-4d02-98ec-0b5f4b9d213f",
                UserId = "bc3a3871-4800-4061-8182-b965c9c109bc"
            }
        );

        // Defines the relationship between UserProfile and the Order's Customer Foreign Key
        modelBuilder.Entity<UserProfile>()
            .HasMany(u => u.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerUserProfileId)
            .IsRequired(false);

        // Defines the relationship between UserProfile and the Order's Employee Foreign Key
        modelBuilder.Entity<UserProfile>()
            .HasMany(u => u.CompletedOrders)
            .WithOne(co => co.Employee)
            .HasForeignKey(co => co.EmployeeUserProfileId)
            .IsRequired(false);

        // Defines the relationship between UserProfile and the TreeHarvestReport's Employee Foreign Key
        modelBuilder.Entity<UserProfile>()
            .HasMany(u => u.TreeHarvestReports)
            .WithOne(thr => thr.Employee)
            .HasForeignKey(thr => thr.EmployeeUserProfileId)
            .IsRequired(false);

        modelBuilder.Entity<UserProfile>().HasData(
            new UserProfile
            {
                Id = 1,
                IdentityUserId = "dbc40bc6-0829-4ac5-a3ed-180f5e916a5f",
                FirstName = "Garry",
                LastName = "Jones",
                Address = "101 Main Street",
                ForcePasswordChange = false
            },
            new UserProfile
            {
                Id = 2,
                IdentityUserId = "8c3605d2-c0da-4592-8879-0c71dc3c02c4",
                FirstName = "Josh",
                LastName = "Harvester",
                Address = "102 Harvest Lane",
                ForcePasswordChange = false
            },
            new UserProfile
            {
                Id = 3,
                IdentityUserId = "3a64b2c1-7780-40f1-a393-8edb30c4b2ab",
                FirstName = "Haley",
                LastName = "Harvester",
                Address = "103 Harvest Lane",
                ForcePasswordChange = false
            },
            new UserProfile
            {
                Id = 4,
                IdentityUserId = "83aab5f4-67ba-4da9-940e-fef0ce8597bd",
                FirstName = "Chris",
                LastName = "Picker",
                Address = "104 Picker Street",
                ForcePasswordChange = false
            },
            new UserProfile
            {
                Id = 5,
                IdentityUserId = "03d8deac-3687-4274-82c1-e1d32392d2de",
                FirstName = "Kyle",
                LastName = "Picker",
                Address = "105 Picker Street",
                ForcePasswordChange = false
            },
            new UserProfile
            {
                Id = 6,
                IdentityUserId = "c8c02266-41e6-414d-a1fc-14bbefef86a0",
                FirstName = "Debbie",
                LastName = "Customer",
                Address = "106 Customer Road",
                ForcePasswordChange = false
            },
            new UserProfile
            {
                Id = 7,
                IdentityUserId = "bc3a3871-4800-4061-8182-b965c9c109bc",
                FirstName = "Aaron",
                LastName = "Customer",
                Address = "107 Customer Road",
                ForcePasswordChange = false
            }
        );

        modelBuilder.Entity<AppleVariety>().HasData(
            new AppleVariety
            {
                Id = 1,
                Type = "Honeycrisp",
                ImageUrl = "https://i.ibb.co/ZMDrZFH/Honey-Crisp-Apple.jpg",
                CostPerPound = 1.20m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 2,
                Type = "Granny Smith",
                ImageUrl = "https://i.ibb.co/8bq7xWW/Granny-Smith-Apple.jpg",
                CostPerPound = 0.95m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 3,
                Type = "Fuji",
                ImageUrl = "https://i.ibb.co/WBtxpKf/Fuji-Apple.webp",
                CostPerPound = 1.10m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 4,
                Type = "Gala",
                ImageUrl = "https://i.ibb.co/SNp02bf/Gala-Apple.jpg",
                CostPerPound = 0.85m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 5,
                Type = "Pink Lady",
                ImageUrl = "https://i.ibb.co/8XjJ88T/Pink-Lady-Apple.jpg",
                CostPerPound = 1.15m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 6,
                Type = "Braeburn",
                ImageUrl = "https://i.ibb.co/5rW7k1s/Braeburn-Apple.jpg",
                CostPerPound = 0.90m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 7,
                Type = "Red Delicious",
                ImageUrl = "https://i.ibb.co/C9GyMDj/Red-Delicious-Apple.jpg",
                CostPerPound = 0.80m,
                IsActive = true,
            },
            new AppleVariety
            {
                Id = 8,
                Type = "Golden Delicious",
                ImageUrl = "https://i.ibb.co/Bz1tZ16/Golden-Delicious-Apple.jpg",
                CostPerPound = 0.85m,
                IsActive = true,
            }
        );

        modelBuilder.Entity<Tree>().HasData(
            new Tree
            {
                Id = 1,
                AppleVarietyId = 1,
                DatePlanted = new DateTime(2015, 5, 10)
            },
            new Tree
            {
                Id = 2,
                AppleVarietyId = 1,
                DatePlanted = new DateTime(2016, 6, 15)
            },
            new Tree
            {
                Id = 3,
                AppleVarietyId = 2,
                DatePlanted = new DateTime(2017, 7, 20)
            },
            new Tree
            {
                Id = 4,
                AppleVarietyId = 2,
                DatePlanted = new DateTime(2018, 8, 25),
                DateRemoved = new DateTime(2023, 1, 30)
            },
            new Tree
            {
                Id = 5,
                AppleVarietyId = 3,
                DatePlanted = new DateTime(2019, 9, 30)
            },
            new Tree
            {
                Id = 6,
                AppleVarietyId = 3,
                DatePlanted = new DateTime(2020, 10, 5)
            },
            new Tree
            {
                Id = 7,
                AppleVarietyId = 4,
                DatePlanted = new DateTime(2021, 11, 10)
            },
            new Tree
            {
                Id = 8,
                AppleVarietyId = 4,
                DatePlanted = new DateTime(2022, 12, 15)
            },
            new Tree
            {
                Id = 9,
                AppleVarietyId = 5,
                DatePlanted = new DateTime(2016, 3, 20)
            },
            new Tree
            {
                Id = 10,
                AppleVarietyId = 5,
                DatePlanted = new DateTime(2017, 4, 25)
            },
            new Tree
            {
                Id = 11,
                AppleVarietyId = 6,
                DatePlanted = new DateTime(2018, 5, 30)
            },
            new Tree
            {
                Id = 12,
                AppleVarietyId = 6,
                DatePlanted = new DateTime(2019, 6, 5)
            },
            new Tree
            {
                Id = 13,
                AppleVarietyId = 7,
                DatePlanted = new DateTime(2020, 7, 10)
            },
            new Tree
            {
                Id = 14,
                AppleVarietyId = 7,
                DatePlanted = new DateTime(2021, 8, 15)
            },
            new Tree
            {
                Id = 15,
                AppleVarietyId = 8,
                DatePlanted = new DateTime(2022, 9, 20)
            },
            new Tree
            {
                Id = 16,
                AppleVarietyId = 8,
                DatePlanted = new DateTime(2023, 10, 25)
            },
            new Tree
            {
                Id = 17,
                AppleVarietyId = 1,
                DatePlanted = new DateTime(2017, 11, 30)
            },
            new Tree
            {
                Id = 18,
                AppleVarietyId = 2,
                DatePlanted = new DateTime(2018, 12, 5),
                DateRemoved = new DateTime(2023, 2, 10)
            },
            new Tree
            {
                Id = 19,
                AppleVarietyId = 3,
                DatePlanted = new DateTime(2019, 1, 10)
            },
            new Tree
            {
                Id = 20,
                AppleVarietyId = 4,
                DatePlanted = new DateTime(2020, 2, 15)
            }
        );

        modelBuilder.Entity<TreeHarvestReport>().HasData(
            new TreeHarvestReport
            {
                Id = 1,
                TreeId = 1,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 45.5M
            },
            new TreeHarvestReport
            {
                Id = 2,
                TreeId = 1,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 23M
            },
            new TreeHarvestReport
            {
                Id = 3,
                TreeId = 2,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 32.25M
            },
            new TreeHarvestReport
            {
                Id = 4,
                TreeId = 2,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 50M
            },
            new TreeHarvestReport
            {
                Id = 5,
                TreeId = 3,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 58.75M
            },
            new TreeHarvestReport
            {

                Id = 6,
                TreeId = 3,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 54M
            },
            new TreeHarvestReport
            {
                Id = 7,
                TreeId = 4,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 57M
            },
            new TreeHarvestReport
            {
                Id = 8,
                TreeId = 5,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 28M
            },
            new TreeHarvestReport
            {
                Id = 9,
                TreeId = 5,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 38M
            },
            new TreeHarvestReport
            {
                Id = 10,
                TreeId = 6,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 28M
            },
            new TreeHarvestReport
            {
                Id = 11,
                TreeId = 6,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 45M
            },
            new TreeHarvestReport
            {
                Id = 12,
                TreeId = 7,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 30M
            },
            new TreeHarvestReport
            {
                Id = 13,
                TreeId = 7,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 49M
            },
            new TreeHarvestReport
            {
                Id = 14,
                TreeId = 8,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 52M
            },
            new TreeHarvestReport
            {
                Id = 15,
                TreeId = 8,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 38M
            },
            new TreeHarvestReport
            {
                Id = 16,
                TreeId = 9,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 56M
            },
            new TreeHarvestReport
            {
                Id = 17,
                TreeId = 9,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 27M
            },
            new TreeHarvestReport
            {
                Id = 18,
                TreeId = 10,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 50M
            },
            new TreeHarvestReport
            {
                Id = 19,
                TreeId = 10,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 16M
            },
            new TreeHarvestReport
            {
                Id = 20,
                TreeId = 11,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 15M
            },
            new TreeHarvestReport
            {
                Id = 21,
                TreeId = 11,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 35M
            },
            new TreeHarvestReport
            {
                Id = 22,
                TreeId = 12,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 26M
            },
            new TreeHarvestReport
            {
                Id = 23,
                TreeId = 12,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 56M
            },
            new TreeHarvestReport
            {
                Id = 24,
                TreeId = 13,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 31M
            },
            new TreeHarvestReport
            {
                Id = 25,
                TreeId = 13,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 50M
            },
            new TreeHarvestReport
            {
                Id = 26,
                TreeId = 14,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 40M
            },
            new TreeHarvestReport
            {
                Id = 27,
                TreeId = 14,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 38M
            },
            new TreeHarvestReport
            {
                Id = 28,
                TreeId = 15,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 46M
            },
            new TreeHarvestReport
            {
                Id = 29,
                TreeId = 15,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 24M
            },
            new TreeHarvestReport
            {
                Id = 30,
                TreeId = 16,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 41M
            },
            new TreeHarvestReport
            {
                Id = 31,
                TreeId = 16,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 46M
            },
            new TreeHarvestReport
            {
                Id = 32,
                TreeId = 17,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 42M
            },
            new TreeHarvestReport
            {
                Id = 33,
                TreeId = 17,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 52M
            },
            new TreeHarvestReport
            {
                Id = 34,
                TreeId = 18,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 55M
            },
            new TreeHarvestReport
            {
                Id = 35,
                TreeId = 19,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 51M
            },
            new TreeHarvestReport
            {
                Id = 36,
                TreeId = 19,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 19M
            },
            new TreeHarvestReport
            {
                Id = 37,
                TreeId = 20,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 7),
                PoundsHarvested = 26M
            },
            new TreeHarvestReport
            {
                Id = 38,
                TreeId = 20,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 14),
                PoundsHarvested = 50M
            },
            new TreeHarvestReport
            {
                Id = 39,
                TreeId = 1,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 16M
            },
            new TreeHarvestReport
            {
                Id = 40,
                TreeId = 1,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 28),
                PoundsHarvested = 43M
            },
            new TreeHarvestReport
            {
                Id = 41,
                TreeId = 2,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 26M
            },
            new TreeHarvestReport
            {
                Id = 42,
                TreeId = 2,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 28),
                PoundsHarvested = 18M
            },
            new TreeHarvestReport
            {
                Id = 43,
                TreeId = 3,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 50M
            },
            new TreeHarvestReport
            {
                Id = 44,
                TreeId = 3,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 28),
                PoundsHarvested = 56M
            },
            new TreeHarvestReport
            {
                Id = 45,
                TreeId = 4,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 58M
            },
            new TreeHarvestReport
            {
                Id = 46,
                TreeId = 5,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 31M
            },
            new TreeHarvestReport
            {
                Id = 47,
                TreeId = 6,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 28M
            },
            new TreeHarvestReport
            {
                Id = 48,
                TreeId = 7,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 51M
            },
            new TreeHarvestReport
            {
                Id = 49,
                TreeId = 8,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 41M
            },
            new TreeHarvestReport
            {
                Id = 50,
                TreeId = 9,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 27M
            },
            new TreeHarvestReport
            {
                Id = 51,
                TreeId = 10,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 25M
            },
            new TreeHarvestReport
            {
                Id = 52,
                TreeId = 11,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 16M
            },
            new TreeHarvestReport
            {
                Id = 53,
                TreeId = 12,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 41M
            },
            new TreeHarvestReport
            {
                Id = 54,
                TreeId = 13,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 19M
            },
            new TreeHarvestReport
            {
                Id = 55,
                TreeId = 14,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 42M
            },
            new TreeHarvestReport
            {
                Id = 56,
                TreeId = 15,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 37M
            },
            new TreeHarvestReport
            {
                Id = 57,
                TreeId = 16,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 20M
            },
            new TreeHarvestReport
            {
                Id = 58,
                TreeId = 17,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 26M
            },
            new TreeHarvestReport
            {
                Id = 59,
                TreeId = 18,
                EmployeeUserProfileId = 2,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 23M
            },
            new TreeHarvestReport
            {
                Id = 60,
                TreeId = 19,
                EmployeeUserProfileId = 3,
                HarvestDate = new DateTime(2023, 1, 21),
                PoundsHarvested = 17M
            }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                Id = 1,
                CustomerUserProfileId = 6,
                DateOrdered = new DateTime(2023, 8, 11),
                Canceled = true
            },
            new Order
            {
                Id = 2,
                CustomerUserProfileId = 6,
                DateOrdered = new DateTime(2023, 9, 14),
                Canceled = true
            },
            new Order
            {
                Id = 3,
                CustomerUserProfileId = 7,
                DateOrdered = new DateTime(2023, 12, 24),
                Canceled = true
            },
            new Order
            {
                Id = 4,
                CustomerUserProfileId = 6,
                EmployeeUserProfileId = 4,
                DateOrdered = new DateTime(2023, 10, 3),
                DateCompleted = new DateTime(2023, 10, 4),
                Canceled = false
            },
            new Order
            {
                Id = 5,
                CustomerUserProfileId = 7,
                EmployeeUserProfileId = 5,
                DateOrdered = new DateTime(2023, 5, 30),
                DateCompleted = new DateTime(2023, 6, 1),
                Canceled = false
            },
            new Order
            {
                Id = 6,
                CustomerUserProfileId = 7,
                EmployeeUserProfileId = 5,
                DateOrdered = new DateTime(2023, 3, 23),
                DateCompleted = new DateTime(2023, 4, 15),
                Canceled = false
            },
            new Order
            {
                Id = 7,
                CustomerUserProfileId = 7,
                EmployeeUserProfileId = 4,
                DateOrdered = new DateTime(2023, 7, 22),
                DateCompleted = new DateTime(2023, 11, 23),
                Canceled = false
            },
            new Order
            {
                Id = 8,
                CustomerUserProfileId = 7,
                EmployeeUserProfileId = 5,
                DateOrdered = new DateTime(2023, 10, 10),
                DateCompleted = new DateTime(2023, 10, 28),
                Canceled = false
            },
            new Order
            {
                Id = 9,
                CustomerUserProfileId = 6,
                DateOrdered = new DateTime(2023, 1, 5),
                DateCompleted = null,
                Canceled = false
            },
            new Order
            {
                Id = 10,
                CustomerUserProfileId = 7,
                DateOrdered = new DateTime(2023, 4, 8),
                DateCompleted = null,
                Canceled = false
            },
            new Order
            {
                Id = 11,
                CustomerUserProfileId = 6,
                DateOrdered = new DateTime(2023, 3, 26),
                DateCompleted = null,
                Canceled = false
            }
        );

        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem { Id = 1, OrderId = 1, AppleVarietyId = 1, Pounds = 1.5m },
            new OrderItem { Id = 2, OrderId = 1, AppleVarietyId = 3, Pounds = 1.0m },
            new OrderItem { Id = 3, OrderId = 1, AppleVarietyId = 4, Pounds = 1.0m },
            new OrderItem { Id = 4, OrderId = 1, AppleVarietyId = 6, Pounds = 2.5m },
            new OrderItem { Id = 5, OrderId = 2, AppleVarietyId = 2, Pounds = 2.0m },
            new OrderItem { Id = 6, OrderId = 2, AppleVarietyId = 8, Pounds = 3.0m },
            new OrderItem { Id = 7, OrderId = 2, AppleVarietyId = 4, Pounds = 1.0m },
            new OrderItem { Id = 8, OrderId = 2, AppleVarietyId = 6, Pounds = 1.0m },
            new OrderItem { Id = 9, OrderId = 3, AppleVarietyId = 7, Pounds = 4.0m },
            new OrderItem { Id = 10, OrderId = 3, AppleVarietyId = 4, Pounds = 1.5m },
            new OrderItem { Id = 11, OrderId = 3, AppleVarietyId = 2, Pounds = 1.0m },
            new OrderItem { Id = 12, OrderId = 3, AppleVarietyId = 1, Pounds = 1.5m },
            new OrderItem { Id = 13, OrderId = 4, AppleVarietyId = 3, Pounds = 4.5m },
            new OrderItem { Id = 14, OrderId = 4, AppleVarietyId = 8, Pounds = 3.0m },
            new OrderItem { Id = 15, OrderId = 4, AppleVarietyId = 5, Pounds = 1.0m },
            new OrderItem { Id = 16, OrderId = 4, AppleVarietyId = 1, Pounds = 4.5m },
            new OrderItem { Id = 17, OrderId = 4, AppleVarietyId = 2, Pounds = 4.5m },
            new OrderItem { Id = 18, OrderId = 4, AppleVarietyId = 7, Pounds = 3.0m },
            new OrderItem { Id = 19, OrderId = 5, AppleVarietyId = 6, Pounds = 3.5m },
            new OrderItem { Id = 20, OrderId = 5, AppleVarietyId = 7, Pounds = 1.5m },
            new OrderItem { Id = 21, OrderId = 5, AppleVarietyId = 1, Pounds = 4.0m },
            new OrderItem { Id = 22, OrderId = 5, AppleVarietyId = 4, Pounds = 2.5m },
            new OrderItem { Id = 23, OrderId = 6, AppleVarietyId = 1, Pounds = 1.0m },
            new OrderItem { Id = 24, OrderId = 6, AppleVarietyId = 8, Pounds = 4.0m },
            new OrderItem { Id = 25, OrderId = 6, AppleVarietyId = 2, Pounds = 4.5m },
            new OrderItem { Id = 26, OrderId = 7, AppleVarietyId = 3, Pounds = 1.0m },
            new OrderItem { Id = 27, OrderId = 7, AppleVarietyId = 4, Pounds = 1.0m },
            new OrderItem { Id = 28, OrderId = 8, AppleVarietyId = 8, Pounds = 1.0m },
            new OrderItem { Id = 29, OrderId = 8, AppleVarietyId = 3, Pounds = 3.0m },
            new OrderItem { Id = 30, OrderId = 8, AppleVarietyId = 2, Pounds = 2.5m },
            new OrderItem { Id = 31, OrderId = 8, AppleVarietyId = 4, Pounds = 4.0m },
            new OrderItem { Id = 32, OrderId = 8, AppleVarietyId = 1, Pounds = 4.5m },
            new OrderItem { Id = 33, OrderId = 8, AppleVarietyId = 6, Pounds = 1.0m },
            new OrderItem { Id = 34, OrderId = 8, AppleVarietyId = 5, Pounds = 2.0m },
            new OrderItem { Id = 35, OrderId = 8, AppleVarietyId = 7, Pounds = 1.0m },
            new OrderItem { Id = 36, OrderId = 9, AppleVarietyId = 3, Pounds = 1.0m },
            new OrderItem { Id = 37, OrderId = 9, AppleVarietyId = 8, Pounds = 1.0m },
            new OrderItem { Id = 38, OrderId = 9, AppleVarietyId = 5, Pounds = 2.5m },
            new OrderItem { Id = 39, OrderId = 9, AppleVarietyId = 7, Pounds = 1.0m },
            new OrderItem { Id = 40, OrderId = 9, AppleVarietyId = 6, Pounds = 3.5m },
            new OrderItem { Id = 41, OrderId = 10, AppleVarietyId = 5, Pounds = 3.0m },
            new OrderItem { Id = 42, OrderId = 10, AppleVarietyId = 3, Pounds = 4.0m },
            new OrderItem { Id = 43, OrderId = 10, AppleVarietyId = 1, Pounds = 4.0m },
            new OrderItem { Id = 44, OrderId = 10, AppleVarietyId = 7, Pounds = 4.0m },
            new OrderItem { Id = 45, OrderId = 10, AppleVarietyId = 4, Pounds = 4.5m },
            new OrderItem { Id = 46, OrderId = 10, AppleVarietyId = 6, Pounds = 4.5m },
            new OrderItem { Id = 47, OrderId = 10, AppleVarietyId = 8, Pounds = 2.0m },
            new OrderItem { Id = 48, OrderId = 11, AppleVarietyId = 1, Pounds = 2.0m },
            new OrderItem { Id = 49, OrderId = 11, AppleVarietyId = 8, Pounds = 2.5m },
            new OrderItem { Id = 50, OrderId = 11, AppleVarietyId = 6, Pounds = 1.0m }
        );
    }
}