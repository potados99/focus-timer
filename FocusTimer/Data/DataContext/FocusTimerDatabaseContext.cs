using System;
using System.IO;
using System.Linq;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace FocusTimer.Data.DataContext;

public class FocusTimerDatabaseContext : DbContext
{
    public DbSet<Domain.Entities.App> Apps { get; set; }
    public DbSet<AppUsage> AppUsages { get; set; }
    public DbSet<TimerUsage> TimerUsages { get; set; }
    public DbSet<Slot> SlotStatuses { get; set; }

    public DbSet<ResetHistory> ResetHistories { get; set; }
    
    private string DbPath
    {
        get
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var thisAppData = Path.Combine(appData, "World Moment", "Focus Timer");

            if (!Directory.Exists(thisAppData))
            {
                Directory.CreateDirectory(thisAppData);
            }

            return Path.Combine(thisAppData, "FocusTimer.db");
        }
    }

    public FocusTimerDatabaseContext()
    {
        Initialize();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
        .UseSqlite($"Data Source={DbPath}");

    private void Initialize()
    {
        if (File.Exists(DbPath))
        {
            try
            {
                TimerUsages.AsEnumerable().LastOrDefault();
            }
            catch (SqliteException e)
            {
                Crashes.TrackError(e);
                e.GetLogger().Warn("DB에 질의할 때에 SqliteException이 발생하여 DB를 새로 생성합니다.");
                e.GetLogger().Warn(e);
                Database.EnsureDeleted();
            }
        }

        Database.EnsureCreated();
    }

    public void Save()
    {
        SaveChanges();
    }
}