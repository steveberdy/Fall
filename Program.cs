using System.CommandLine;

const int DEFAULT_ROW_COUNT = 2;
const int DEFAULT_COLUMN_COUNT = 20;
const int DEFAULT_FREQUENCY = 8;
const string DEFAULT_VALUES = "|/\\";

Option<int> rowsOpt = new("-r", getDefaultValue: () => DEFAULT_ROW_COUNT, "Number of rows");
Option<int> columnsOpt = new("-c", getDefaultValue: () => DEFAULT_COLUMN_COUNT, "Number of columns");
Option<int> frequencyOpt=  new("-f", getDefaultValue: () => DEFAULT_FREQUENCY, "Frequency (hertz) at which the waterfall moves");
Option<string> valuesOpt = new("-v", getDefaultValue: () => DEFAULT_VALUES, "Values to use");
Option<bool> inputOpt = new("-i", "Allows using up/down arrows to increase/decrease frequency");

var rootCommand = new RootCommand
{
    rowsOpt,
    columnsOpt,
    frequencyOpt,
    valuesOpt,
    inputOpt
};
rootCommand.SetHandler<int, int, int, string, bool>((rows, columns, frequency, values, input) =>
{
    rows = rows > 0 ? rows : DEFAULT_ROW_COUNT;
    columns = columns > 0 ? columns : DEFAULT_COLUMN_COUNT;
    frequency = frequency > 0 ? frequency : DEFAULT_FREQUENCY;
    values = string.IsNullOrEmpty(values) ? DEFAULT_VALUES : values;
    int msPerFrame = 1000 / frequency;

    (int origLeft, int origTop) = Console.GetCursorPosition();
    Console.CursorVisible = false;
    Console.CancelKeyPress += (s, e) =>
    {
        Console.CursorVisible = true;
        Console.SetCursorPosition(origLeft,  origTop + rows);
    };

    if (input)
    {
        Task.Run(() =>
        {
            while (true)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        frequency++;
                        msPerFrame = 1000 / frequency;
                        break;
                    case ConsoleKey.DownArrow:
                        frequency = frequency > 1 ? frequency - 1 : frequency;
                        msPerFrame = 1000 / frequency;
                        break;
                }
            }
        });
    }

    var rand = new Random();
    while (true)
    {
        string s = string.Join("\n", new byte[rows].Select(y => string.Concat(new byte[columns].Select(x => values[rand.Next(values.Length)]))));
        Console.WriteLine(s);
        Console.SetCursorPosition(origLeft, origTop);
        Task.Delay(msPerFrame).Wait();
    }
}, rowsOpt, columnsOpt, frequencyOpt, valuesOpt, inputOpt);

rootCommand.Invoke(args);