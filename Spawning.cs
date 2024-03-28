namespace ConsoleApp1;

static class Spawning
{
    public static GameObject Spawn(Prefabs which, v2 at)
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
        Line
    }
    static readonly Dictionary<Prefabs, GameObject> Prefab = new()
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
                Order = 1,
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
                Appearance = new ('═', ConsoleColor.DarkCyan),
                IsDense = false,
                Order = 3,
            }
        },
    };
}