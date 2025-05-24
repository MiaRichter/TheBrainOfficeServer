using System.IO.Ports;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using TheBrainOfficeServer.Models;

namespace TheBrainOfficeServer;

public class SensorHub : Hub
{
    private static SerialPort _serialPort;
    private static CancellationTokenSource _cts;

    public async IAsyncEnumerable<SensorData> StreamSensorData(string portNumber)
    {
        _cts = new CancellationTokenSource();
        _serialPort = new SerialPort($"/dev/ttyUSB{portNumber}", 115200)
        {
            ReadTimeout = 1500,
            WriteTimeout = 1500,
            Encoding = System.Text.Encoding.UTF8
        };

        _serialPort.Open();
        _serialPort.DiscardInBuffer();

        try
        {
            while (!_cts.IsCancellationRequested)
            {
                string jsonData = null;
                bool hasError = false;
                string errorMessage = null;

                try
                {
                    jsonData = await Task.Run(() => 
                    {
                        try { return _serialPort.ReadLine(); }
                        catch (TimeoutException) { return null; }
                    }, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    hasError = true;
                    errorMessage = ex.Message;
                }

                if (hasError)
                {
                    Console.WriteLine($"Ошибка: {errorMessage}");
                    await Clients.Caller.SendAsync("ReceiveError", errorMessage);
                    continue;
                }

                if (!string.IsNullOrEmpty(jsonData) && jsonData.StartsWith("{"))
                {
                    SensorData data = null;
                    try
                    {
                        data = JsonSerializer.Deserialize<SensorData>(jsonData, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Ошибка десериализации: {ex.Message}");
                        await Clients.Caller.SendAsync("ReceiveError", "Invalid data format");
                        continue;
                    }

                    if (data != null)
                    {
                        yield return data;
                    }
                }
            }
        }
        finally
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _cts?.Cancel();
        await base.OnDisconnectedAsync(exception);
    }
}