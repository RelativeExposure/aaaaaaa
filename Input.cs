using System.Runtime.InteropServices;

namespace ConsoleApp1;

static class Input
{
    [DllImport("user32.dll")]
    static extern int GetAsyncKeyState(int key);
    public static bool IsPressed(char key) => GetAsyncKeyState(key) != 0;
}