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
    static v2 Camera = new (0,0);
    public static Piece? Player;
    static Piece Pointer;
    static Piece? movingWall;

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
    static int LastMoveTime = -99;
    static int LastCursorMoveTime = -99;
    
    static void Update()
    {
        if (Player == null)
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

            if (offset != new v2(0, 0) && Time - LastMoveTime > 5)
            {
                LastMoveTime = Time;
                
                Player.Pos += offset;
                Player.NetMove();
                
                Pointer.Pos += offset;
            }
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

            if (offset != new v2(0, 0) && Time - LastCursorMoveTime > 1)
            {
                LastCursorMoveTime = Time;
                Pointer.Pos += offset;
            }
        }
        
        if (IsPressed('P') && WallsRemaining > 0)
        {
            Spawn(Prefabs.Window, Player.Pos);
            WallsRemaining--;
        }
        
        if (IsPressed('F') & Time - LastShootTime > 15)
        {
            LastShootTime = Time;

            var dir = Pointer.Pos - Player.Pos;
            while (dir.Magnitude < 50)
                dir += dir;
            
            var objs = Drawing.Draw(Player.Pos, Player.Pos + dir);
            objs.NetSpawn();
            
            new Action(() =>
            {
                for (int i = objs.Count - 1; i >= 0; i--)
                {
                    objs[i].Destroy();
                    objs[i].NetDestroy();
                    objs.RemoveAt(i);
                }
            }).Time(10);
        }

        if (movingWall != null && Time % 25 == 0)
        {
            movingWall.Pos += new v2(1, 0);
            movingWall.NetMove();
        }
        
        // camera follow
        Camera = Player.Pos;
    }

    
    
    
    
    public static void Main()
    {
        Input.OurTitle = new Random().Next(0, 999).ToString();

        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(5);
                Console.Title = OurTitle;
            }
        }).Start();
            
            
        Console.OutputEncoding = Encoding.Unicode;
        Console.BufferWidth += 45;
        Console.WindowWidth += 45;
        Console.CursorVisible = false;

        NetTools.NetSetup();

        if (NetTools.isServer)
        {
            // wait till someone connects
            while (NetTools.server.ConnectionsCount == 0)
            {
            }

            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
            {
                var a = Spawn(Prefabs.Grass, new(x, y));
                a.NetSpawn();
            }
            
            // some walls
            List<Piece> walls =
            [
                Spawn(Prefabs.Wall, new(0, 4)),
                Spawn(Prefabs.Wall, new(0, 3)),
                Spawn(Prefabs.Wall, new(0, 2)),
                Spawn(Prefabs.Wall, new(0, 1)),
                Spawn(Prefabs.Wall, new(0, 0)),
            ];
            walls.NetSpawn();
            movingWall = walls[0];
            
            Drawing.CreateUI(new(5, 5), new(10, 10))
                .NetSpawn();
        }

        
        
        Player = Spawn(Prefabs.Player, new(0, 0));
        Pointer = Spawn(Prefabs.Pointer, new(0, 0));
        Player.NetSpawn();

        new Thread(() =>
        {
            while (true)
            {
                NetTools.NetUpdate();
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