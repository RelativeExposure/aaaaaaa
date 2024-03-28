using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
}

static class Game
{
    static v2 Camera = new (0,0);
    static GameObject Player;
    static GameObject wall;

    static int WallsRemaining
    {
        get => _wallsRemaining;
        set
        {
            StatusPanel[0] = new ColorString(" You have ", 'W') + new ColorString(value.ToString(), 'R') + new ColorString(" deployable walls left.", 'W');
            _wallsRemaining = value;
        }
    } static int _wallsRemaining = 5;
    
    static int Time;
    static void Update()
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

        if (IsPressed('P') && WallsRemaining > 0)
        {
            Spawn(Prefabs.Window, Player.Pos);
            WallsRemaining--;
        }
        
        Player.Pos += offset;
        
        if (Time % 3 == 0)
            wall.Pos += new v2(1, 0);
        
        // camera follow
        Camera = Player.Pos;
        
        Drawing.Draw(new v2(0,0), Player.Pos);
    }
    
    public static void Main()
    {
        Console.OutputEncoding = Encoding.Unicode;
        Console.BufferWidth += 45;
        Console.WindowWidth += 45;
        Console.CursorVisible = false;
        
        for (int x = 0; x < 25; x++)
            for (int y = 0; y < 25; y++)
                Spawn(Prefabs.Grass, new(x,y));
        
        // player
        Player = Spawn(Prefabs.Player, new(0, 0));
        
        // some walls
        List<GameObject> walls =
        [
            Spawn(Prefabs.Wall, new(0,0)),
            Spawn(Prefabs.Wall, new(0,1)),
            Spawn(Prefabs.Wall, new(0,2)),
            Spawn(Prefabs.Wall, new(0,3)),
            Spawn(Prefabs.Wall, new(0,4)),
        ];
        wall = walls[3];
        
        while (true)
        {
            Update();
            Render();
            //Render();
            Time++;
        }
    }

    static List<ColorString> StatusPanel =
    [
        new ColorString(" You have ", 'W') + new ColorString("5", 'R') + new ColorString(" deployable walls left", 'W')
    ];
    
    
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
                if (GameObject.All.Where(_ => _.Pos == new v2(y, -x)).OrderBy(_ => _.Order).LastOrDefault() is { } found)
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