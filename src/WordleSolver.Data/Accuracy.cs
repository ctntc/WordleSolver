using System.Diagnostics;

namespace WordleSolver.Data;

public enum Accuracy
{
    Black = 0,
    Yellow = 1,
    Green = 2,
}

public static class AccuracyExtensions
{
    extension(Accuracy)
    {
        public static List<Accuracy> FromString(string value)
        {
            List<Accuracy> result = [];

            foreach (var c in value)
            {
                result.Add(
                    c switch
                    {
                        'b' => Accuracy.Black,
                        'y' => Accuracy.Yellow,
                        'g' => Accuracy.Green,
                        _ => throw new ArgumentException($"Invalid accuracy value: {value}"),
                    }
                );
            }
            return result;
        }
    }

    extension(Accuracy accuracy)
    {
        public string Display()
        {
            return accuracy switch
            {
                Accuracy.Black => "black",
                Accuracy.Yellow => "yellow",
                Accuracy.Green => "green",
                _ => throw new UnreachableException(),
            };
        }
    }
}