// FocusTimerDatabaseContext.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
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

public sealed class FocusTimerDatabaseContext : DbContext
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

    /// <summary>
    /// 이 DB Context를 사용하려면 이 메소드를 시작 시점에 호출해주어야 합니다.
    /// </summary>
    public void Initialize()
    {
        // 여기서 Migrate()을 호출해야 했습니다...
        // EnsureCreated()는 마이그레이션 히스토리 테이블을 만들지 않습니다. 즉, 개발 및 테스트용입니다.
        // DB가 이미 생성된 상태에서 Migrate()을 호출하면 지금 존재하는 스케마를 또 생성하려 하기 때문에 뻗습니다!
        // 따라서 배포가 끝난 지금, 아래 호출을 Migrate()으로 변경할 수 없습니다... 마이그레이션 할 일이 없기를 비는 수밖에,,
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
        .UseSqlite($"Data Source={DbPath}");
}