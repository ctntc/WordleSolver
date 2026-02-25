using System.Diagnostics;

namespace WordleSolver.Data;

/// <summary>
///     Compact representation of a Wordle feedback pattern.
///     Encodes 5 positions x 3 states (Black = 0, Yellow = 1, Green = 2) as a base-3 number.
///     Total range: 0 to 242 (3^5 -1 = 242), fits in a single byte.
/// </summary>
/// <param name="Value">value of the encoded pattern.</param>
public readonly record struct Pattern(int Value)
{
    /// <summary>
    ///     Pattern value representing all green (correct word).
    /// </summary>
    private const int AllGreen = 2 * 81 + 2 * 27 + 2 * 9 + 2 * 3 + 2;

    /// <summary>
    ///     Check if this pattern represents all green (word solved).
    /// </summary>
    public bool IsAllGreen => Value == AllGreen;

    /// <summary>
    ///     Compute the feedback pattern when guessing <paramref name="guess" /> against <paramref name="answer" />.
    ///     Implements official Wordle rules for duplicate letter handling.
    /// </summary>
    /// <param name="guess">the guessed word (5 lowercase letters).</param>
    /// <param name="answer">the target answer (5 lowercase letters). </param>
    /// <returns>the computed feedback pattern.</returns>
    public static Pattern Compute(string guess, string answer)
    {
        var result = new int[5];
        var letterCounts = new int[26];

        // Counter letters in answer.
        for (var i = 0; i < 5; i++)
            letterCounts[answer.ElementAt(i) - 'a']++;

        // First pass: mark greens and decrement counts.
        for (var i = 0; i < 5; i++)
            if (guess.ElementAt(i) == answer.ElementAt(i))
            {
                result[i] = (int)Accuracy.Green;
                letterCounts[guess.ElementAt(i) - 'a']--;
            }

        // Second pass: mark yellows where a letter exists elsewhere.
        for (var i = 0; i < 5; i++)
            if (result[i] != (int)Accuracy.Green)
            {
                var index = guess.ElementAt(i) - 'a';
                if (letterCounts[index] > 0)
                {
                    result[i] = (int)Accuracy.Yellow;
                    letterCounts[index]--;
                }
                // else remains 0 (Black).
            }

        // Encode as a base-3- number: position 0 is the most significant.
        return new Pattern(result[0] * 81 + result[1] * 27 + result[2] * 9 + result[3] * 3 + result[4]);
    }

    /// <summary>
    ///     Convert user feedback into a pattern.
    /// </summary>
    /// <param name="feedback">list of 5 <see cref="WordleSolver.Data.Accuracy" /> values from user input.</param>
    /// <returns>the encoded pattern.</returns>
    public static Pattern FromFeedback(IList<Accuracy> feedback)
    {
        int value = 0;
        int multiplier = 81;

        foreach (var accuracy in feedback)
        {
            value +=
                accuracy switch
                {
                    Accuracy.Green => 2,
                    Accuracy.Yellow => 1,
                    Accuracy.Black => 0,
                    _ => throw new UnreachableException(),
                } * multiplier;

            multiplier /= 3;
        }

        return new Pattern(value);
    }

    /// <summary>
    ///     Decode this pattern back to a list of <see cref="WordleSolver.Data.Accuracy" /> values.
    /// </summary>
    /// <returns>list of 5 <see cref="WordleSolver.Data.Accuracy" /> values.</returns>
    public IList<Accuracy> ToAccuracyList()
    {
        var result = new Accuracy[5];
        var remaining = Value;
        int[] divisors = [81, 27, 9, 3, 1];

        for (var i = 0; i < 5; i++)
        {
            var digit = remaining / divisors[i];
            remaining %= divisors[i];
            result[i] = digit switch
            {
                2 => Accuracy.Green,
                1 => Accuracy.Yellow,
                _ => Accuracy.Black,
            };
        }

        return result;
    }
}