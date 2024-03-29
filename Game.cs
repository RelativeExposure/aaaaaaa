using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lidgren.Network;
using System.Text;

namespace ConsoleApp1;

using static Spawning;
using static Input;
using static Visuals;

public struct v2(int x, int y)
{
    public int x = x, y = y;
    public static bool operator ==(v2 a, v2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(v2 a, v2 b) => !(a == b);
    public static v2 operator +(v2 a, v2 b) => new v2(a.x + b.x, a.y + b.y);
    public static v2 operator -(v2 a, v2 b) => new v2(a.x - b.x, a.y - b.y);
    public float Magnitude => MathF.Sqrt(x * x + y * y);
}

static class Game
{
    static NetClient? client;
    static NetServer? server;
    static bool isClient => client != null;
    static bool isServer => server != null;
    
    static v2 Camera = new (0,0);
    public static Piece? Player;
    static Piece Pointer;
    static Piece wall;

    static int WallsRemaining
    {
        get => _wallsRemaining;
        set
        {
            StatusPanel[0] = new ColorString(" You have ", 'W') + new ColorString(value.ToString(), 'R') + new ColorString(" deployable walls left.", 'W');
            _wallsRemaining = value;
        }
    } static int _wallsRemaining = 5;
    
    public static int Time;

    static int LastShootTime = -99;
    
    static void Update()
    {
        if (Player == null || !Tools.ApplicationIsActivated())
            return;
        
        {
            v2 offset = new v2(0, 0);
            if (IsPressed('W'))
                offset += new v2(0, 1);
            if (IsPressed('A'))
                offset += new v2(-1, 0);
            if (IsPressed('S'))
                offset += new v2(0, -1);
            if (IsPressed('D'))
                offset += new v2(1, 0);
            Player.Pos += offset;
            SendMove(Player);
            Pointer.Pos += offset;
        }
        {
            v2 offset = new v2(0, 0);
            if (IsPressed("\x25\x26\x27\x28"[1]))
                offset += new v2(0, 2);
            if (IsPressed("\x25\x26\x27\x28"[0]))
                offset += new v2(-2, 0);
            if (IsPressed("\x25\x26\x27\x28"[3]))
                offset += new v2(0, -2);
            if (IsPressed("\x25\x26\x27\x28"[2]))
                offset += new v2(2, 0);
            Pointer.Pos += offset;
        }
        
        if (IsPressed('P') && WallsRemaining > 0)
        {
            Spawn(Prefabs.Window, Player.Pos);
            WallsRemaining--;
        }
        
        if (IsPressed('F') & Time - LastShootTime > 5)
        {
            LastShootTime = Time;

            var dir = Pointer.Pos - Player.Pos;
            while (dir.Magnitude < 100)
                dir += dir;
            
            var objs = Drawing.Draw(Player.Pos, Player.Pos + dir);
            
            new Action(() =>
            {
                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    objs[i].Destroy();
                    objs.RemoveAt(i);
                }
            }).Time(10);
        }

        if (Time % 3 == 0)
        {
            wall.Pos += new v2(1, 0);
            SendMove(wall);
        }
        
        // camera follow
        Camera = Player.Pos;
    }

    static void ReceivedMessage(string s)
    {
        //Console.WriteLine("Received message: " + s);
        
        string[] args = s.Split(' ');
        
        if (args[0] == "spawn")
        {
            Piece spawned = args[1].Deserialize<Piece>()!;
        }
        else if (args[0] == "move")
        {
            if (Piece.All.FirstOrDefault(_ => _.ID == Convert.ToInt32(args[1])) is { } found)
                found._pos = args[2].Deserialize<v2>();
        }
    }

    static void SendMove(Piece p) => SendMessage("move " + p.ID + " " + p._pos.Serialize());

    static void SendSpawn(Piece p)
    {
        string str = p.Serialize();
        SendMessage("spawn " + str);  
    } 
    
    static void 
    static void SendMessage(string[] s)
    {
        if (isClient)
        {
            var msg = client.CreateMessage();
            msg.Write(s.Serialize());
            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
        }
        else if (isServer)
        {
            var msg = server.CreateMessage();
            msg.Write(s.Serialize());
            server.SendToAll(msg, NetDeliveryMethod.ReliableOrdered, 0);
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
            
            new Thread(() =>
            {
                while (true)
                {
                    var message = server.ReadMessage();
                    if (message == null) continue;
                    ReceivedMessage(message.ReadString());
                }
            }).Start();
        }
        else if (command?[0] == "join")
        {
            client = new NetClient(new NetPeerConfiguration("consoleGame"));
            client.Start();
            client.Connect(command[1], 7777);

            new Thread(() =>
            {
                while (true)
                {
                    var message = client.ReadMessage();
                    if (message == null) continue;
                    ReceivedMessage(message.ReadString());
                }
            }).Start();
        }
        else goto RETRY;
    }
    
    public static void Main()
    
     {
        Console.OutputEncoding = Encoding.Unicode;
        Console.BufferWidth += 45;
        Console.WindowWidth += 45;
        Console.CursorVisible = false;
        
        NetSetup();

        if (isServer)
        {
            // wait till someone connects
            while (server.ConnectionsCount == 0) { }

            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
            {
                var a = Spawn(Prefabs.Grass, new(x, y));
                SendSpawn(a);
            }

            Player = Spawn(Prefabs.Player, new(0, 0));
            Pointer = Spawn(Prefabs.Pointer, new(0, 0));

            // some walls
            List<Piece> walls =
            [
                Spawn(Prefabs.Wall, new(0, 4)),
                Spawn(Prefabs.Wall, new(0, 3)),
                Spawn(Prefabs.Wall, new(0, 2)),
                Spawn(Prefabs.Wall, new(0, 1)),
                Spawn(Prefabs.Wall, new(0, 0)),
            ];
            wall = walls[0];

            //Drawing.CreateUI(new(5, 5), new(10, 10));
            
            // spawn all
            walls.ForEach(SendSpawn);
        }

        new Thread(() =>
        {
            while (true)
            {
                Update();
                Render();
                Time++;
                Tools.ProcessTime();
            }
        }).Start();
    }

    // Limit is 74
    static List<ColorString> StatusPanel =
    [
        new ColorString(" You have ", 'W') + new ColorString("5", 'R') + new ColorString(" deployable walls left", 'W'),
    ];

    static void AppendStatusPanel(int index, char text)
    {
        while (StatusPanel.Count < index + 1)
            StatusPanel.Add(new (" ", 'W'));
        while (StatusPanel[index].Length == 73)
        {
            index++;
            if (StatusPanel.Count < index + 1)
                StatusPanel.Add(new (" ", 'W'));
        }
        
        StatusPanel[index] += new ColorChar(text);
    }
    static void TrimStatusPanel(int index)
    {
        if (StatusPanel.Count > index)
        {
            if (StatusPanel.Last().Length > 0)
                StatusPanel.Last().List.RemoveAt(StatusPanel.Last().List.Count - 1);
            else
                StatusPanel.RemoveAt(StatusPanel.Count - 1);
        }
    }
    
    
    static readonly Dictionary<v2, ColorChar> current = new();
    static void Render()
    {
        List<List<ColorChar>> desired = new();
        
        int status_i = 0;
        for (int x = -Camera.y - 15; x < -Camera.y + 15; x++)
        {
            desired.Add(new());
            
            for (int y = Camera.x - 45; y < Camera.x + 45; y++)
            {
                if (Piece.All.ToList().Where(_ => _.Pos == new v2(y, -x)).OrderBy(_ => _.Order).LastOrDefault() is { } found)
                    desired[^1].Add(found.Appearance);
                else
                    desired[^1].Add(new(' '));
            }
            
            desired[^1].Add(new('|'));
            
            if (status_i < StatusPanel.Count)
            {
                for (int i = 0; i < StatusPanel[status_i].List.Count; i++)
                    desired[^1].Add(StatusPanel[status_i].List[i]);
                status_i++;
            }
        }

        for (int i = 0; i < desired.Count; i++)
        {
            for (int j = 0; j < desired[i].Count; j++)
            {
                if (!current.ContainsKey(new(j,i)) || current[new(j,i)] != desired[i][j])
                {
                     Console.SetCursorPosition(j,i);
                     Console.ForegroundColor = desired[i][j].ForeColor;
                     Console.BackgroundColor = desired[i][j].BackColor;
                     Console.Write(desired[i][j].Content);
                     if (!current.ContainsKey(new(j, i)))
                         current.Add(new v2(j, i), desired[i][j]);
                     else
                         current[new v2(j, i)] = desired[i][j];
                }
            }
        }
    }
}