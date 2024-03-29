namespace ConsoleApp1;



public static class Drawing
{
    public static List<Piece> Draw(v2 a, v2 b)
    {
        List<Piece> output = new();

        bool alt = Math.Abs(a.x - b.x) < Math.Abs(a.y - b.y);
            
        var result = CastLine(a, b, _ => Piece.All.Any(__ => __.Pos == _ && __.IsDense && __ != Game.Player));
        result.ForEach(_ =>
        {
            var item = Spawning.Spawn(Spawning.Prefabs.Line, _);
            item.Appearance = new(!alt ? '╴' : '╷', ConsoleColor.Yellow);
            output.Add(item);
        });

        return output;
    }

    public delegate bool pred(v2 at);
    static List<v2> CastLine(v2 a, v2 b, pred? StopCondition = null)
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
            if (StopCondition is { } __ && __(new(a.x, a.y)))
                return path;
            
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

    static readonly string thinSet = "└┐┌┘│─";
    static readonly string thickSet = "╚╗╔╝║═";
    
    public static void CreateUI(v2 min, v2 max, bool thick = false)
    {
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                v2 c = new v2(x, y);

                char s = ' ';

                string charSet = !thick ? thinSet : thickSet;
                
                if (c == min)
                    s = charSet[0];
                else if (c == max)
                    s = charSet[1];
                else if (c.x == min.x && c.y == max.y)
                    s = charSet[2];
                else if (c.y == min.y && c.x == max.x)
                    s = charSet[3];
                else if (c.x == min.x || c.x == max.x)
                    s = charSet[4];
                else if (c.y == min.y || c.y == max.y)
                    s = charSet[5];
                
                new Piece
                {
                    _pos = c,
                    Appearance = new(s),
                    Order = 5
                };
            }
        }
    }
}