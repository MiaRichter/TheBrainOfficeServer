using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using TheBrainOfficeServer.Models;

namespace TheBrainOfficeServer;

public class SensorHub : Hub
{
    // Словари для хранения открытых портов и токенов отмены по ConnectionId клиента
    private static readonly ConcurrentDictionary<string, SerialPort> Ports = new();
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> ReadTokens = new();
    [HubMethodName("OpenPort")] // Явное указание имени метода
    public async Task OpenPort(string portNumber)
    {
        var connectionId = Context.ConnectionId;

        // Закрыть старый порт, если есть
        //ClosePort(connectionId);

        try
        {
            var serialPort = new SerialPort($"/dev/ttyUSB{portNumber}", 115200)
            {
                ReadTimeout = 1500,
                WriteTimeout = 1500,
                Encoding = Encoding.UTF8
            };

            serialPort.Open();
            serialPort.DiscardInBuffer();

            Ports[connectionId] = serialPort;

            var cts = new CancellationTokenSource();
            ReadTokens[connectionId] = cts;

            var token = cts.Token;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var clientProxy = Clients.Client(connectionId);
            // Запуск таска для чтения данных с порта
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        string line = serialPort.ReadLine().Trim();
                        if (!string.IsNullOrEmpty(line) && line.StartsWith("{"))
                        {
                            SensorData data = null;
                            try
                            {
                                data = JsonSerializer.Deserialize<SensorData>(line, options);
                            }
                            catch (JsonException)
                            {
                                continue;
                            }

                            if (data != null)
                            {
                                await clientProxy.SendAsync("ReceiveSensorData", data); // Используем clientProxy, а не Clients.Client(...)
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка чтения с порта для клиента {connectionId}: {ex.Message}");
                        break;
                    }
                }
            }, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось открыть порт /dev/ttyUSB{portNumber} для клиента {connectionId}: {ex.Message}");
            throw new HubException($"Ошибка открытия порта: {ex.Message}");
        }
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;
        ClosePort(connectionId);
        return base.OnDisconnectedAsync(exception);
    }

    private void ClosePort(string connectionId)
    {
        if (ReadTokens.TryRemove(connectionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }

        if (Ports.TryRemove(connectionId, out var port))
        {
            try
            {
                if (port.IsOpen)
                    port.Close();
                port.Dispose();
            }
            catch
            {
                // Игнорируем ошибки закрытия порта
            }
        }
    }

    public string ListUsbPorts()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"ls /dev/ttyUSB*\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine("Ошибка при выполнении команды ls: " + error);
                return $"Ошибка: {error}";
            }

            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка выполнения команды ls: " + ex.Message);
            return $"Ошибка: {ex.Message}";
        }
    }
}