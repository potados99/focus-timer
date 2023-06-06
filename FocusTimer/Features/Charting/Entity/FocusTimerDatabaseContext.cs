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

        public FocusTimerDatabaseContext() : base(GetOptions()) { }

        private static DbContextOptions GetOptions()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var thisAppData = Path.Combine(appData, "FocusTimer");

            if (!Directory.Exists(thisAppData))
            {
                Directory.CreateDirectory(thisAppData);
            }

            var dbPath = Path.Combine(thisAppData, "FocusTimer.db");

            return new DbContextOptionsBuilder().UseSqlite($"Data Source={dbPath}").Options;
        }

        public void Initialize()
        {
            Database.EnsureCreated();

            AppUsages.Add(new AppUsage
            {
                AppPath = "hahahahah",
                RegisteredAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Usage = 9999,
                IsConcentrated = true
            });

            SaveChanges();
        }
    }
}
