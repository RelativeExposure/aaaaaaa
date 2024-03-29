namespace ConsoleApp1;

using System.Collections;
using System.Text.Json;
using static Visuals;

public struct Map : IEnumerable<List<char>>
{
    public Map(int width, int height)
    {
        for (int i = 0; i < height; i++)
        {
            var list = new List<char>();
            for (int j = 0; j < width; j++)
                list.Add(' ');
            map.Add(list);
        }
    }
    
    public v2 dimensions;
    public List<List<char>> map = [];

    public Map AddSticker(Map a)
    {
        if (dimensions != a.dimensions)
            throw new();
            
        for (int i = 0; i < a.map.Count; i++)
            for (int j = 0; j < a.map[i].Count; j++)
                if (a.map[i][j] != ' ')
                    map[i][j] = a.map[i][j];
                    
        return this;
    }
    
    public IEnumerator<List<char>> GetEnumerator() => map.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Add(List<char> c) => map.Add(c);
}


public class PieceSet
{
    public List<Piece> Pieces;
    public v2 CommonPos
    {
        get => Pieces[0]._pos;
        set => Pieces.ForEach(_ => _._pos += (value - CommonPos));
    }
}

public class Piece
{
    public static readonly List<Piece> All = [];

    public Piece()
    {
        ID = new Random().Next(int.MinValue, int.MaxValue);
        while(All.Any(_ => _.ID == ID))
            ID = new Random().Next(int.MinValue, int.MaxValue);
        All.Add(this);
    }

    public v2 Pos
    {
        get => _pos;
        set
        {
            if (!IgnoreCollision && All.Any(_ => _.IsDense && _.Pos == value))
                return;
            _pos = value;
        }
    } public v2 _pos;
        
    public ColorChar Appearance;
    public int Order = 0;
    public bool IsDense = false;
    public bool IgnoreCollision = false;
    public int ID;
    
    public Piece Clone() => new Piece()
    {
        Appearance = Appearance,
        _pos = _pos,
        Order = Order,
        IsDense = IsDense,
        IgnoreCollision = IgnoreCollision,
        ID = ID
    };
    public void Destroy() => All.Remove(this);
}

abstract class Component(Piece owner)
{
    public Piece Owner = owner;
}

class Animator(Piece owner) : Component(owner)
{
    
}