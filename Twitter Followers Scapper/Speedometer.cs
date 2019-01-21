using System;

class Speedometer
{
    private int StartTickCount;
    public int Progress { get; set; }
    
    public void Start()
    {
        Progress = 0;
        StartTickCount = Environment.TickCount;
    }

    public double GetProgressPerSecond()
    {
        return Progress / TimeSpan.FromMilliseconds(Environment.TickCount - StartTickCount).TotalSeconds;
    }

    public double GetProgressPerHour()
    {
        return Progress / TimeSpan.FromMilliseconds(Environment.TickCount - StartTickCount).TotalHours;
    }
}
