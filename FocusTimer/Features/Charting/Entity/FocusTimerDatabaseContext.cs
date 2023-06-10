using FocusTimer.Features.Charting.Processing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace FocusTimer.Features.Charting.Entity
{
    public class FocusTimerDatabaseContext : DbContext
    {
        public DbSet<AppUsage> AppUsages { get; set; }
        public DbSet<TimerUsage> TimerUsages { get; set; }

        private readonly Queue<Action> PendingActions = new();

        private string DbPath
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

        public FocusTimerDatabaseContext(bool readOnly)
        {
            Initialize(readOnly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Data Source={DbPath}");

        public void Initialize(bool readOnly)
        {
            if (readOnly)
            {
                // 읽기 전용 컨텍스트이면 DB 생성과 백그라운드 워커 스레드 실행을 하지 않습니다.
                return;
            }

            if (!File.Exists(DbPath))
            {
                PendingActions.Enqueue(() =>
                {
                    Database.EnsureCreated();

                    //var c = DummyDataGenerator.GenerateEmpty();

                    //AppUsages.AddRange(c.AppUsages);
                    //TimerUsages.AddRange(c.TimerUsages);
                    //SaveChanges();
                });
            }

            new BackgroundWorker(this).StartWorking();
        }

        public void AddAppUsage(AppUsage usage)
        {
            PendingActions.Enqueue(() =>
            {
                AppUsages.Add(usage);
            });
        }

        public void AddTimerUsage(TimerUsage usage)
        {
            PendingActions.Enqueue(() =>
            {
                TimerUsages.Add(usage);
            });
        }

        public void Save()
        {
            PendingActions.Enqueue(() =>
            {
                SaveChanges();
            });
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
                    while (Context.PendingActions.Count > 0)
                    {
                        Context.PendingActions.Dequeue()();
                    }

                    Thread.Sleep(1000);
                }
            }
        }
    }
}
