using Google.Cloud.Firestore;
using TaskFlow_G2.DTOs;

namespace TaskFlow_G2.Services;

public class TaskService
{
    private const string TasksCollection = "tasks";

    private readonly FirebaseService _firebaseService;

    public TaskService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    public async Task<TaskResponseDto> Create(string userId, TaskCreateDto dto)
    {
        var collection = _firebaseService.GetCollection(TasksCollection);
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid().ToString();
        
        await collection.Document(id).SetAsync(new Dictionary<string, object>
        {
            { "Title", dto.Title },
            { "PriorityLevel", dto.PriorityLevel },
            { "Notes", dto.Notes ?? string.Empty },
            { "UserId", userId },
            { "CreatedAt", Timestamp.FromDateTime(now) }
        });

        return new TaskResponseDto
        {
            Id = id,
            Title = dto.Title,
            PriorityLevel = dto.PriorityLevel,
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = now
        };
    }

    public async Task<List<TaskResponseDto>> GetAllForUser(string userId)
    {
        var collection = _firebaseService.GetCollection(TasksCollection);

        var snapshot = await collection
            .WhereEqualTo("UserId", userId)
            .OrderByDescending("CreatedAt")
            .GetSnapshotAsync();

        var result = new List<TaskResponseDto>();

        foreach (var doc in snapshot.Documents)
        {
            var data = doc.ToDictionary();

            result.Add(new TaskResponseDto
            {
                Id = doc.Id,
                Title = data["Title"].ToString()!,
                PriorityLevel = Convert.ToInt32(data["PriorityLevel"]),
                Notes = data.TryGetValue("Notes", out var notes) ? notes.ToString()! : string.Empty,
                CreatedAt = ((Timestamp)data["CreatedAt"]).ToDateTime()
            });
        }

        return result;
    }
    
    public async Task<bool> Delete(string userId, string taskId)
    {
        var docRef = _firebaseService.GetCollection(TasksCollection).Document(taskId);
        var doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
        {
            return false;
        }

        var data = doc.ToDictionary();
        
        var ownerId = data["UserId"].ToString();
        if (ownerId != userId)
        {
            return false;
        }

        await docRef.DeleteAsync();
        return true;
    }
}