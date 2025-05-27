using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheBrainOfficeServer;

public class UsbPortMonitorService : IHostedService, IDisposable
{
    private readonly IHubContext<SensorHub> _hubContext;
    private Timer _timer;
    private HashSet<string> _known = new();

    public UsbPortMonitorService(IHubContext<SensorHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // проверяем сразу, а затем каждые 2 секунды
        _timer = new Timer(CheckPorts, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        return Task.CompletedTask;
    }

    private void CheckPorts(object _)
    {
        // собираем текущий список имен устройств
        var current = Directory
            .GetFiles("/dev", "ttyUSB*")
            .Select(f => Path.GetFileName(f).Replace("ttyUSB", ""))
            .ToHashSet();

        if (!current.SetEquals(_known))
        {
            _known = current;
            // пушим обновлённый список портов (масcив строк, например ["0","1","2"])
            _hubContext.Clients.All.SendAsync("UpdatePortList", _known);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}