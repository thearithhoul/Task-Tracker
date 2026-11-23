
using System.Text.Json;
using TaskTracker.Models;

namespace TaskTracker.Services;

public class LocalStorageService
{
    private  readonly string DefaultFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"TaskTracker"
    );

    private  readonly JsonSerializerOptions options = new JsonSerializerOptions() {WriteIndented = true};

    private readonly string FilePath;
    private readonly string FolderPath;

    
    public LocalStorageService(string? filePath = null)
    {
        FilePath = filePath ?? Path.Combine(DefaultFolderPath, "tasks.json");
        FolderPath = Path.GetDirectoryName(FilePath)!;
    }

    public  bool Save(List<TaskItem> items)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            string json = JsonSerializer.Serialize(items, options);
            File.WriteAllText(FilePath, json);

            return true;
        }
        catch
        {
            
            return false;
        }

    }

    public List<TaskItem> Load()
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }
        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<TaskItem>>(json ) ?? [];
    }
}