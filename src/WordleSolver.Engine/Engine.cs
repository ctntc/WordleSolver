using WordleSolver.Data;

namespace WordleSolver.Engine;

public sealed class Engine
{
    private readonly List<AllowedWord> _allWords;
    private readonly List<string> _candidates;
    private readonly Dictionary<string, double> _frequencyMap;
    private readonly PatternCache _patternCache;
    private readonly Scorer _scorer;

    private string _currentGuess;
    private string _bestCandidate;

    public Engine(string opener, bool useSigmoid)
    {
        _allWords = [.. DataLoader.LoadAllowedWords()];
        _candidates = [.. DataLoader.LoadCandidates()];
        _patternCache = new PatternCache();
        _scorer = new Scorer(_patternCache, useSigmoid ? 0.7 : 1.0);

        _frequencyMap = [];
        foreach (var word in _allWords)
            _frequencyMap[word.Word] = word.NormalizedFrequency;

        _currentGuess = opener;
        _bestCandidate = opener;
    }

    public void MakeGuess() =>
            _currentGuess = _bestCandidate;

    public void ProcessFeedback(List<Accuracy> feedback)
    {
        var pattern = Pattern.FromFeedback(feedback);

        _candidates.RemoveAll(candidate =>
            _patternCache.GetPattern(_currentGuess, candidate) != pattern
        );

        UpdateBestCandidate();
    }

    public string? BestCandidate => _candidates.Count > 0 ? _bestCandidate : null;

    public List<string> RemainingCandidates => [.. _candidates];

    private void UpdateBestCandidate()
    {
        if (_candidates.Count == 0)
            return;

        if (_candidates.Count == 1)
        {
            _bestCandidate = _candidates.First();
            return;
        }

        var guessPool = _candidates.Count <= 20 ? _candidates : _allWords.Select(w => w.Word);

        double maxEntropy = Math.Log2(_candidates.Count / Math.Log2(2));

        var bestWord = guessPool
            .AsParallel()
            .Select(guess =>
            {
                double entropy = _scorer.Entropy(guess, _candidates);
                double frequency = _frequencyMap.GetValueOrDefault(guess, 0.0);
                double score = _scorer.Score(entropy, maxEntropy, frequency);

                if (_candidates.Contains(guess))
                    score += 0.01; // Slightly favor candidates

                return new ScoredWord(guess, score);
            })
            .MaxBy(sw => sw.Score)
            .Word;

#if DEBUG
        foreach (var candidate in _candidates)
            Console.WriteLine($"Debug: Candidate '{candidate}'");

        Console.WriteLine($"Debug: Best candidate is '{bestWord}' with score {guessPool.First(g => g == bestWord)}");
#endif

        _bestCandidate = bestWord ?? _candidates.First();
    }
}

internal readonly record struct ScoredWord(string Word, double Score);