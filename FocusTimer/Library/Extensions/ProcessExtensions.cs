using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AppCenter.Crashes;

namespace FocusTimer.Library.Extensions;

public static class ProcessExtensions
{
    private static readonly Dictionary<int, string> s_cache = new();

    public static string? ExecutablePath(this Process process)
    {
        if (process.Id == 0)
        {
            process.GetLogger().Warn("프로세스의 실행 파일 경로를 가져오려 하는데, 주어진 프로세스의 ID가 0입니다. 따라서 실행 파일의 경로를 가져올 수 없기에 null을 반환합니다.");
            return null;
        }
        
        if (!s_cache.ContainsKey(process.Id))
        {
            TryGetProcessFilename(process);
        }

        return s_cache[process.Id];
    }

    private static void TryGetProcessFilename(Process process)
    {
        var logger = process.GetLogger();

        try
        {
            s_cache[process.Id] = APIWrapper.GetProcessFilename(process);
            logger.Info($"조사해보니, 이 프로세스({process.Id}, {process.ProcessName})의 실행 파일 경로는 [{s_cache[process.Id]}] 입니다.");
        }
        catch (Exception e)
        {
            Crashes.TrackError(e);
            logger.Error($"프로세스({process.Id}, {process.ProcessName})의 실행 파일 정보를 가져오지 못 하였습니다.");
            logger.Error(e);

            s_cache[process.Id] = "";
        }
    }
}