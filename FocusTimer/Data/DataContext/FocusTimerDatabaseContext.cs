using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace FocusTimer.Data.DataContext;

public class FocusTimerDatabaseContext : DbContext
{
    private readonly bool _readOnly;

    public DbSet<Domain.Entities.App> Apps { get; set; }
    public DbSet<AppUsage> AppUsages { get; set; }
    public DbSet<TimerUsage> TimerUsages { get; set; }
    public DbSet<SlotStatus> SlotStatuses { get; set; }

    private readonly Queue<Action> _pendingActions = new();

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

    public FocusTimerDatabaseContext(bool readOnly)
    {
        this._readOnly = readOnly;

        Initialize();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
        .UseSqlite($"Data Source={DbPath}")
        .UseQueryTrackingBehavior(_readOnly
            ? QueryTrackingBehavior.NoTracking
            : // 읽기전용에서는 캐싱을 꺼야 DB에 생긴 변화를 제대로 가져옵니다.
            QueryTrackingBehavior.TrackAll
        );

    public void Initialize()
    {
        if (_readOnly)
        {
            // 읽기 전용 컨텍스트이면 DB 생성과 백그라운드 워커 스레드 실행을 하지 않습니다.
            return;
        }

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

        new BackgroundWorker(this).StartWorking();
    }


    public void AddAppUsage(AppUsage usage)
    {
        _pendingActions.Enqueue(() => { AppUsages.Add(usage); });
    }

    public void AddTimerUsage(TimerUsage usage)
    {
        _pendingActions.Enqueue(() => { TimerUsages.Add(usage); });
    }

    public void AddSlotStatus(SlotStatus status)
    {
        _pendingActions.Enqueue(() => { SlotStatuses.Add(status); });
    }

    public void Save()
    {
        _pendingActions.Enqueue(() => { SaveChanges(); });
    }

    class BackgroundWorker
    {
        private readonly FocusTimerDatabaseContext Context;

        public BackgroundWorker(FocusTimerDatabaseContext context)
        {
            Context = context;
        }

        public void StartWorking()
        {
            ThreadStart start = new(Work);
            Thread thread = new(start)
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void Work()
        {
            while (true)
            {
                while (Context._pendingActions.Count > 0)
                {
                    Context._pendingActions.Dequeue()();
                }

                Thread.Sleep(1000);
            }
        }
    }
}