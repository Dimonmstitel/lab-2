using System;
using System.IO;
using System.Text;

namespace CatAndMouse
{
    public enum State
    {
        Winner,
        Loser,
        Playing,
        NotInGame
    }

    public enum GameState
    {
        Start,
        End
    }

    public class Player
    {
        public string Name { get; set; }
        public int Location { get; set; } = -1;
        public State State { get; set; } = State.NotInGame;
        public int DistanceTraveled { get; set; } = 0;

        public Player(string name)
        {
            Name = name;
            Location = -1;
        }

        public void Move(int steps, int fieldSize)
        {
            if (State == State.NotInGame)
            {
                int initialPos = ((steps - 1) % fieldSize + fieldSize) % fieldSize + 1;
                Location = initialPos;
                State = State.Playing;
            }
            else
            {
                int newZeroBased = ((Location - 1 + steps) % fieldSize + fieldSize) % fieldSize;
                Location = newZeroBased + 1;
                DistanceTraveled += Math.Abs(steps);
            }
        }
    }

    public class Game
    {
        public static string InputFile { get; set; } = "ChaseData.txt";
        public static string OutFile { get; set; } = "PursuitLog.txt";

        public int Size { get; set; }
        public Player Cat { get; set; }
        public Player Mouse { get; set; }
        public GameState Status { get; set; }

        private StringBuilder logBuffer = new StringBuilder();

        public Game(int size)
        {
            Size = size;
            Cat = new Player("Cat");
            Mouse = new Player("Mouse");
            Status = GameState.Start;
        }

        public void Run()
        {
            if (!File.Exists(InputFile))
            {
                Console.WriteLine($"Файл {InputFile} не найден!");
                return;
            }

            string[] lines = File.ReadAllLines(InputFile);
            if (lines.Length == 0) return;

            if (int.TryParse(lines[0].Trim(), out int parsedSize))
            {
                Size = parsedSize;
            }

            logBuffer.AppendLine("Cat and Mouse");
            logBuffer.AppendLine();
            logBuffer.AppendLine("Cat Mouse  Distance");
            logBuffer.AppendLine("-------------------");

            int currentLine = 1;

            while (Status != GameState.End && currentLine < lines.Length)
            {
                string line = lines[currentLine].Trim();
                currentLine++;

                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                char command = parts[0][0];

                if (command == 'P')
                {
                    DoPrintCommand();
                }
                else if (command == 'M' || command == 'C')
                {
                    if (parts.Length > 1 && int.TryParse(parts[1], out int steps))
                    {
                        DoMoveCommand(command, steps);
                    }
                }

                if (Cat.State == State.Playing && Mouse.State == State.Playing && Cat.Location == Mouse.Location)
                {
                    Cat.State = State.Winner;
                    Mouse.State = State.Loser;
                    Status = GameState.End;
                }
            }

            logBuffer.AppendLine("-------------------");
            logBuffer.AppendLine();
            logBuffer.AppendLine();
            logBuffer.AppendLine($"Distance traveled:   Mouse    Cat");
            logBuffer.AppendLine($"                      {Mouse.DistanceTraveled,4}   {Cat.DistanceTraveled,4}");
            logBuffer.AppendLine();

            if (Mouse.State == State.Loser)
            {
                logBuffer.AppendLine($"Mouse caught at: {Cat.Location}");
            }
            else
            {
                logBuffer.AppendLine("Mouse evaded Cat");
            }

            string resultOutput = logBuffer.ToString();
            File.WriteAllText(OutFile, resultOutput);
            Console.WriteLine(resultOutput);
        }

        private void DoMoveCommand(char command, int steps)
        {
            switch (command)
            {
                case 'M':
                    Mouse.Move(steps, Size);
                    break;
                case 'C':
                    Cat.Move(steps, Size);
                    break;
            }
        }

        private void DoPrintCommand()
        {
            string catLoc = Cat.State == State.NotInGame ? "??" : Cat.Location.ToString();
            string mouseLoc = Mouse.State == State.NotInGame ? "??" : Mouse.Location.ToString();

            if (Cat.State == State.NotInGame || Mouse.State == State.NotInGame)
            {
                logBuffer.AppendLine($"{catLoc,3}  {mouseLoc,4}");
            }
            else
            {
                int distance = GetDistance();
                logBuffer.AppendLine($"{catLoc,3}  {mouseLoc,4}       {distance,3}");
            }
        }

        private int GetDistance()
        {
            if (Cat.State == State.NotInGame || Mouse.State == State.NotInGame) return -1;

            int d1 = Math.Abs(Cat.Location - Mouse.Location);
            int d2 = Size - d1;
            return Math.Min(d1, d2);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 1; i <= 3; i++)
            {
                Console.WriteLine($"================ ИГРА {i} ================");
                Game.InputFile = $"{i}.ChaseData.txt";
                Game.OutFile = $"{i}.PursuitLog.txt";

                Game game = new Game(10000);
                game.Run();
            }
        }
    }
}