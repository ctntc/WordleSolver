namespace WordleSolver.Data;

public readonly record struct AllowedWord(string Word, double Frequency, double NormalizedFrequency)
    : IComparable<AllowedWord>
{
    public int CompareTo(AllowedWord other)
    {
        return NormalizedFrequency.CompareTo(other.NormalizedFrequency);
    }
}