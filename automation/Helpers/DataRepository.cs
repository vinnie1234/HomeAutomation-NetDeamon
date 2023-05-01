using System.IO;

namespace Automation.Helpers;

public class DataRepository : IDataRepository
{
    // ReSharper disable once MemberInitializerValueIgnored
    private readonly string _dataStoragePath = "./apps/.storage";
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger _logger;

    public DataRepository(string dataStoragePath, ILogger logger)
    {
        _dataStoragePath = dataStoragePath;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions();
    }
    
    public T? Get<T>(string id) where T : class
    {
        try
        {
            var storageJsonFile = Path.Combine(_dataStoragePath, $"{id}_store.json");

            if (!File.Exists(storageJsonFile))
                return null;

            using var jsonStream = File.OpenRead(storageJsonFile);

            return JsonSerializer.Deserialize<T>(jsonStream, _jsonOptions);
        }
        catch (Exception e)
        {
            _logger.LogError("Error getting storage file {Id}, error message: {Error}", id, e.Message);
        }
        
        return default;
    }

    /// <inheritdoc/>
    public void Save<T>(string id, T data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        SaveInternal<T>(id, data);
    }

    private void SaveInternal<T>(string id, T data)
    {
        var storageJsonFile = Path.Combine(_dataStoragePath, $"{id}_store.json");

        if (!Directory.Exists(_dataStoragePath))
        {
            Directory.CreateDirectory(_dataStoragePath);
        }

        using var jsonStream = File.Open(storageJsonFile, FileMode.Create, FileAccess.Write);

        JsonSerializer.Serialize(jsonStream, data);
    }
}