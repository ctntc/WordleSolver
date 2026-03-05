using System.Reflection;

namespace WordleSolver.Data;

/// <summary>
///     Provides methods for loading Wordle game data, such as allowed words and candidate answers.
/// </summary>
public static class DataLoader
{
    private static readonly Assembly ThisAssembly = typeof(DataLoader).Assembly;

    /// <summary>
    ///     Loads the allowed words for the Wordle game, along with their frequencies and normalized frequencies.
    /// </summary>
    /// <returns>An enumerable of <see cref="AllowedWord" />.</returns>
    public static IEnumerable<AllowedWord> LoadAllowedWords()
    {
        string? resourceName = ThisAssembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("dictionary.txt"));
        if (resourceName == null)
        {
            throw new FileNotFoundException("Could not find embedded resource 'dictionary.txt'.");
        }

        using var stream = ThisAssembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Could not load embedded resource 'dictionary.txt'.");
        }

        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            lines.Add(reader.ReadLine()!);
        }

        double totalFrequency = lines.Select(line => double.Parse(line.Split(' ')[1])).Sum();
        var allowedWords = lines
            .Select(line =>
            {
                string[] parts = line.Split(' ');
                string word = parts[0];
                double frequency = double.Parse(parts[1]);
                double normalizedFrequency = frequency / totalFrequency;
                return new AllowedWord(word, frequency, normalizedFrequency);
            })
            .ToList();
        return allowedWords;
    }

    /// <summary>
    ///     Loads the candidate answers for the Wordle game.
    /// </summary>
    /// <returns>An enumerable of candidate answer strings.</returns>
    public static IEnumerable<string> LoadCandidates()
    {
        string? resourceName = ThisAssembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("candidates.txt"));
        if (resourceName == null)
        {
            throw new FileNotFoundException("Could not find embedded resource 'candidates.txt'.");
        }

        using var stream = ThisAssembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException("Could not load embedded resource 'candidates.txt'.");
        }

        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            lines.Add(reader.ReadLine()!);
        }

        return lines;
    }
}