﻿// ProcessExtensions.cs
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
using System.Diagnostics;
using Microsoft.AppCenter.Crashes;

namespace FocusTimer.Library.Extensions;

public static class ProcessExtensions
{
    private static readonly Dictionary<int, string?> s_cache = new();

    public static string? ExecutablePath(this Process process)
    {
        if (!s_cache.ContainsKey(process.Id))
        {
            s_cache[process.Id] = SafeGetProcessFilename(process);
        }

        return s_cache[process.Id];
    }

    /// <summary>
    /// 프로세스의 실행파일 이름을 안전하게 가져옵니다.
    /// 못 가져오면 null입니다.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    private static string? SafeGetProcessFilename(Process process)
    {
        var logger = process.GetLogger();

        if (process.Id == 0)
        {
            logger.Warn("프로세스의 실행 파일 경로를 가져오려 하는데, 주어진 프로세스의 ID가 0입니다. 따라서 실행 파일의 경로를 가져올 수 없기에 null을 반환합니다.");
            return null;
        }

        try
        {
            var filename = APIWrapper.GetProcessFilename(process);
            logger.Info($"조사해보니, 이 프로세스({process.Id}, {process.SafeGetProcessName() ?? "???"})의 실행 파일 경로는 [{filename}] 입니다.");
            
            return filename;
        }
        catch (Exception e)
        {
            Crashes.TrackError(e);
            logger.Error($"프로세스({process.Id}, {process.SafeGetProcessName() ?? "???"})의 실행 파일 정보를 가져오지 못 하였습니다.");
            logger.Error(e);

            return null;
        }
    }

    /// <summary>
    /// 프로세스 이름을 안전하게 가져옵니다.
    /// 못 가져오면 null입니다.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    public static string? SafeGetProcessName(this Process process)
    {
        var logger = process.GetLogger();

        try
        {
            return process.ProcessName;
        }
        catch (Exception e)
        {
            Crashes.TrackError(e);
            logger.Error($"프로세스({process.Id})의 이름을 가져오지 못 하였습니다.");
            logger.Error(e);
            
            return null;
        }
    }
}