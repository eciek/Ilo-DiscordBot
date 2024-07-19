using DiscordBot.Models;
using System.Runtime.CompilerServices;
using System.Timers;

namespace DiscordBot.Services;

public enum TimerJobTiming
{
    OneTimeCountDown = 1,
    NowAndRepeatOnInterval = 2,
    TriggerDailyAtSetMinute = 3
}

public class TimerService
{
    private readonly ILogger<TimerService> _logger;
    private readonly System.Timers.Timer _timer;

    double _minutes;
    DateTime _today;

    readonly List<TimerJob> _timerJobs;

    public TimerService(ILogger<TimerService> logger)
    {
        _logger = logger;

        _timer = new()
        {
            // 1 minute
            Interval = 60 * 1000,
            Enabled = true
        };
        
        _timer.Start();
        _timer.Elapsed += new ElapsedEventHandler(Timer_Tick);

        _today = DateTime.Today;
        _minutes = double.Floor(DateTime.Now.TimeOfDay.TotalMinutes);
        _timerJobs = [];

        _logger.LogInformation("Timer Started! [Day:{Date} ] [minute: {minutes}]",_today.Date, _minutes);
    }

    public void RegisterJob(TimerJob job)
    {
        if (_timerJobs.Where(x => x.Name == job.Name).Any() == false)
        {
            _timerJobs.Add(job);
            _logger.LogInformation("\"TimerJob [{Name}] Registered! Executing in [{Interval}] minutes\"", job.Name, job.Interval);
        }
    }


    private void Timer_Tick(object? sender, ElapsedEventArgs e)
    {
        _minutes++;

        // triggers around 0:00-0:01
        if (_today != DateTime.Today)
        {
            _today = DateTime.Today;
            _minutes = 0;
        }

        foreach (var job in _timerJobs.Where(x=> x.TargetMinute == _minutes))
        {
            job.ExecuteJob();
        }

        _timerJobs.RemoveAll(x=> x.IsDone);
    }
}

