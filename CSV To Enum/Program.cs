using System.Drawing;
using System.Text;
using Yannick.Extensions.HashSetExtensions;
using Console = Yannick.UI.Console;

var seperator = new[] { ',', ';', '|', ':', '\t', ' ' };
var options = new[] { "C#" };
var csvFilePath = new List<string>();
string lang;
var keysH = new HashSet<string>();
var keys = new List<string>();
var completeKeys = new List<HashSet<string>>();
var invalidCsvPath = new List<string>();
bool? skipByNotEqual = null;
string attributeName;

Console.UseAnimation = Console.Animation.Animation1;

Console.SetColor(ConsoleColor.White, ConsoleColor.Black);
Console.Clear();
Console.Title = "CSV Converter";
Console.WriteLineCenter("Bitte die Sprache auswählen");
lang = options[Console.Select(options)];
while (true)
{
    Console.Clear();
    Console.Write("Sprache: ");
    Console.WriteLine(lang, ConsoleColor.Cyan);
    Console.WriteLineCenter("Bitte nun CSV Dateipfade angeben");
    Console.WriteLine();
    Console.WriteLine();
    
    Console.Write("Dateipfad: ");
    var path = Console.ReadLine(ConsoleColor.Cyan);

    if (Directory.Exists(path))
    {
        Console.ClearLine(-1, true);
        Console.WriteLine("Dein Dateipfad ist ein Verzeichnis");
        Console.WriteLine("bist du dir sicher?\n\n\n");
        if (Console.Select("Ja", "Nein") == 1)
            continue;
        
        Console.Clear();
        Console.Write("Sprache: ");
        Console.WriteLine(lang, ConsoleColor.Cyan);
        Console.WriteLineCenter("Bitte nun CSV Dateipfade angeben");
        Console.WriteLine();
        Console.WriteLine();
        var o = csvFilePath.Count;
        foreach(var f in Directory.GetFiles(path, "*.csv"))
            csvFilePath.Add(f);
        
        Console.Write("Es wurden: ");
        Console.Write(""+(csvFilePath.Count - o), ConsoleColor.Cyan);
        Console.WriteLine(" Dateipfade hinzugefügt");
    }
    else if (File.Exists(path))
    {
        csvFilePath.Add(path);
        Console.ClearLine(-1, true);
        Console.Write("Dateipfad: ");
        Console.WriteLine("Gültig", ConsoleColor.Green);
        Thread.Sleep(1500);
        break;
    }
    else
    {
        Console.ClearLine(-1, true);
        Console.Write("Dateipfad: ");
        Console.WriteLine("Ungültig", ConsoleColor.Red);
        Thread.Sleep(1500);
    }
    
    Console.WriteLine("\nSoll noch ein Pfad hinzugefügt werden ?");
    if (Console.Select("Ja", "Nein") == 1)
        break;
}

string targetPath;
do
{
    Console.Clear();
    Console.WriteLineCenter("Bitte nun den Ziel Pfad angeben\n\n\n\n");
    Console.Write("Path: ");
    targetPath = Console.ReadLine(ConsoleColor.Cyan)!;
    if (targetPath.Length > 0 && targetPath[0] == '?')
        targetPath = new string(targetPath.Skip(1).ToArray());
    if (File.Exists(targetPath))
    {
        Console.ClearLine(-1);
        Console.Write("Path: ");
        Console.WriteLine("Vorhanden", ConsoleColor.Yellow);
        Thread.Sleep(750);
        Console.WriteLineCenter("Soll die Datei gelöscht werden ?\n\n\n");
        if (Console.Select("Ja", "Nein") == 1)
        {
            continue;
        }
        else
        {
            try
            {
                File.Delete(targetPath);
            }
            catch (Exception e)
            {
                Console.ClearLine(true, -1, -2, -3, -4);
                Console.WriteLine("Die Datei Konnte nicht gelöscht werden");
                Console.WriteLine(e.Message, ConsoleColor.Red);
                Thread.Sleep(1500);
                continue;
            }
        }
        Thread.Sleep(1500);
        break;
    }
    else
    {
        Console.ClearLine(-1);
        Console.Write("Path: ");
        Console.WriteLine("Gültig", ConsoleColor.Green);
        Thread.Sleep(1500);
        break;
    }
} while (true);

Console.Clear();
Console.Write("Analysiere nun ");
Console.Write(options.Length+"", ConsoleColor.Cyan);
Console.WriteLine(" Items");
Console.Write("Schlüssel Sammlung: ");
Console.ActiveAnimation = true;

using var targetFs = new StreamWriter(new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None));

foreach (var path in csvFilePath)
{
    using var fs = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));

    while (fs.ReadLine() is { } line)
    {
        if (line.Length == 0)
            continue;
        
        foreach (var delimiter in seperator)
            if (line.Contains(delimiter))
            {
                var rs = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                keysH.AddRange(rs);
                completeKeys.Add(new HashSet<string>(rs));
                goto exit;
            }
        
        invalidCsvPath.Add(path);
        exit:
        break;
    }
}

Console.ActiveAnimation = false;

Console.Clear();
Console.WriteLineCenter("Stage 1 Ist Complete");
Thread.Sleep(500);
Console.ClearLine(-1, true);
Console.WriteFormat("Stage 1 Ist Complete", 
    waitBetweenChars:TimeSpan.FromMilliseconds(25),
    graphicMode:Console.AnsiGraphicMode.STRIKETHROUGH,
    lineStyle:Console.LineStyle.NEW_LINE);
Console.Write("Es wurden ");
Console.Write(keysH.Count, ConsoleColor.Cyan);
Console.WriteLine(" Schlüssel gefunden");
var isKeyLenghtEqual = completeKeys.All(hashSet => hashSet.SetEquals(completeKeys.First()));
if (isKeyLenghtEqual)
{
    Console.Write("Es wurden die ");
    Console.Write("gleiche");
    Console.Write(" ");
    Console.Write("Anzahl", ConsoleColor.Cyan);
    Console.Write(" und Die Selben ");
    Console.Write("Schlüssel", ConsoleColor.Cyan);
    Console.WriteLine(" in allen CSV gefunden");
}
else
{
    Console.WriteLine("Es wurden mehr Schlüssel gefunden\n\n\n");
    Console.WriteLineCenter("Was Soll mit den ungleichen Schlüsseln passieren");
    var optionsForOption = new [] { "Überspringen", "Empty", "Null" };
    var option = Console.Select(optionsForOption);
    skipByNotEqual = option switch
    {
        0 => true,
        1 => false,
        _ => null
    };
    Console.ClearLine(-1, true);
    Console.Write("Die ungleichen Schlüssel werden mit den Wert ");
    Console.Write(optionsForOption[option], ConsoleColor.Cyan);
    Console.WriteLine(" belegt");
}
Console.WriteLine("Bitte gebe nun dein Attribut Namen ein");
Console.Write("Dein Attribut Name: ");
attributeName = Console.ReadLine(ConsoleColor.Magenta);
Console.Clear();
Console.WriteLineCenter("Stage 2 Ist Complete");
Thread.Sleep(500);
Console.ClearLine(-1, true);
Console.WriteFormat("Stage 2 Ist Complete", 
    waitBetweenChars:TimeSpan.FromMilliseconds(25),
    graphicMode:Console.AnsiGraphicMode.STRIKETHROUGH,
    lineStyle:Console.LineStyle.NEW_LINE);
Console.WriteLine("Füge nun die CSV Dateien zusammen ");
keys = new List<string>(keysH);
var pI = -1;
targetFs.WriteLine("enum C {");
foreach (var path in csvFilePath)
{
    pI++;
    var extra = csvFilePath.Count.ToString().Length + pI.ToString().Length + 5;
    Console.WriteCenter($"Beginne nun mit {Enumerable.Repeat(" ", extra)}");
    Console.CursorLeft -= extra;
    Console.Write(pI, ConsoleColor.Cyan);
    Console.Write(" von ");
    Console.Write(csvFilePath.Count, ConsoleColor.Cyan);
    Console.ActiveAnimation = true;
    
    using var fs = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));

    var lineCount = -1;
    string? line;
    var kMatch = new Dictionary<int, bool?>();
    string[]? k = null;
    while ((line = fs.ReadLine()) != null)
    {
        lineCount++;
        
        if (line.Length == 0)
            continue;

        if (lineCount == 0)
        {
            foreach (var delimiter in seperator)
                if (line.Contains(delimiter))
                {
                    k = line.Split(delimiter,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    break;
                }

            for (var i = 0; i < keys.Count; i++)
            {
                var found = false;
                for (var j = 0; j < k!.Length; j++)
                {
                    if (keys[i] == k[j])
                    {
                        kMatch[j] = i == 0 ? (bool?)null : true;
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    kMatch[-i - 1] = false;
            }
        }
        else
        {
            foreach (var delimiter in seperator)
                if (line.Contains(delimiter))
                {
                    k = line.Split(delimiter,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    break;
                }
            
            targetFs.Write($"[{attributeName}(");

            foreach (var (index, isMatch) in kMatch)
            {
                if (index < 0 || (k != null && index >= k.Length))
                    continue;

                if (isMatch.HasValue)
                {
                    targetFs.Write(isMatch.Value
                        ? $"""
                           "{(k![index])}"
                           """
                        : DefaultVal());

                    if (index + 1 < kMatch.Count)
                        targetFs.Write(", ");
                }
                else if (index + 1 == kMatch.Count)
                    targetFs.Write("\"\"");
            }
            
            targetFs.WriteLine(")]");
            targetFs.WriteLine($"{ConvertToLegitLang(k![kMatch.First(e => e.Value == null).Key])}, ");
        }
    }
    
    Console.ActiveAnimation = false;
    var newPosition = Console.CursorLeft - 16 - extra;
    Console.CursorLeft = Math.Max(0, newPosition);
    
    targetFs.WriteLine("}");
    break;
    
    string DefaultVal()
    {
        return skipByNotEqual switch
        {
            true => "",
            false => "string.Empty",
            _ => "null"
        };
    }
}

Console.Write("Umwandlung ist ");
Console.WriteLine("Abgeschlossen", ConsoleColor.Green);
await targetFs.DisposeAsync();
Console.Read();
return;


static string ConvertToLegitLang(string val)
{
    if (string.IsNullOrEmpty(val))
        return val;
    
    var numberMap = new Dictionary<char, string>
    {
        { '0', "Zero" },
        { '1', "One" },
        { '2', "Two" },
        { '3', "Three" },
        { '4', "Four" },
        { '5', "Five" },
        { '6', "Six" },
        { '7', "Seven" },
        { '8', "Eight" },
        { '9', "Nine" }
    };

    var result = new StringBuilder();
    
    var isFirstChar = true;
    foreach (var c in val)
    {
        if (isFirstChar && char.IsDigit(c))
        {
            result.Append(numberMap[c]);
            isFirstChar = false;
            continue;
        }
        
        if (c == ' ')
            result.Append('_');
        else if (!char.IsLetterOrDigit(c))
            result.Append('_');
        else
            result.Append(c);
        
        isFirstChar = false;
    }
    
    if (result.Length > 0)
        result[0] = char.ToUpper(result[0]);

    return result.ToString();
}









































