// FocusTimerDatabaseContext.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.IO;
using System.Linq;
using FocusTimer.Domain.Entities;
using FocusTimer.Library.Extensions;
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