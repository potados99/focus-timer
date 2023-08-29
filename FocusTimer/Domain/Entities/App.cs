using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using FocusTimer.Lib;
using FocusTimer.Lib.Extensions;
using FocusTimer.Lib.Utility;

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

        var title = FileVersionInfo.GetVersionInfo(path).FileDescription ?? "";
        var icon = APIWrapper.ExtractAssociatedIcon(path);

        return new App
        {
            ExecutablePath = path,
            Title = title,
            Icon = icon
        };
    }
}