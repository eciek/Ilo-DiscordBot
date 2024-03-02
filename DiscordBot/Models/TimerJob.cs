using DiscordBot.Services;

namespace DiscordBot.Models;

public class TimerJob
{
    public string Name { get; init; }
    public TimerJobTiming Timing { get; init; }

    //Time units in minutes
    public int Interval { get; private set; }

    public double TargetMinute { get; protected set; }

    public bool IsDone { get; protected set; } = false;

    private readonly Action _targetMethod;

    public TimerJob(string jobName, int interval, TimerJobTiming timing, Action method)
    {
        Name = jobName;
        Interval = interval;
        Timing = timing;
        _targetMethod = method;
        TargetMinute = double.Floor(DateTime.Now.TimeOfDay.TotalMinutes);

        switch (Timing)
        {
            case TimerJobTiming.OneTimeCountDown:
                TargetMinute += Interval;
                break;
            case TimerJobTiming.NowAndRepeatOnInterval:
                TargetMinute += 1;

                // Check for next day execution
                var dayInMinutes = TimeSpan.FromDays(1).TotalMinutes;
                while (TargetMinute >= dayInMinutes)
                {
                    TargetMinute -= dayInMinutes;
                }
                break;
            case TimerJobTiming.TriggerDailyAtSetMinute:
                TargetMinute = Interval;
                break;
        }
    }

    public Task ExecuteJob()
    {
        Console.WriteLine($"TimerJob [{Name}] Invoked! Starting [{_targetMethod.Method.Name}]");
        try
        {
            _targetMethod.Invoke();
        }
        catch (Exception ex) { Console.WriteLine($"TimerJob [{Name}] failed! Next execution in [{Interval}] minutes \n" + ex.Message); };

        UpdateTriggerTime();
        return Task.CompletedTask;
    }

    private void UpdateTriggerTime()
    {
        switch (Timing)
        {
            case TimerJobTiming.OneTimeCountDown:
                IsDone = true;
                return;
            case TimerJobTiming.NowAndRepeatOnInterval:
                TargetMinute += Interval;

                // Check for next day execution
                var dayInMinutes = TimeSpan.FromDays(1).TotalMinutes;
                while (TargetMinute >= dayInMinutes)
                {
                    TargetMinute -= dayInMinutes;
                }
                break;
            case TimerJobTiming.TriggerDailyAtSetMinute:
                TargetMinute = Interval;
                break;
        }
    }
}
