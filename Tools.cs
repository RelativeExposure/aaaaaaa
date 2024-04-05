
using System.Diagnostics;
using System.Runtime.InteropServices;
using Lidgren.Network;

namespace ConsoleApp1;

public static class NetTools
{
    public static string Serialize(this object target) => Newtonsoft.Json.JsonConvert.SerializeObject(target);
    public static T? Deserialize<T>(this string target) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(target);
    
    
    static NetClient? client;
    public static NetServer? server;
    static bool isClient => client != null;
    public static bool isServer => server != null;
    
    static void SendMessage(string s) => SendMessage([s]);
    static void SendMessage(string[] s)
    {
        if (isClient)
        {
            var msg = client.CreateMessage();
            msg.Write("#$%" + s.Serialize());
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
        }
        else if (isServer)
        {
            var msg = server.CreateMessage();
            msg.Write("#$%" + s.Serialize());
            server.SendToAll(msg, NetDeliveryMethod.ReliableOrdered, 0);
        }
    }

    public static void NetMove(this Piece p) => SendMessage("move⧂" + p.ID + "⧂" + p._pos.Serialize());

    public static void NetSpawn(this Piece p)
    {
        SendMessage("spawn⧂" + p.Serialize());  
    }
    public static void NetDestroy(this Piece p)
    {
        SendMessage("destroy⧂" + p.ID);  
    }

    public static void NetSpawn(this IReadOnlyList<Piece> p)
    {
        string[] msgs = new string[p.Count];
        for (int i = 0; i < p.Count; i++)
            msgs[i] = "spawn⧂" + p[i].Serialize();
        SendMessage(msgs);
    }
    
    static void ReceivedMessage(string[] ss)
    {
        foreach (var s in ss)
        {
            string[] args = s.Split('⧂');
            
            if (args[0] == "spawn")
            {
                Piece spawned = args[1].Deserialize<Piece>()!;
            }
            else if (args[0] == "move")
            {
                if (Piece.All.FirstOrDefault(_ => _.ID == Convert.ToInt32(args[1])) is { } found)
                    found._pos = args[2].Deserialize<v2>();
            }
            else if (args[0] == "destroy")
            {
                if (Piece.All.FirstOrDefault(_ => _.ID == Convert.ToInt32(args[1])) is { } found) 
                    found.Destroy();
            }
        }
    }

    public static void NetUpdate()
    {
        if (isServer)
        {
            var message = server.ReadMessage();
            while (message != null)
            {
                string msg = message.ReadString();
                if (msg.StartsWith("#$%"))
                {
                    msg = msg.Remove(0, 3);
                    ReceivedMessage(msg.Deserialize<string[]>());
                }
                
                message = server.ReadMessage();
            }
        }
        else if (isClient)
        {
            var message = client.ReadMessage();
            while (message != null)
            {
                string msg = message.ReadString();
                if (msg.StartsWith("#$%"))
                {
                    msg = msg.Remove(0, 3);
                    ReceivedMessage(msg.Deserialize<string[]>());
                }
                
                message = client.ReadMessage();
            }
        }
    }
    
    public static void NetSetup()
    {
        RETRY:
        string[]? command = Console.ReadLine()?.Split(' ');
        if (command?[0] == "host")
        {
            var config = new NetPeerConfiguration("consoleGame");
            config.Port = 7777;
            config.MaximumConnections = 999;
            server = new NetServer(config);
            server.Start();
        }
        else if (command?[0] == "join")
        {
            client = new NetClient(new NetPeerConfiguration("consoleGame"));
            client.Start();
            client.Connect(command[1], 7777);
        }
        else goto RETRY;
    }
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