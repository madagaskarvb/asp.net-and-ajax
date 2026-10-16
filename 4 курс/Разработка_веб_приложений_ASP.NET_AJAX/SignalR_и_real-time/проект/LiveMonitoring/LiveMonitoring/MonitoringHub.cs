using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace LiveMonitoring;

public class MonitoringHub : Hub
{
    private readonly ConnectionTracker _tracker;

    public MonitoringHub(ConnectionTracker tracker)
    {
        _tracker = tracker;
    }

    // ===== Задание 1 =====
    public override async Task OnConnectedAsync()
    {
        await _tracker.RegisterAsync(Context.ConnectionId);
        await Clients.Others.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _tracker.RemoveAsync(Context.ConnectionId);
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId, message);
    }

    // ===== Задание 2 =====
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await _tracker.UpdateRoomAsync(Context.ConnectionId, roomName);
        await Clients.Caller.SendAsync("JoinedRoom", roomName);
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await _tracker.UpdateRoomAsync(Context.ConnectionId, null);
        await Clients.Caller.SendAsync("LeftRoom", roomName);
    }

    public async Task SendToRoom(string roomName, string message)
    {
        await Clients.Group(roomName)
            .SendAsync("ReceiveRoomMessage", roomName, Context.ConnectionId, message);
    }

    // ===== Задание 5 =====
    public async Task SendPrivate(string targetConnectionId, string message)
    {
        await Clients.Client(targetConnectionId)
            .SendAsync("PrivateMessage", Context.ConnectionId, message);

        await Clients.Caller.SendAsync("PrivateSent", targetConnectionId, message);
    }

    // ===== Доп. задание: «Печатает…» =====
    public async Task NotifyTyping(string roomName)
    {
        // ОТЛАДКА: увидим в консоли Visual Studio
        Console.WriteLine($"[HUB] Получен NotifyTyping для комнаты '{roomName}' от {Context.ConnectionId}");

        // Всем в комнате, КРОМЕ отправителя
        await Clients.OthersInGroup(roomName)
            .SendAsync("UserTyping", roomName, Context.ConnectionId);
    }

    // ===== Сложный уровень: потоковая передача =====
    public async IAsyncEnumerable<int> StreamNumbers(
        int maxValue,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Если клиент передал 0 или отрицательное — защищаемся
        if (maxValue <= 0) maxValue = 100;

        var rnd = new Random();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Пауза 500 мс. При отмене Task.Delay выбросит исключение
                await Task.Delay(500, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Клиент отменил поток или отключился — завершаем цикл
                yield break;
            }

            yield return rnd.Next(0, maxValue + 1);
        }
    }
}