using DiscordBot.Services;

namespace DiscordBot.Interfaces;

public abstract class TimerJob
{
    public TimerJobTiming Timing { get; init; }

    //Time units in minutes
    public int Interval { get; private set; }

    public double TargetMinute { get; protected set; }

    public bool IsDone { get; protected set; } = false;

    public TimerJob( int interval, TimerJobTiming timing)
    {
        Interval = interval;
        Timing = timing;

        switch (Timing)
        {
            case TimerJobTiming.OneTimeCountDown:
                TargetMinute += Interval;
                break;
            case TimerJobTiming.NowAndRepeatAfterCountDown:
                TargetMinute += 1;

                // Check for next day execution
                var dayInMinutes = TimeSpan.FromDays(1).TotalMinutes;
                while (TargetMinute >= dayInMinutes)
                {
                    TargetMinute -= dayInMinutes;
                }
                break;
            case TimerJobTiming.TriggerDailyAtSetHour:
                TargetMinute = Interval;
                break;
        }

    }

    public virtual Task HandleJob()
    {
        UpdateTriggerTime();
        return Task.CompletedTask;
    }

    protected void UpdateTriggerTime()
    {
        switch (Timing)
        {
            case TimerJobTiming.OneTimeCountDown:
                IsDone = true;
                return;
            case TimerJobTiming.NowAndRepeatAfterCountDown:
                TargetMinute += Interval;

                // Check for next day execution
                var dayInMinutes = TimeSpan.FromDays(1).TotalMinutes;
                while (TargetMinute >= dayInMinutes)
                {
                    TargetMinute -= dayInMinutes;
                }
                break;
            case TimerJobTiming.TriggerDailyAtSetHour:
                TargetMinute = Interval;
                break;
        }
    }
}
