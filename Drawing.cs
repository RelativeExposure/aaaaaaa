namespace ConsoleApp1;

public static class Drawing
{
    static readonly List<GameObject> drawn = [];
    public static void Draw(v2 a, v2 b)
    {
        for (int i = drawn.Count - 1; i >= 0; i--)
        {
            drawn[i].Destroy();
            drawn.RemoveAt(i);
        }
        
        var result = CastLine(a, b);
        result.ForEach(_ => drawn.Add(Spawning.Spawn(Spawning.Prefabs.Line, _)));
    }
    
    static List<v2> CastLine(v2 a, v2 b)
    {
        List<v2> path = [];
        
        int width = b.x - a.x;
        int height = b.y - a.y;

        v2 dA = new(0, 0);
        v2 dB = new(0, 0);

        dA.x = width switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dA.x
        };
        dA.y = height switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dA.y
        };
        dB.x = width switch
        {
            < 0 => -1,
            > 0 => 1,
            _ => dB.x
        };

        int longest = Math.Abs(width);
        int shortest = Math.Abs(height);
        if (longest <= shortest)
        {
            longest = Math.Abs(height);
            shortest = Math.Abs(width);
            if (height < 0)
                dB.y = -1;
            else if (height > 0)
                dB.y = 1;
            dB.x = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            path.Add(new (a.x,a.y));
            numerator += shortest;
            if (numerator >= longest)
            {
                numerator -= longest;
                a.x += dA.x;
                a.y += dA.y;
            }
            else
            {
                a.x += dB.x;
                a.y += dB.y;
            }
        }

        return path;
    }
}