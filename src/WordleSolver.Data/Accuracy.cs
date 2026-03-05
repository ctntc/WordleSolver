namespace WordleSolver.Data;

/// <summary>
///     Represents the accuracy of a guessed letter in the Wordle game.
/// </summary>
public enum Accuracy
{
    Black = 0,
    Yellow = 1,
    Green = 2
}

public static class AccuracyExtensions
{
    extension(Accuracy)
    {
        /// <summary>
        ///     Converts a string representation of accuracies into a list of <see cref="Accuracy" /> values.
        /// </summary>
        /// <param name="value">The string representation of accuracies (e.g., "bgy").</param>
        /// <returns>A list of <see cref="Accuracy" /> values corresponding to the input string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input string contains invalid characters.</exception>
        public static List<Accuracy> FromString(string value)
        {
            List<Accuracy> result = [];

            result.AddRange(value.Select(c => c switch
            {
                'b' => Accuracy.Black,
                'y' => Accuracy.Yellow,
                'g' => Accuracy.Green,
                _ => throw new ArgumentException($"Invalid accuracy value: {value}")
            }));

            return result;
        }
    }
}