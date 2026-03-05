using System.Collections.Concurrent;

using WordleSolver.Data;

namespace WordleSolver.Engine;

/// <summary>
///     Thread-safe cache for storing computed feedback patterns for guess-answer pairs.
///     Uses compact long keys to minimize memory usage and optimize retrieval speed.
/// </summary>
public sealed class PatternCache
{
    private readonly ConcurrentDictionary<long, Pattern> _cache = new();

    /// <summary>
    ///     Encode a guess-answer pair into a compact long key.
    /// </summary>
    /// <param name="guess">the guessed word (5 lowercase letters).</param>
    /// <param name="answer">the target answer (5 lowercase letters).</param>
    /// <returns>a compact long key representing the guess-answer pair.</returns>
    private static long EncodeKey(string guess, string answer)
    {
        long key = 0L;

        for (int i = 0; i < 5; i++)
        {
            key = (key << 5) | (char)(guess.ElementAt(i) - 'a');
        }

        for (int i = 0; i < 5; i++)
        {
            key = (key << 5) | (char)(answer.ElementAt(i) - 'a');
        }

        return key;
    }

    /// <summary>
    ///     Retrieve the feedback pattern for a given guess-answer pair, computing and caching it if necessary.
    /// </summary>
    /// <param name="guess">the guessed word (5 lowercase letters).</param>
    /// <param name="answer">the target answer (5 lowercase letters).</param>
    /// <returns>the feedback pattern for the guess-answer pair.</returns>
    public Pattern GetPattern(string guess, string answer)
    {
        long key = EncodeKey(guess, answer);

        return _cache.GetOrAdd(key, _ => Pattern.Compute(guess, answer));
    }
}