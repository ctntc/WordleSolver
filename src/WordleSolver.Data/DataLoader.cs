namespace WordleSolver.Data;

public static class DataLoader
{
    public static IEnumerable<AllowedWord> LoadAllowedWords()
    {
        var lines = File.ReadAllLines("dictionary.txt");
        var totalFrequency = lines.Select(line => double.Parse(line.Split(' ')[1])).Sum();
        var allowedWords = lines
            .Select(line =>
            {
                var parts = line.Split(' ');
                var word = parts[0];
                var frequency = double.Parse(parts[1]);
                var normalizedFrequency = frequency / totalFrequency;
                return new AllowedWord(word, frequency, normalizedFrequency);
            })
            .ToList();
        return allowedWords;
    }

    public static IEnumerable<string> LoadCandidates() => File.ReadAllLines("answers.txt");
}