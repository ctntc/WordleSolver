namespace WordleSolver.Application;

using System.CommandLine;

using WordleSolver.Data;

public sealed class Program
{
    static void Main(string[] args)
    {
        Option<string> opener = new("-o", "--opener")
        {
            Description = "Specify the opening word to use (default: 'crane')",
            DefaultValueFactory = _ => "crane",
        };

        Option<bool> useSigmoid = new("--use-sigmoid")
        {
            Description = "Use sigmoid function for frequency scoring (default: false)",
            DefaultValueFactory = _ => false,
        };

        var rootCommand = new RootCommand("Wordle Solver Application");
        rootCommand.Options.Add(opener);
        rootCommand.Options.Add(useSigmoid);

        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count != 0)
        {
            Console.WriteLine("Invalid arguments:");
            foreach (var error in parseResult.Errors)
                Console.WriteLine($"  {error.Message}");
            return;
        }

        var useSigmoidValue = parseResult.GetValue(useSigmoid);

        if (parseResult.GetValue(opener) != "crane")
        {
            var openerValue = parseResult.GetValue(opener)!.ToLower();
            if (!IsGuessValid(openerValue))
            {
                Console.WriteLine("Opener must be 5 letters long.");
                return;
            }
            Repl(openerValue, useSigmoidValue);
        }
        else
        {
            Repl("crane", useSigmoidValue);
        }
    }

    private static bool IsFeedbackValid(string feedback) =>
        feedback.Length == 5 && feedback.All(c => c == 'g' || c == 'y' || c == 'b');

    private static bool IsGuessValid(string guess) =>
        guess.Length == 5 && guess.All(char.IsLetter);

    private static void Repl(string opener, bool useSigmoid)
    {
        var reader = new StreamReader(Console.OpenStandardInput());

        do
        {
            Play(reader, opener, useSigmoid);
        } while (PromptPlayAgain(reader));

        Console.WriteLine("Thanks for playing!");
    }

    private static void Play(StreamReader reader, string opener, bool useSigmoid)
    {
        Console.WriteLine("Welcome to Wordle Solver!");
        Console.WriteLine("=========================");
        Console.WriteLine("Enter your feedback (g for green, y for yellow, b for black).");

        var engine = new Engine.Engine(opener, useSigmoid);

        // First guess, internally uses the opener provided in the constructor.
        engine.MakeGuess();

        while (true)
        {
            var candidate = engine.BestCandidate;

            if (candidate is null)
            {
                Console.Error.WriteLine(
                    "No candidates remaining (the word might not be in our dictionary). Please check your feedback for consistency."
                );
                break;
            }

            int remainingCount = engine.RemainingCandidates.Count;

            Console.WriteLine(
                $"Suggested guess: {candidate} (remaining candidates: {remainingCount})"
            );

            var feedback = reader.ReadLine()?.Trim().ToLower() ?? string.Empty;

            if (new[] { "q", "quit", "exit" }.Contains(feedback))
            {
                Console.WriteLine("Exiting the game. Thanks for playing!");
                return;
            }

            if (!IsFeedbackValid(feedback))
            {
                Console.WriteLine(
                    "Invalid feedback. Please enter a string of 5 characters using 'g', 'y', and 'b' only."
                );
                continue;
            }

            var accuracy = Accuracy.FromString(feedback);

            if (accuracy.All(a => a == Accuracy.Green))
            {
                Console.WriteLine($"Congratulations! You've guessed the word: {candidate}");
                break;
            }

            engine.ProcessFeedback(accuracy);

            if (engine.BestCandidate is not null)
                engine.MakeGuess();

            Console.WriteLine();
        }
    }

    private static bool PromptPlayAgain(StreamReader reader)
    {
        Console.Write("Do you want to play again? (y/n): ");
        var response = reader.ReadLine()?.Trim().ToLower() ?? string.Empty;
        return response == "y";
    }
}