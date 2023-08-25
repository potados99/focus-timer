using System.Collections.Generic;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;

namespace FocusTimer.Domain.Services;

public class SlotService
{
    private readonly UsageRepository _repository;

    public SlotService(UsageRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<Slot> GetSlotStatuses()
    {
        return _repository.GetSlotStatuses();
    }

    public Slot GetOrCreateStatus(long slotNumber)
    {
        var existing = _repository.FindSlotStatusBySlotNumber(slotNumber);

        return existing ?? CreateNewStatus(slotNumber);
    }

    public Slot CreateNewStatus(long slotNumber)
    {
        var status = Slot.OfEmpty(slotNumber);
        
        _repository.StartTracking(status);

        return status;
    }

    public void SaveRepository()
    {
        _repository.Save();
    }
}