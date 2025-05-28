using System.Collections.Concurrent;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using TheBrainOfficeServer.Models;

namespace TheBrainOfficeServer;

public class SensorHub : Hub
{
    // Открытые порты и токены отмены по ConnectionId
    private static readonly ConcurrentDictionary<string, SerialPort> Ports = new();
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> ReadTokens = new();

    [HubMethodName("OpenPort")]
    public async Task OpenPort(string portNumber)
    {
        var connectionId = Context.ConnectionId;
        var groupName = $"port{portNumber}";

        // Подписываем клиента на группу порта
        await Groups.AddToGroupAsync(connectionId, groupName);

        try
        {
            // Открываем последовательный порт
            var serialPort = new SerialPort($"/dev/ttyUSB{portNumber}", 115200)
            {
                ReadTimeout = 1500,
                WriteTimeout = 1500,
                Encoding = System.Text.Encoding.UTF8
            };
            serialPort.Open();
            serialPort.DiscardInBuffer();

            Ports[connectionId] = serialPort;

            // Готовим токен отмены
            var cts = new CancellationTokenSource();
            ReadTokens[connectionId] = cts;
            var token = cts.Token;

            // Захватываем прокси группы один раз
            var groupProxy = Clients.Group(groupName);
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Фоновый таск чтения
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var line = serialPort.ReadLine().Trim();
                        if (line.StartsWith("{"))
                        {
                            var data = JsonSerializer.Deserialize<SensorData>(line, jsonOptions);
                            if (data != null)
                            {
                                // Отправляем всем участникам группы
                                await groupProxy.SendAsync("ReceiveSensorData", data);
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        // просто продолжаем
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка чтения порта для {connectionId}: {ex.Message}");
                        break;
                    }
                }
            }, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось открыть /dev/ttyUSB{portNumber}: {ex.Message}");
            throw new HubException($"Ошибка открытия порта: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;

        // Удаляем из группы (чтобы не слать несуществующему клиенту)
        // Номер порта здесь нужно хранить, но если принимаем только один порт, можно
        // извлечь его из Ports[connectionId] или передать клиенту.
        // Для простоты — удалим из всех потенциальных групп:
        foreach (var key in Ports.Keys)
            await Groups.RemoveFromGroupAsync(connectionId, $"port{key}");

        // Останавливаем таск и закрываем порт
        if (ReadTokens.TryRemove(connectionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        if (Ports.TryRemove(connectionId, out var port))
        {
            if (port.IsOpen) port.Close();
            port.Dispose();
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Стрим списка портов, без изменений
    public async IAsyncEnumerable<string[]> StreamAvailablePorts(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            string[] files;
            try
            {
                files = Directory
                    .GetFiles("/dev", "ttyUSB*")
                    .Select(Path.GetFileName)
                    .Select(n => n.Replace("ttyUSB", ""))
                    .ToArray();
            }
            catch
            {
                files = Array.Empty<string>();
            }
            yield return files;

            try { await Task.Delay(2000, cancellationToken); }
            catch (TaskCanceledException) { yield break; }
        }
    }
}
