// AppUsageService.cs
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
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Domain.Services;

public class AppUsageService
{
    private readonly FocusRepository _repository;
    
    public AppUsageService(FocusRepository repository)
    {
        _repository = repository;
    }

    public AppUsage? GetLastUsage(Entities.App app)
    {
        var usage = _repository.FindLastAppUsageByApp(app);
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 AppUsage를 가져왔습니다: {usage}");
        }
        
        return usage;
    }

    public AppUsage CreateNewUsage(Entities.App app)
    {
        this.GetLogger().Debug("새로운 AppUsage를 생성합니다.");

        var usage = new AppUsage
        {
            App = app,
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsConcentrated = true
        };

        _repository.StartTracking(usage);
        
        return usage;
    }
    
    public void AddResetHistory()
    {
        var history = ResetHistory.OfNow();
        
        _repository.StartTracking(history);
    }

    public void SaveRepository()
    {
        _repository.Save();
    }
}