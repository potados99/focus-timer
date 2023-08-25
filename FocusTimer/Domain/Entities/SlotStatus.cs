namespace FocusTimer.Domain.Entities;

/// <summary>
/// 슬롯의 상태입니다.
/// </summary>
public class SlotStatus
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 슬롯 번호입니다.
    /// </summary>
    public long SlotNumber { get; set; }

    /// <summary>
    /// 슬롯에 등록된 앱입니다.
    /// </summary>
    public App? App { get; set; }

    public static SlotStatus OfEmpty(long slotNumber)
    {
        return new SlotStatus
        {
            SlotNumber = slotNumber
        };
    }
}