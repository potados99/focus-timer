using FocusTimer.Data.Repositories;

namespace FocusTimer.Domain.Services;

public class AppService
{
    private readonly UsageRepository _repository;
    
    public AppService(UsageRepository repository)
    {
        _repository = repository;
    }

    public Entities.App GetOrCreateApp(string path)
    {
        var existing = _repository.FindAppByPath(path);
        
        return existing ?? Entities.App.FromExecutablePath(path);
    }
}