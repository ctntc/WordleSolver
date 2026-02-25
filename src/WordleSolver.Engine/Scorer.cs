using System.Runtime.InteropServices;

namespace WordleSolver.Engine;

public sealed class Scorer(PatternCache patternCache, double alpha = 0.7)
{
    private readonly PatternCache _patternCache = patternCache;

    public static double Sigmoid(double x)
    {
        return 1 / (1 + Math.Exp(-x));
    }

    public double Entropy(string guess, IList<string> candidates)
    {
        if (candidates.Count == 0)
            return 0.0;

        var patternCounts = new Dictionary<int, int>();
        foreach (var candidate in candidates)
        {
            var pattern = _patternCache.GetPattern(guess, candidate);

            ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(
                patternCounts,
                pattern.Value,
                out _
            );
            count++;
        }

        double total = candidates.Count;
        double entropy = 0.0;
        foreach (var count in patternCounts.Values)
        {
            double p = count / total;
            entropy -= p * Math.Log2(p);
        }

        return entropy;
    }

    public double Score(double entropy, double maxEntropy, double normalizedFrequency)
    {
        double normalizedEntropy = maxEntropy > 0 ? entropy / maxEntropy : 0.0;
        double frequencyScore = Sigmoid(normalizedFrequency * 10 - 5);
        return alpha * normalizedEntropy + (1 - alpha) * frequencyScore;
    }
}