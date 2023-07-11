using System.IO;

namespace Automation.Repository;

public class DataRepository : IDataRepository
{
    private readonly string _dataStoragePath;
    private readonly ILogger _logger;

    public DataRepository(string dataStoragePath, ILogger logger)
    {
        _dataStoragePath = dataStoragePath;
        _logger = logger;
    }
    
    public T? Get<T>(string id) where T : class
    {
        try
        {
            var storageJsonFile = Path.Combine(_dataStoragePath, $"{id}_store.json");

            if (!File.Exists(storageJsonFile))
                return null;

            using var jsonStream = File.OpenRead(storageJsonFile);

            return JsonSerializer.Deserialize<T>(jsonStream);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting storage file {Id}, error message: {Error}", id, ex.Message);
        }
        
        return default;
    }
    
    public void Save<T>(string id, T data)
    {
        SaveInternal(id, data);
    }

    private void SaveInternal<T>(string id, T data)
    {
        var storageJsonFile = Path.Combine(_dataStoragePath, $"{id}_store.json");
        Directory.CreateDirectory(_dataStoragePath);

        using var jsonStream = File.Open(storageJsonFile, FileMode.Create, FileAccess.Write);

        JsonSerializer.Serialize(jsonStream, data);
    }
}