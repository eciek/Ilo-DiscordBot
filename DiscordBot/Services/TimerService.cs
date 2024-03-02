using DiscordBot.Models;
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
    private readonly System.Timers.Timer _timer;

    double _minutes;
    DateTime _today;

    readonly List<TimerJob> _timerJobs;

    public TimerService()
    {
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

        Console.WriteLine($"Timer Started! [Day:{_today.Date} ] [minute: {_minutes}]");
    }

    public void RegisterJob(TimerJob job)
    {
        if (_timerJobs.Where(x => x.Name == job.Name).Any() == false)
        {
            _timerJobs.Add(job);
            Console.WriteLine($"TimerJob [{job.Name}] Registered! Executing in [{job.Interval}] minutes");
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

