using FocusTimer.Features.Charting.Processing;
using HarfBuzzSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Features.Charting.Entity
{
    public class FocusTimerDatabaseContext : DbContext
    {
        public DbSet<AppUsage> AppUsages { get; set; }
        public DbSet<TimerUsage> TimerUsages { get; set; }

        private static string DbPath
        {
            get
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var thisAppData = Path.Combine(appData, "The Potato Baking Company", "Focus Timer");

                if (!Directory.Exists(thisAppData))
                {
                    Directory.CreateDirectory(thisAppData);
                }

                return Path.Combine(thisAppData, "usages.db");
            }
        }

        public FocusTimerDatabaseContext() : base(GetOptions()) {
            Initialize();
        }

        private static DbContextOptions GetOptions()
        {
            return new DbContextOptionsBuilder().UseSqlite($"Data Source={DbPath}").Options;
        }

        public async Task Initialize()
        {
            if (!File.Exists(DbPath)) {             
                await Database.EnsureCreatedAsync();

                var c = DummyDataGenerator.GenerateEmpty();

                await AppUsages.AddRangeAsync(c.AppUsages);
                await TimerUsages.AddRangeAsync(c.TimerUsages);

                await SaveChangesAsync();
            }
        }
    }
}
