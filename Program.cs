using System.Text.Json;
using System.Text.RegularExpressions;

class Program
{
    class Section
    {
        public string Title { get; set; }
        public string? Content { get; set; }
        public Dictionary<string, Section> Children { get; set; } = new();
    }

    static void Main(string[] args)
    {
        string dataFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data-files"));
        string inputPath = Path.Combine(dataFolder, "content_eng.txt");
        string outputPath = Path.Combine(dataFolder, "output.json");

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("Файл не найден: " + inputPath);
            return;
        }

        var lines = File.ReadAllLines(inputPath);
        var root = new Dictionary<string, Section>();
        var sectionStack = new Stack<(string prefix, Section section)>();

        Regex sectionRegex = new Regex(@"^(\d+(\.\d+)*)\s+(.*)");
        Section? lastSection = null;

        foreach (var line in lines)
        {
            var match = sectionRegex.Match(line);
            if (match.Success)
            {
                string prefix = match.Groups[1].Value;
                string title = match.Groups[3].Value;
                string fullTitle = prefix + " " + title;

                var newSection = new Section { Title = fullTitle };

                while (sectionStack.Count > 0 && !prefix.StartsWith(sectionStack.Peek().prefix + "."))
                {
                    sectionStack.Pop();
                }

                if (sectionStack.Count == 0)
                {
                    root[fullTitle] = newSection;
                }
                else
                {
                    sectionStack.Peek().section.Children[fullTitle] = newSection;
                }

                sectionStack.Push((prefix, newSection));
                lastSection = newSection;
            }
            else if (!string.IsNullOrWhiteSpace(line) && lastSection != null)
            {
                lastSection.Content = line.Trim();
                lastSection = null;
            }
        }

        var jsonData = ConvertToPlainObject(root);
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(outputPath, JsonSerializer.Serialize(jsonData, options));

        Console.WriteLine("Готово! JSON сохранён в:");
        Console.WriteLine(outputPath);
    }

    static object ConvertToPlainObject(Dictionary<string, Section> sections)
    {
        var result = new Dictionary<string, object>();
        foreach (var kvp in sections)
        {
            if (kvp.Value.Children.Count > 0)
            {
                result[kvp.Key] = ConvertToPlainObject(kvp.Value.Children);
            }
            else
            {
                result[kvp.Key] = kvp.Value.Content ?? "";
            }
        }
        return result;
    }
}
