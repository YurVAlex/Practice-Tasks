using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet;


internal static class TimerLoop
{
    public static async Task LaunchBatchTasksAsync(int loopPeriod, params Task[] tasks)
    {
        while (true)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i].Start();
            }
            await Task.Delay(loopPeriod);
        }
    }

    public static async Task LaunchBatchActionsAsync(int loopPeriod, params Action[] act)
    {
        while (true)
        {
            for (int i = 0; i < act.Length; i++)
            {
                act[i].Invoke();
            }
            await Task.Delay(loopPeriod);
        }
    }

    public static async Task LaunchBatchCombineAsync(int loopPeriod, Task task, params Action[] act)
    {
        while (true)
        {
            await task;

            for (int i = 0; i < act.Length; i++)
            {
                act[i].Invoke();
            }
            await Task.Delay(loopPeriod);
        }
    }
}
