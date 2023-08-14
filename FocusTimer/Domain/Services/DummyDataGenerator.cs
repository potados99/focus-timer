using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;

namespace FocusTimer.Domain.Services
{
    public static class DummyDataGenerator
    {
        public static Context GenerateEmpty()
        {
            var c = new Context();

            while (c.IsItNotDone)
            {
                c.GetOrCreateTimerUsage();

                c.TimeGoes();
            }

            return c;
        }

        public static Context GenerateDummy()
        {
            var c = new Context();

            while (c.IsItNotDone)
            {
                if (c.OutOfWorkingHour)
                {
                    // 타이머가 켜져 있지 않으면
                    // 아무것도 안 해요
                }
                else
                {
                    if (c.IsFocusing)
                    {
                        // 앱을 사용 중이라면
                        // 그 앱의 사용 시간을 늘립니다.
                        var app = c.GetOrCreateCurrentAppUsage();

                        app.Usage += new TimeSpan(0, 1, 0).Ticks;
                    }
                    else if (c.IsFocusingFinished)
                    {
                        // 앱 사용이 끝났으면
                        if (c.WithHalfChance)
                        {
                            // 절반 확률로 쉬거나
                            c.StartResting();
                        }
                        else
                        {
                            // 바로 다른거 하러 갑니다.
                            c.StartFocusing();
                        }
                    }
                    else if (c.IsResting)
                    {
                        // 놀 때에는 아무 것도 안 해요
                    }
                    else if (c.IsRestingFinished)
                    {
                        // 휴식이 끝났으면
                        if (c.WithHalfChance)
                        {
                            // 절반 확률로 쉬거나
                            c.StartResting();
                        }
                        else
                        {
                            // 바로 다른거 하러 갑니다.
                            c.StartFocusing();
                        }
                    }
                    else
                    {
                        // 집중하지도 놀지도 않고 있을 때에는
                        // 바로 집중 시작합니다.
                        c.StartFocusing();
                    }

                    c.GetOrCreateTimerUsage().Usage += new TimeSpan(0, 1, 0).Ticks;
                }

                // 암튼 시간은 흐릅니다.
                c.TimeGoes();
            }

            return c;
        }

        public class Context
        {
            private DateTime CurrentDateTime = DateTime.Now.Subtract(new TimeSpan(21, 0, 0, 0));
            private DateTime UntilNow = DateTime.Now;

            private string[] AppPaths = new string[]
                {
                "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe",
                "C:\\Program Files (x86)\\Hnc\\Office 2020\\HOffice110\\Bin\\Hwp.exe",
                "C:\\Program Files (x86)\\Kakao\\KakaoTalk\\KakaoTalk.exe",
                "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
                "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
                "C:\\Windows\\explorer.exe"
                };

            public DateTime? FocusOnUntil { get; set; }
            public DateTime? TakeARestUntil { get; set; }

            private TimeSpan WorkingHourBegin = new TimeSpan(7, 0, 0);
            private TimeSpan WorkingHourEnd = new TimeSpan(18, 0, 0);

            private Random r = new Random();
            public string? CurrentAppPath { get; set; } = null;

            public List<AppUsage> AppUsages = new();
            public List<TimerUsage> TimerUsages = new();

            public bool OutOfWorkingHour
            {
                get
                {
                    var timeElapsedToday = CurrentDateTime - CurrentDateTime.Date;

                    return timeElapsedToday > WorkingHourEnd || timeElapsedToday < WorkingHourBegin;
                }
            }

            public bool WithHalfChance
            {
                get
                {
                    return r.Next(0, 10) >= 5;
                }
            }

            public bool IsItNotDone
            {
                get
                {
                    return CurrentDateTime < UntilNow;
                }
            }

            public bool IsItZeroOClock
            {
                get
                {
                    return CurrentDateTime.Hour == 0 && CurrentDateTime.Minute == 0;
                }
            }

            public bool IsFocusing
            {
                get
                {
                    return FocusOnUntil != null && CurrentDateTime < FocusOnUntil;
                }
            }

            public bool IsFocusingFinished
            {
                get
                {
                    return FocusOnUntil != null && (FocusOnUntil - CurrentDateTime)?.TotalMinutes == 0;
                }
            }

            public bool IsResting
            {
                get
                {
                    return TakeARestUntil != null && CurrentDateTime < TakeARestUntil;
                }
            }

            public bool IsRestingFinished
            {
                get
                {
                    return TakeARestUntil != null && (TakeARestUntil - CurrentDateTime)?.TotalMinutes == 0;
                }
            }

            public void TimeGoes()
            {
                if (OutOfWorkingHour)
                {
                    if (IsItZeroOClock)
                    {
                        WorkingHourBegin = new TimeSpan(r.Next(6, 12), r.Next(0, 60), 0);
                        WorkingHourEnd = new TimeSpan(r.Next(14, 22), r.Next(0, 60), 0);
                    }

                    CurrentDateTime = CurrentDateTime.AddMinutes(1);
                }
                else
                {
                    if (IsItZeroOClock)
                    {
                        var yesterday = CurrentDateTime.Date.Subtract(new TimeSpan(1, 0, 0, 0));
                        var usagesOfYesterday = AppUsages.Where(u => u.RegisteredAt.Date == yesterday);

                        AppUsages.AddRange(
                            usagesOfYesterday.Select(u => new AppUsage
                            {
                                AppPath = u.AppPath,
                                Usage = 0,
                                RegisteredAt = new DateTime(CurrentDateTime.Ticks),
                                UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                                IsConcentrated = true
                            }).ToList());

                        TimerUsages.Add(new TimerUsage
                        {
                            Usage = 0,
                            StartedAt = new DateTime(CurrentDateTime.Ticks),
                            UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                        });

                        WorkingHourBegin = new TimeSpan(r.Next(6, 12), r.Next(0, 60), 0);
                        WorkingHourEnd = new TimeSpan(r.Next(14, 22), r.Next(0, 60), 0);
                    }

                    var appUsagesOfToday = AppUsages.Where(u => u.RegisteredAt.Date == CurrentDateTime.Date);

                    foreach (var usage in appUsagesOfToday)
                    {
                        usage.UpdatedAt = new DateTime(CurrentDateTime.Ticks);
                    }

                    var timerUsagesOfToday = TimerUsages.Where(u => u.StartedAt.Date == CurrentDateTime.Date);

                    foreach (var usage in timerUsagesOfToday)
                    {
                        usage.UpdatedAt = new DateTime(CurrentDateTime.Ticks);
                    }

                    CurrentDateTime = CurrentDateTime.AddMinutes(1);
                }
            }

            public AppUsage GetOrCreateCurrentAppUsage()
            {
                var u = AppUsages.LastOrDefault(u => u.AppPath == CurrentAppPath && u.RegisteredAt.Date == CurrentDateTime.Date);

                if (u == null)
                {
                    u = new AppUsage
                    {
                        AppPath = CurrentAppPath,
                        Usage = 0,
                        RegisteredAt = new DateTime(CurrentDateTime.Ticks),
                        UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                        IsConcentrated = true
                    };

                    AppUsages.Add(u);
                }

                return u;
            }

            public TimerUsage GetOrCreateTimerUsage()
            {
                var u = TimerUsages.LastOrDefault(u => u.StartedAt.Date == CurrentDateTime.Date);

                if (u == null)
                {
                    u = new TimerUsage
                    {
                        Usage = 0,
                        StartedAt = new DateTime(CurrentDateTime.Ticks),
                        UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                    };

                    TimerUsages.Add(u);
                }

                return u;
            }

            public void StartFocusing()
            {
                TakeARestUntil = null;
                FocusOnUntil = CurrentDateTime.AddMinutes(r.Next(1, 60));
                CurrentAppPath = AppPaths[r.Next(0, AppPaths.Length)];
            }

            public void StartResting()
            {
                FocusOnUntil = null;
                TakeARestUntil = CurrentDateTime.AddMinutes(r.Next(5, 24));
                CurrentAppPath = null;
            }
        }
    }
}
