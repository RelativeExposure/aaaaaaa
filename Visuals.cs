namespace ConsoleApp1;

public static class Visuals
{
    public static ConsoleColor toCc(this char s) => s switch
    {
        'W' => ConsoleColor.White,
        '_' => ConsoleColor.Black,
        
        'B' => ConsoleColor.Blue,
        'b' => ConsoleColor.DarkBlue,
        
        'G' => ConsoleColor.Green,
        'g' => ConsoleColor.DarkGreen,
        
        'C' => ConsoleColor.Cyan,
        'c' => ConsoleColor.DarkCyan,
        
        'R' => ConsoleColor.Red,
        'r' => ConsoleColor.DarkRed,
        
        'M' => ConsoleColor.Magenta,
        'm' => ConsoleColor.DarkMagenta,
        
        'Y' => ConsoleColor.Yellow,
        'y' => ConsoleColor.DarkYellow,
        
        'X' => ConsoleColor.DarkGray,
        'x' => ConsoleColor.Gray,
    };

    public struct ColorChar
    {
        public char Content;
        public ConsoleColor ForeColor = ConsoleColor.White;
        public ConsoleColor BackColor = ConsoleColor.Black;

        public ColorChar(char a)
        {
            Content = a;
        }
        public ColorChar(char a, ConsoleColor b)
        {
            Content = a;
            ForeColor = b;
        }
        public ColorChar(char a, ConsoleColor b, ConsoleColor c)
        {
            Content = a;
            ForeColor = b;
            BackColor = c;
        }

        public static bool operator ==(ColorChar a, ColorChar b) =>
            a.BackColor == b.BackColor &&
            a.ForeColor == b.ForeColor &&
            a.Content == b.Content;
        public static bool operator !=(ColorChar a, ColorChar b) => !(a == b);

        public void Write()
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            
            Console.ForegroundColor = ForeColor;
            Console.BackgroundColor = BackColor;
            Console.Write(Content);

            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
        }
    }
    
    public struct ColorString
    {
        public List<ColorChar> List;

        public ColorString(string text, string color)
        {
            List = new();
            for (int i = 0; i < text.Length; i++)
                List.Add(new (text[i], color[i].toCc()));
        }
        public ColorString(string text, char color)
        {
            List = new();
            for (int i = 0; i < text.Length; i++)
                List.Add(new(text[i], color.toCc()));
        }

        public void Write()
        {
            var storedFore = Console.ForegroundColor;
            var storedBack = Console.BackgroundColor;

            string buffer = "";
            
            ConsoleColor lastFore = List[0].ForeColor;
            ConsoleColor lastBack = List[0].BackColor;
            for (int i = 0; i < List.Count; i++)
            {
                if (lastFore == List[i].ForeColor && lastBack == List[i].BackColor)
                    buffer += List[i].Content;
                else
                {
                    Console.ForegroundColor = lastFore;
                    Console.BackgroundColor = lastBack;
                    Console.Write(buffer);
                    buffer = "";
                    lastFore = List[i].ForeColor;
                    lastBack = List[i].BackColor;
                }
            }
            Console.ForegroundColor = lastFore;
            Console.BackgroundColor = lastBack;
            Console.Write(buffer);
            
            Console.ForegroundColor = storedFore;
            Console.BackgroundColor = storedBack;
        }

        public int Length => List.Count;

        public static ColorString operator +(ColorString a, ColorString b)
        {
            var copy = a.List.ToList();
            copy.AddRange(b.List);
            return new() { List = copy };
        }
        public static ColorString operator +(ColorString a, ColorChar b)
        {
            var copy = a.List.ToList();
            copy.Add(b);
            return new() { List = copy };
        }
    }
    
    public struct Sprite(v2 center)
    {
        public v2 Center = center;
    
        Map map =
        [
            ['#','#','#','#','#','#','#','#'],
            ['#',' ',' ',' ',' ',' ',' ','#'],
            ['#',' ','#',' ',' ','#',' ','#'],
            ['#',' ','#',' ',' ','#',' ','#'],
            ['#',' ','#',' ',' ','#',' ','#'],
            ['#',' ',' ',' ',' ',' ',' ','#'],
            ['#',' ',' ',' ',' ',' ',' ','#'],
            ['#',' ','#',' ',' ','#',' ','#'],
            ['#',' ','#',' ',' ','#',' ','#'],
            ['#',' ','#','#','#','#',' ','#'],
            ['#',' ',' ',' ',' ',' ',' ','#'],
            ['#','#','#','#','#','#','#','#'],
        ];
        Map colorMap =
        [
            ['W','W','W','W','W','W','W','W'],
            ['W',' ',' ',' ',' ',' ',' ','W'],
            ['W',' ','W',' ',' ','W',' ','W'],
            ['W',' ','W',' ',' ','W',' ','W'],
            ['W',' ','W',' ',' ','W',' ','W'],
            ['W',' ',' ',' ',' ',' ',' ','W'],
            ['W',' ',' ',' ',' ',' ',' ','W'],
            ['W',' ','W',' ',' ','W',' ','W'],
            ['W',' ','W',' ',' ','W',' ','W'],
            ['W',' ','W','W','W','W',' ','W'],
            ['W',' ',' ',' ',' ',' ',' ','W'],
            ['W','W','W','W','W','W','W','W'],
        ];
    }
}