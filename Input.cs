using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp1;

static class Input
{
    [DllImport("user32.dll")]
    static extern int GetAsyncKeyState(int key);

    public static bool IsPressed(char key)
    {
        if (!Input.IsActive) return false;
        return GetAsyncKeyState(key) != 0;
    } 
    
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public static bool IsActive => OurTitle == GetActiveWindowTitle();
    public static string OurTitle = "";
    public static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();

        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return null;
    }
}