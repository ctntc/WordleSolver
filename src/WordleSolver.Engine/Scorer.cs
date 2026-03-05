using System.Runtime.InteropServices;

namespace WordleSolver.Engine;

/// <summary>
///     Calculates word scores based on a combination of expected information gain (entropy) and word frequency, using a
///     tunable parameter alpha to balance the two factors.
/// </summary>
/// <param name="patternCache">The pattern cache used to store and retrieve feedback patterns.</param>
/// <param name="alpha">The weight given to entropy in the final score calculation (0.0 to 1.0).</param>
public sealed class Scorer(PatternCache patternCache, double alpha = 0.7)
{
    /// <summary>
    ///     Applies the sigmoid function to a given value.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <returns>The result of applying the sigmoid function to the input.</returns>
    private static double Sigmoid(double x)
    {
        return 1 / (1 + Math.Exp(-x));
    }

    /// <summary>
    ///     Calculates the expected information gain (entropy) of a guess against a list of candidate answers.
    /// </summary>
    /// <param name="guess">The guessed word.</param>
    /// <param name="candidates">The list of candidate words.</param>
    /// <returns>The calculated entropy value.</returns>
    public double Entropy(string guess, IList<string> candidates)
    {
        if (candidates.Count == 0)
        {
            return 0.0;
        }

        var patternCounts = new Dictionary<int, int>();
        foreach (string candidate in candidates)
        {
            var pattern = patternCache.GetPattern(guess, candidate);

            // C# doesn't have a .Merge() method for Dictionaries like Java, so we use GetValueRefOrAddDefault to get a reference to the count and increment it.
            ref int count = ref CollectionsMarshal.GetValueRefOrAddDefault(
                patternCounts,
                pattern.Value,
                out _
            );
            count++;
        }

        double total = candidates.Count;

        return patternCounts.Values.Select(count => count / total)
            .Aggregate(0.0, (current, p) => current - (p * Math.Log2(p)));
    }

    /// <summary>
    ///     Calculates the final score of a word based on its entropy and frequency.
    /// </summary>
    /// <param name="entropy">The entropy value of the word.</param>
    /// <param name="maxEntropy">The maximum possible entropy value.</param>
    /// <param name="normalizedFrequency">The normalized frequency of the word.</param>
    /// <returns>The calculated score.</returns>
    public double Score(double entropy, double maxEntropy, double normalizedFrequency)
    {
        double normalizedEntropy = maxEntropy > 0 ? entropy / maxEntropy : 0.0;
        double frequencyScore = Sigmoid((normalizedFrequency * 10) - 5);
        return (alpha * normalizedEntropy) + ((1 - alpha) * frequencyScore);
    }
}