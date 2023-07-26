using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FocusTimer.Lib.Utility
{
    public static class ProcessExtensions
    {
        private static readonly Dictionary<int, string> Cache = new();

        public static string? ExecutablePath(this Process process)
        {
            if (process.Id == 0)
            {
                process.GetLogger().Warn("이 프로세스의 ID는 0입니다. 따라서 실행 파일의 경로를 가져올 수 없기에 null을 반환합니다.");
                return null;
            }

            var logger = process.GetLogger();

            if (!Cache.ContainsKey(process.Id))
            {
                try
                {
                    Cache[process.Id] = APIWrapper.GetProcessFilename(process);
                    logger.Info($"조사해보니, 이 프로세스({process.Id}, {process.ProcessName})의 실행 파일 경로는 [{Cache[process.Id]}] 입니다.");
                }
                catch (Exception e)
                {
                    Crashes.TrackError(e);
                    logger.Error($"프로세스({process.Id}, {process.ProcessName})의 실행 파일 정보를 가져오지 못 하였습니다.");
                    logger.Error(e);

                    Cache[process.Id] = "";
                }
            }

            return Cache[process.Id];
        }
    }
}
