// UsageSplitter.cs
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
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;

namespace FocusTimer.Domain.Services;

public class UsageSplitter<T> where T : IElapsable, new()
{
    private readonly T _usage;

    public UsageSplitter(T usage)
    {
        _usage = usage;
    }

    public IEnumerable<T> Split()
    {
        var result = new List<T>();

        var currentDate = _usage.StartedAt.Date;

        while (currentDate <= _usage.UpdatedAt.Date)
        {
            var isFirstDay = currentDate == _usage.StartedAt.Date;
            var isLastDay = currentDate == _usage.UpdatedAt.Date;

            if (isFirstDay && isLastDay)
            {
                result.Add(BuildUsage(_usage.StartedAt, _usage.UpdatedAt));
            }
            else if (isFirstDay)
            {
                result.Add(BuildUsage(_usage.StartedAt, currentDate.AddHours(24)));
            }
            else if (isLastDay)
            {
                result.Add(BuildUsage(currentDate, _usage.UpdatedAt));
            }
            else
            {
                result.Add(BuildUsage(currentDate, currentDate.AddHours(24)));
            }

            currentDate = currentDate.AddHours(24);
        }

        return result;
    }

    private T BuildUsage(DateTime startedAt, DateTime updatedAt)
    {
        return new T
        {
            StartedAt = startedAt,
            UpdatedAt = updatedAt,
            ElapsedTicks = (updatedAt - startedAt).Ticks
        };
    }

    public static IEnumerable<TRunningUsage>
        GetRunningUsagesSplitByDate<TUsage, TRunningUsage, TActiveUsage>(TUsage usage)
        where TUsage : IUsage<TRunningUsage>
        where TRunningUsage : class, IRunningUsage<TUsage, TActiveUsage>, new()
        where TActiveUsage : class, IActiveUsage<TRunningUsage>, new()
    {
        var runningUsages = usage.RunningUsages
            .SelectMany(u => new UsageSplitter<TRunningUsage>(u).Split())
            .ToList(); // 이렇게 이 시점에 컬렉션을 확보해야 아래에서 수정할 수 있습니다.

        var activeUsages = usage.RunningUsages
            .SelectMany(u => u.ActiveUsages)
            .Where(u => (u.UpdatedAt - u.StartedAt).Ticks > 0 || u.ElapsedTicks > 0) // 실제로 사용된 것만 취급합니다.
            .SelectMany(u => new UsageSplitter<TActiveUsage>(u).Split())
            .ToList(); // 이렇게 이 시점에 컬렉션을 확보해야 아래에서 수정할 수 있습니다.

        foreach (var runningUsage in runningUsages)
        {
            runningUsage.ParentUsage = usage;

            // ActiveUsage 중 현재 RunningUsage의 시간 범위에 속한 것만 선택합니다.
            var activeUsagesInThisRunningUsage = activeUsages
                .Where(u => runningUsage.StartedAt <= u.StartedAt && u.UpdatedAt <= runningUsage.UpdatedAt);

            foreach (var activeUsage in activeUsagesInThisRunningUsage)
            {
                activeUsage.ParentRunningUsage = runningUsage;

                runningUsage.ActiveUsages.Add(activeUsage);
            }
        }

        return runningUsages;
    }
}