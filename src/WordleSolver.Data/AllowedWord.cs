namespace WordleSolver.Data;

/// <summary>
///     Represents a word allowed in the Wordle game, along with its frequency and normalized frequency.
/// </summary>
/// <param name="Word">The word itself.</param>
/// <param name="Frequency">The frequency of the word.</param>
/// <param name="NormalizedFrequency">The normalized frequency of the word.</param>
public readonly record struct AllowedWord(string Word, double Frequency, double NormalizedFrequency)
    : IComparable<AllowedWord>
{
    public int CompareTo(AllowedWord other)
    {
        return NormalizedFrequency.CompareTo(other.NormalizedFrequency);
    }
}