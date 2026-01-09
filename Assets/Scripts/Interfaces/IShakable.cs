using System.Collections.Generic;

public interface IShakable
{
    // Why do it like this? It allows for the most flexibility out of all methods I could think of
    IEnumerable<Shaker> Shakers { get; }
}
