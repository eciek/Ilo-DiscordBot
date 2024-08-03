using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    internal class ApiService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //var iloApi = IloApi.Program.CreateApp();
            //return iloApi.RunAsync(token: stoppingToken);
            return Task.CompletedTask;
        }
    }
}
