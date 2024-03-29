namespace ConsoleApp1;

static class Spawning
{
    public static Piece Spawn(Prefabs which, v2 at)
    {
        var created = Prefab[which].Clone();
        created._pos = at;
        return created;
    }

    public enum Prefabs
    {
        Grass,
        Player,
        Wall,
        Window,
        Line,
        Pointer,
    }
    static readonly Dictionary<Prefabs, Piece> Prefab = new()
    {
        {
            Prefabs.Grass, new()
            {
                _pos = new(-999,-999),
                Appearance = new ('_', ConsoleColor.DarkGreen),
            }
        },
        {
            Prefabs.Player, new()
            {
                _pos = new(-999,-999),
                Appearance = new ('P', ConsoleColor.White),
                IsDense = true,
                Order = 2,
            }
        },
        {
            Prefabs.Wall, new()
            {
                _pos = new(-999,-999),
                Appearance = new ('#', ConsoleColor.Red, ConsoleColor.Red),
                IsDense = true,
                Order = 2,
            }
        },
        {
            Prefabs.Window, new()
            {
                _pos = new(-999,-999),
                Appearance = new ('#', ConsoleColor.Blue, ConsoleColor.DarkRed),
                IsDense = true,
                Order = 2,
            }
        },
        {
            Prefabs.Line, new()
            {
                _pos = new(-999,-999),
                Appearance = new (' ', ConsoleColor.DarkCyan),
                IsDense = false,
                Order = 1,
            }
        },
        {
            Prefabs.Pointer, new()
            {
                _pos = new(-999,-999),
                Appearance = new ('X', ConsoleColor.White),
                IsDense = false,
                Order = 3,
                IgnoreCollision = true
            }
        },
    };
}