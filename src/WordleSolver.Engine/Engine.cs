using WordleSolver.Data;

namespace WordleSolver.Engine;

/// <summary>
///     Wordle solver engine using entropy-based scoring with sigmoid frequency weighting.<br />
///     <br />The solver works by:
///     <br />1. Maintaining a list of remaining possible answers
///     <br />2. For each guess, filtering answers based on the feedback pattern
///     <br />3. Selecting the next guess by maximizing a blended score of entropy and word frequency
/// </summary>
public sealed class Engine
{
    private readonly List<AllowedWord> _allWords;
    private readonly List<string> _candidates;
    private readonly Dictionary<string, double> _frequencyMap;
    private readonly PatternCache _patternCache;
    private readonly Scorer _scorer;
    private string _bestCandidate;

    private string _currentGuess;

    public Engine(string opener, bool useSigmoid)
    {
        _allWords = [.. DataLoader.LoadAllowedWords()];
        _candidates = [.. DataLoader.LoadCandidates()];
        _patternCache = new PatternCache();
        _scorer = new Scorer(_patternCache, useSigmoid ? 0.7 : 1.0);

        _frequencyMap = [];
        foreach (var word in _allWords)
        {
            _frequencyMap[word.Word] = word.NormalizedFrequency;
        }

        _currentGuess = opener;
        _bestCandidate = opener;
    }

    public string? BestCandidate => _candidates.Count > 0 ? _bestCandidate : null;

    public List<string> RemainingCandidates => [.. _candidates];

    /// <summary>
    ///     Makes the next guess by selecting the best candidate based on the current scoring strategy.
    /// </summary>
    public void MakeGuess()
    {
        _currentGuess = _bestCandidate;
    }

    /// <summary>
    ///     Processes the feedback from the previous guess and updates the list of remaining candidates.
    /// </summary>
    /// <param name="feedback">The feedback received for the previous guess.</param>
    public void ProcessFeedback(List<Accuracy> feedback)
    {
        var pattern = Pattern.FromFeedback(feedback);

        _candidates.RemoveAll(candidate =>
            _patternCache.GetPattern(_currentGuess, candidate) != pattern
        );

        UpdateBestCandidate();
    }

    /// <summary>
    ///     Updates the best candidate word based on the current scoring strategy.
    /// </summary>
    private void UpdateBestCandidate()
    {
        switch (_candidates.Count)
        {
            case 0:
                return;
            case 1:
                _bestCandidate = _candidates.First();
                return;
        }

        var guessPool = _candidates.Count <= 20 ? _candidates : _allWords.Select(w => w.Word).ToList();

        double maxEntropy = Math.Log2(_candidates.Count / Math.Log2(2));

        string? bestWord = guessPool
            .AsParallel()
            .Select(guess =>
            {
                double entropy = _scorer.Entropy(guess, _candidates);
                double frequency = _frequencyMap.GetValueOrDefault(guess, 0.0);
                double score = _scorer.Score(entropy, maxEntropy, frequency);

                if (_candidates.Contains(guess))
                {
                    score += 0.01; // Slightly favor candidates
                }

                return new ScoredWord(guess, score);
            })
            .MaxBy(sw => sw.Score)
            .Word;

#if DEBUG
        foreach (string candidate in _candidates)
        {
            Console.WriteLine($"Debug: Candidate '{candidate}'");
        }

        Console.WriteLine($"Debug: Best candidate is '{bestWord}' with score {guessPool.First(g => g == bestWord)}");
#endif

        _bestCandidate = bestWord ?? _candidates.First();
    }
}

internal readonly record struct ScoredWord(string Word, double Score);