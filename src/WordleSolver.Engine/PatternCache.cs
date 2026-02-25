using System.Collections.Concurrent;

using WordleSolver.Data;

namespace WordleSolver.Engine;

public sealed class PatternCache
{
    private readonly ConcurrentDictionary<long, Pattern> _cache = new();

    public int Count => _cache.Count;

    private static long EncodeKey(string guess, string answer)
    {
        var key = 0L;

        for (var i = 0; i < 5; i++)
            key = (key << 5) | (char)(guess.ElementAt(i) - 'a');

        for (var i = 0; i < 5; i++)
            key = (key << 5) | (char)(answer.ElementAt(i) - 'a');

        return key;
    }

    public Pattern GetPattern(string guess, string answer)
    {
        var key = EncodeKey(guess, answer);

        return _cache.GetOrAdd(key, Pattern.Compute(guess, answer));
    }

    public void Clear()
    {
        _cache.Clear();
    }
}