namespace BioscoopApplicatie.Controllers;

public class Controller
{
    public void TypeWriterEffect(string message, int delayMs = 10)
    {
        foreach (char character in message)
        {
            Console.Write(character);
            Thread.Sleep(delayMs);
        }

        Console.WriteLine();
    }

    public int ReadNumber(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int value) && value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Choose a number between {minimum} and {maximum}.");
        }
    }
}
