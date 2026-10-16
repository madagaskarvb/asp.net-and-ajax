using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace LiveMonitoring;

public class ConnectionTracker
{
    // connectionId -> имя комнаты (null = без комнаты)
    private readonly ConcurrentDictionary<string, string?> _connections = new();
    private readonly IHubContext<MonitoringHub> _hubContext;

    public ConnectionTracker(IHubContext<MonitoringHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public int Count => _connections.Count;

    public async Task RegisterAsync(string connectionId)
    {
        _connections[connectionId] = null;
        await BroadcastStateAsync();
    }

    public async Task RemoveAsync(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
        await BroadcastStateAsync();
    }

    public async Task UpdateRoomAsync(string connectionId, string? roomName)
    {
        if (_connections.ContainsKey(connectionId))
        {
            _connections[connectionId] = string.IsNullOrWhiteSpace(roomName) ? null : roomName;
        }
        // Теперь при смене комнаты тоже рассылаем обновлённый список
        await BroadcastStateAsync();
    }

    public IReadOnlyDictionary<string, string?> GetAll() => _connections;

    // Единый метод рассылки: и число онлайн, и полный список пользователей
    private async Task BroadcastStateAsync()
    {
        // 1. Число онлайн (совместимость с прошлым заданием)
        await _hubContext.Clients.All.SendAsync("OnlineCount", _connections.Count);

        // 2. Полный список: массив объектов { connectionId, room }
        var list = _connections
            .Select(kv => new
            {
                connectionId = kv.Key,
                room = kv.Value ?? "без комнаты"
            })
            .ToList();

        await _hubContext.Clients.All.SendAsync("UserList", list);
    }

    public async Task SendSystemMessageAsync(string text)
    {
        await _hubContext.Clients.All.SendAsync("SystemMessage", text);
    }

    public async Task SendToRoomAsync(string roomName, string text)
    {
        await _hubContext.Clients.Group(roomName)
            .SendAsync("ExternalRoomMessage", roomName, text);
    }

    public async Task SendPrivateAsync(string targetConnectionId, string text)
    {
        await _hubContext.Clients.Client(targetConnectionId)
            .SendAsync("PrivateMessage", "external", text);
    }
}

public record SystemMessageRequest(string Text);
public record RoomMessageRequest(string Room, string Text);
public record PrivateMessageRequest(string ConnectionId, string Text);