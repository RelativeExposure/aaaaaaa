
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ConsoleApp1;

public static class NetTools
{
    public static string Serialize(this object target) => Newtonsoft.Json.JsonConvert.SerializeObject(target);
    public static T? Deserialize<T>(this string target) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(target);
}

public static class Tools
{
    static List<(int, Action)> timedActions = new();
    public static void Time(this Action a, int delay) => timedActions.Add((Game.Time + delay, a));
    public static void ProcessTime()
    {
        for (int i = timedActions.Count - 1; i >= 0; i--)
        {
            if (timedActions[i].Item1 < Game.Time)
                timedActions.RemoveAt(i);
            else if (timedActions[i].Item1 == Game.Time)
            {
                timedActions[i].Item2();
                timedActions.RemoveAt(i);
            }
        }
    }
    
    /// <summary>Returns true if the current application has focus, false otherwise</summary>
    public static bool ApplicationIsActivated()
    {
        var activatedHandle = GetForegroundWindow();
        if (activatedHandle == IntPtr.Zero) {
            return false;       // No window is currently activated
        }

        var procId = Process.GetCurrentProcess().Id;
        int activeProcId;
        GetWindowThreadProcessId(activatedHandle, out activeProcId);

        return activeProcId == procId;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
}