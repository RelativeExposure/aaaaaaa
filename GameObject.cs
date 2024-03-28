namespace ConsoleApp1;

using System.Collections;
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

public class GameObject
{
    public static readonly List<GameObject> All = [];
    public GameObject() => All.Add(this);
        
    public v2 Pos
    {
        get => _pos;
        set
        {
            if (All.Any(_ => _.IsDense && _.Pos == value))
                return;
            _pos = value;
        }
    } public v2 _pos;
        
    public ColorChar Appearance;
    public int Order = 0;
    public bool IsDense = false;
    
    public GameObject Clone() => new GameObject()
    {
        Appearance = Appearance,
        _pos = _pos,
        IsDense = IsDense,
        Order = Order
    };
    public void Destroy() => All.Remove(this);
}

abstract class Component(GameObject owner)
{
    public GameObject Owner = owner;
}

class Animator(GameObject owner) : Component(owner)
{
    
}