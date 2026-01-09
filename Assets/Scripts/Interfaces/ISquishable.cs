using System.Collections.Generic;

public interface ISquishable
{
    // 'Squishers' sounds really weird and kind of suggestive /:
    IEnumerable<Squisher> Squishers { get; }
}
