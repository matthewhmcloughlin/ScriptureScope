using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ScriptureTelescope
{
    public class BibleTelescope
    {
        public string Path { get; set; }
        public XmlDocument BibleXml { get; set; } = new XmlDocument();
        public Dictionary<string, int> WordFrequencies { get; set; } = new Dictionary<string, int>();
        public HashSet<string> UniqueWords { get; set; } = new HashSet<string>();
        public HashSet<string> HyphenatedWords { get; set; } = new HashSet<string>();
        public int TotalWordCount { get; set; }
        public int TotalVerseCount { get; set; }
        public BibleTelescope(string path)
        {
            Path = path;
            BibleXml.Load(path);
            LoadWordFrequencies();
        }

        public void ProcessCommands()
        {
            while (true)
            {
                Console.WriteLine("wf = word frequency explorer");
                Console.WriteLine("wl = word location explorer");
                Console.WriteLine("pf = print word frequencies");
                Console.WriteLine("all = print all verses");

                Console.WriteLine();
                Console.Write("Enter command: ");
                var command = Console.ReadLine().ToLowerInvariant().Trim();
                if (string.IsNullOrWhiteSpace(command)) return;

                switch (command)
                {
                    case "wf":
                        WordFrequencyExplorer();
                        break;
                    case "wl":
                        WordLocationExplorer();
                        break;
                    case "pf":
                        PrintWordFrequencies();
                        break;
                    case "all":
                        ReadAndPrint();
                        break;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }


        }
        private void ReadAndPrint()
        {
            foreach (XmlNode book in BibleXml.DocumentElement.ChildNodes)
            {
                var bookName = book.Attributes["n"].InnerText;

                foreach (XmlNode chapter in book.ChildNodes)
                {
                    var chapterName = chapter.Attributes["n"].InnerText;

                    foreach (XmlNode verse in chapter.ChildNodes)
                    {
                        var verseNumber = verse.Attributes["n"].InnerText;
                        PrintBibleVerse(bookName, chapterName, verseNumber, verse.InnerText);
                    }
                }
            }
        }
        private IEnumerable<WordLocation> WordLocations(string wordToSearchFor)
        {
            string tempWord = wordToSearchFor.ToUpperInvariant().Trim();

            foreach (XmlNode book in BibleXml.DocumentElement.ChildNodes)
            {
                var bookName = book.Attributes["n"].InnerText;

                foreach (XmlNode chapter in book.ChildNodes)
                {
                    var chapterName = chapter.Attributes["n"].InnerText;
                    int chapterNumber = int.Parse(chapterName);

                    foreach (XmlNode verse in chapter.ChildNodes)
                    {
                        var verseNumberText = verse.Attributes["n"].InnerText;
                        int verseNumber = int.Parse(verseNumberText);

                        string rawText = verse.InnerText;
                        string text = verse.InnerText.ToUpperInvariant();
                        var chars = new List<char>();

                        for (int i = 0; i < text.Length; i++)
                        {
                            var c = text[i];
                            if (!char.IsLetter(c) && c != ' ' && c != '-')
                            {
                                continue;
                            }

                            // allow hyphenated words
                            if (i > 0 && i < text.Length - 1 && c == '-')
                            {
                                if (!char.IsLetter(text[i - 1]) || !char.IsLetter(text[i + 1]))
                                {
                                    continue;
                                }
                            }

                            chars.Add(text[i]);
                        }

                        text = string.Concat(chars).ToUpperInvariant();

                        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var word in words)
                        {
                            var tempWord2 = word.Trim().TrimStart('-').TrimEnd('-');
                            if (string.IsNullOrWhiteSpace(tempWord2)) continue;

                            if (tempWord2 == wordToSearchFor)
                            {
                                yield return new WordLocation { BookName = bookName, Chapter = chapterNumber, VerseNumber = verseNumber, Text = rawText };
                                
                                // only print verse once
                                break;
                            }
                        }
                    }
                }
            }

            yield break;
        }
        private void LoadWordFrequencies()
        {
            foreach (XmlNode book in BibleXml.DocumentElement.ChildNodes)
            {
                var bookName = book.Attributes["n"].InnerText;

                foreach (XmlNode chapter in book.ChildNodes)
                {
                    var chapterName = chapter.Attributes["n"].InnerText;

                    foreach (XmlNode verse in chapter.ChildNodes)
                    {
                        TotalVerseCount++;
                        var verseNumber = verse.Attributes["n"].InnerText;
                        string text = verse.InnerText.ToUpperInvariant();
                        var chars = new List<char>();

                        for (int i = 0; i < text.Length; i++)
                        {
                            var c = text[i];
                            if (!char.IsLetter(c) && c != ' ' && c != '-')
                            {
                                continue;
                            }

                            // allow hyphenated words
                            if (i > 0 && i < text.Length - 1 && c == '-')
                            {
                                if (!char.IsLetter(text[i-1]) || !char.IsLetter(text[i+1]))
                                {
                                    continue;
                                }
                            }

                            chars.Add(text[i]);
                        }

                        text = string.Concat(chars).ToUpperInvariant();

                        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        
                        foreach (var word in words)
                        {
                            var tempWord = word.Trim().TrimStart('-').TrimEnd('-');
                            if (string.IsNullOrWhiteSpace(tempWord)) continue;

                            TotalWordCount++;

                            if (WordFrequencies.ContainsKey(tempWord))
                            {
                                WordFrequencies[tempWord]++;
                            }
                            else
                            {
                                WordFrequencies[tempWord] = 1;
                            }

                            UniqueWords.Add(tempWord);
                        }
                    }
                }
            }
        }
        private void PrintWordFrequencies()
        {
            foreach (var word in WordFrequencies.Keys.OrderBy(c => c))
            {
                Console.WriteLine($"{word}: {WordFrequencies[word]:n0}");
                Task.Delay(50).Wait();
            }
        }
        private void WordFrequencyExplorer()
        {
            Console.WriteLine($"Unique word count: {UniqueWords.Count:n0}");
            Console.WriteLine($"Total word count: {TotalWordCount:n0}");
            Console.WriteLine($"Total verse count: {TotalVerseCount:n0}");
            Console.WriteLine();
            Console.WriteLine();

            while (true)
            {
                Console.Write("Enter word: ");
                var line = Console.ReadLine();
                var key = line.ToUpperInvariant().Trim();
                if (string.IsNullOrWhiteSpace(key)) return;

                if (!WordFrequencies.ContainsKey(key))
                {
                    Console.WriteLine($"{key} was not found in this bible.");
                    continue;
                }

                int count = WordFrequencies[key];
                Console.WriteLine($"{key} was found ({count:n0}) times in this bible.");
                Console.WriteLine();
            }
        }
        private void WordLocationExplorer()
        {
            while (true)
            {
                Console.Write("Enter word: ");
                var line = Console.ReadLine();
                var key = line.ToUpperInvariant().Trim();
                if (string.IsNullOrWhiteSpace(key)) return;

                if (!WordFrequencies.ContainsKey(key))
                {
                    Console.WriteLine($"{key} was not found in this bible.");
                    continue;
                }

                int count = WordFrequencies[key];
                Console.WriteLine($"{key} was found ({count:n0}) times in this bible.");
                Console.WriteLine();

                foreach (var location in WordLocations(key))
                {
                    Console.WriteLine(location.ToString());
                    Console.WriteLine(location.Text);
                    Console.WriteLine();
                }
            }
        }
        private static void PrintBibleVerse(string bookName, string chapterName, string verseNumber, string verse)
        {
            Console.WriteLine($"{bookName} {chapterName}:{verseNumber}");
            Console.WriteLine(verse);
            Console.WriteLine();
            Task.Delay(1000).Wait();
        }
    }

    public class WordLocation
    {
        public string BookName { get; set; }
        public int Chapter { get; set; }
        public int VerseNumber { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{BookName} {Chapter}:{VerseNumber}";
        }
    }
}
