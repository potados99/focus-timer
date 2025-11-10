// App.cs
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 등록 및 추적의 대상이 되는 프로그램입니다.
/// </summary>
public class App
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 앱의 실행 파일 경로입니다.
    /// </summary>
    public string ExecutablePath { get; set; }

    /// <summary>
    /// 앱의 이름입니다.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 앱의 아이콘입니다.
    /// DB에 저장하는 용도로, 비즈니스 로직에서 사용하지는 않습니다.
    /// </summary>
    public byte[] IconBytes
    {
        get => Icon.ToBytes();
        set => Icon = value.ToIcon();
    }

    /// <summary>
    /// 앱의 아이콘입니다.
    /// </summary>
    [NotMapped]
    public Icon Icon { get; set; }

    /// <summary>
    /// 주어진 경로로부터 새 <see cref="App"/> 인스턴스를 만들어 옵니다.
    /// DataContext에 추가하지는 않습니다.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static App FromExecutablePath(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"주어진 경로({path})로부터 App 엔티티를 생성할 수 없습니다. 해당 경로에 파일이 존재하지 않습니다.");
        }

        var versionInfo = FileVersionInfo.GetVersionInfo(path);
        var title = GetAppTitle(versionInfo, path);
        var icon = APIWrapper.ExtractAssociatedIcon(path);

        return new App
        {
            ExecutablePath = path,
            Title = title,
            Icon = icon
        };
    }

    private static string GetAppTitle(FileVersionInfo versionInfo, string path)
    {
        var logger = log4net.LogManager.GetLogger(typeof(App));
            
        // 1순위: FileDescription
        if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
        {
            return versionInfo.FileDescription;
        }

        logger.Warn($"앱의 타이틀을 가져오려는데, 실행 파일 '{path}'에 FileDescription 정보가 없습니다. 차선책으로 갑니다.");
        
        // 2순위: ProductName
        if (!string.IsNullOrWhiteSpace(versionInfo.ProductName))
        {
            return versionInfo.ProductName;
        }

        logger.Warn($"앱의 타이틀을 가져오려는데, 실행 파일 '{path}'에 ProductName 정보가 없습니다. 최후의 수단으로 갑니다.");

        // 2.5순위: Program Files 경로에서 제품 폴더명 추출 시도
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                var parts = directory.Split(Path.DirectorySeparatorChar);

                // Program Files 또는 Program Files (x86) 찾기
                var programFilesIndex = -1;
                for (var i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Equals("Program Files", StringComparison.OrdinalIgnoreCase) ||
                        parts[i].Equals("Program Files (x86)", StringComparison.OrdinalIgnoreCase))
                    {
                        programFilesIndex = i;
                        break;
                    }
                }

                // Program Files\회사명\제품명 패턴인지 확인
                if (programFilesIndex >= 0 && programFilesIndex + 2 < parts.Length)
                {
                    var productFolder = parts[programFilesIndex + 2];
                    if (!string.IsNullOrWhiteSpace(productFolder))
                    {
                        logger.Info($"Program Files 경로에서 제품 폴더명 '{productFolder}'을(를) 추출했습니다.");
                        return productFolder;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Warn($"Program Files 경로 분석 중 예외 발생: {ex.Message}");
        }

        // 3순위: 실행 파일 이름 (확장자 제외)
        return Path.GetFileNameWithoutExtension(path);
    }

    public void TryUpdateTitleIfEmpty()
    {
        if (!string.IsNullOrEmpty(Title))
        {
            return;
        }
        
        var versionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);
        Title = GetAppTitle(versionInfo, ExecutablePath);
            
        this.GetLogger().Info($"앱 '{ExecutablePath}'의 Title이 비어있어 자동으로 '{Title}'(으)로 업데이트했습니다.");
    }
}