using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleGacha
{
    enum Rarity { ThreeStar = 3, FourStar = 4, FiveStar = 5 }

    class Character
    {
        public string Name;
        public Rarity Rarity;

        public int? PulledAtPity5;
        public int? PulledAtPity4;
        public Character(string name, Rarity rarity)
        {
            Name = name;
            Rarity = rarity;
        }
    }

    class OwnedCharacter
    {
        public Character Character;
        public int Copies;

        public OwnedCharacter(Character character)
        {
            Character = character;
            Copies = 1;
        }
    }

    class PullHistory
    {
        public Character Character;
        public int PullNumber;
        public int? Pity;

        public PullHistory(Character character, int pullNumber, int? pity)
        {
            Character = character;
            PullNumber = pullNumber;
            Pity = pity;
        }
    }

    class Banner
    {
        // How many pulls since last 5-star / 4-star
        public int Pity5 = 0;
        public int Pity4 = 0;

        public bool GuaranteedFeatured = false; // True if lost 50/50

        public const int HardPity5 = 80;      // guaranteed 5-star at this count
        public const int SoftPity5Start = 74;  // rate starts ramping up here
        public const int HardPity4 = 10;       // guaranteed 4-star (or better) every 10 pulls

        static readonly Character FeaturedFiveStar = new Character("Maria, the goddess of pain", Rarity.FiveStar);

        static readonly List<Character> StandardFiveStars = new()
        {
            new Character("Aaron, the potato God", Rarity.FiveStar),
            new Character("Natsuki Subaru, the superman complex", Rarity.FiveStar),
            new Character("SameSaturn, the larping one", Rarity.FiveStar),
            new Character("AbyssalDream, the silly one", Rarity.FiveStar),
        };

        static readonly List<Character> FourStars = new()
        {
            new Character("Berk the Stalwart", Rarity.FourStar),
            new Character("Lira Windrunner", Rarity.FourStar),
            new Character("Talon Ashblade", Rarity.FourStar),
            new Character("Mira Frostveil", Rarity.FourStar),
            new Character("Doran Stonefist", Rarity.FourStar),
            new Character("Kael Ironheart", Rarity.FourStar),
            new Character("Sylva Moonshade", Rarity.FourStar),
            new Character("Ronan Emberforge", Rarity.FourStar),
            new Character("Nyra Stormcaller", Rarity.FourStar),
            new Character("Garrick Thornhelm", Rarity.FourStar),
        };

        static readonly List<Character> ThreeStars = new()
        {
            new Character("Farmhand Joji", Rarity.ThreeStar),
            new Character("Squire Beb", Rarity.ThreeStar),
            new Character("Apprentice Coll", Rarity.ThreeStar),
            new Character("Wanderer Fen", Rarity.ThreeStar),
            new Character("Scout Rill", Rarity.ThreeStar),
            new Character("Peddler Oskar", Rarity.ThreeStar),
        };

        static readonly Random Rng = new Random();

        public Character Pull()
        {
            Pity5++;
            Pity4++;

            // Base 5-star chance
            double fiveStarChance = 0.006; // 0.6% // o.006

            // Soft pity: chance ramps up sharply after SoftPity5Start
            if (Pity5 >= SoftPity5Start)
            {
                int stepsIntoSoft = Pity5 - SoftPity5Start + 1;
                fiveStarChance = Math.Min(1.0, 0.006 + stepsIntoSoft * 0.15);
            }

            // Hard pity: guaranteed at 80
            if (Pity5 >= HardPity5)
                fiveStarChance = 1.0;

            bool got5 = Rng.NextDouble() < fiveStarChance;

            if (got5)
            {
                Character baseChar;

                if (GuaranteedFeatured)
                {
                    // We lost the previous 50/50, so give the featured character.
                    baseChar = FeaturedFiveStar;

                    // Consume the guarantee.
                    GuaranteedFeatured = false;
                }
                else
                {
                    // Flip a coin.
                    bool won5050 = Rng.Next(2) == 0;

                    if (won5050)
                    {
                        // Won the 50/50.
                        baseChar = FeaturedFiveStar;
                    }
                    else
                    {
                        // Lost the 50/50.
                        baseChar = StandardFiveStars[Rng.Next(StandardFiveStars.Count)];

                        // The next 5★ is now guaranteed to be featured.
                        GuaranteedFeatured = true;
                    }
                }

                Character c = new Character(baseChar.Name, baseChar.Rarity)
                {
                    PulledAtPity5 = Pity5
                };

                Pity5 = 0;
                Pity4 = 0;

                return c;
            }

            // 4-star roll (only checked if no 5-star)
            double fourStarChance = 0.051; // 5.1%
            bool guaranteed4 = Pity4 >= HardPity4;
            bool got4 = guaranteed4 || Rng.NextDouble() < fourStarChance;

            if (got4)
            {
                Character c = FourStars[Rng.Next(FourStars.Count)];

                c.PulledAtPity4 = Pity4;
                c.PulledAtPity5 = null;

                Pity4 = 0;

                return c;
            }

            return ThreeStars[Rng.Next(ThreeStars.Count)];
        }
    }

    class Program
    {
        static readonly Banner banner = new Banner();

        static readonly List<OwnedCharacter> collection = new();

        static readonly List<PullHistory> history = new();
        static int totalPulls = 0;

        static void Main()
        {
            Console.Title = "Console Gacha";

            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("1. Single Pull");
                Console.WriteLine("2. Pull x10");
                Console.WriteLine("3. Character Collection");
                Console.WriteLine("4. Pull History");
                Console.WriteLine("5. Exit");
                Console.Write("\nChoice: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        DoPull(1);
                        break;

                    case "2":
                        DoPull(10);
                        break;

                    case "3":
                        ShowCollection();
                        break;

                    case "4":
                        ShowHistory();
                        break;
                    case "5":
                        running = false;
                        break;
                }
            }

            Console.WriteLine("Thanks for playing!");
        }

        static void DoPull(int amount)
        {
            var results = new List<Character>();

            for (int i = 0; i < amount; i++)
                results.Add(banner.Pull());

            Console.Clear();
            ShowLoadingBar(results);

            foreach (var pulled in results)
            {

                AddToHistory(pulled);
                // Skip 3-star weapons
                if (pulled.Rarity == Rarity.ThreeStar)
                    continue;

                var owned = collection.FirstOrDefault(c => c.Character.Name == pulled.Name);

                if (owned == null)
                {
                    collection.Add(new OwnedCharacter(pulled));
                }
                else if (owned.Copies < 7)
                {
                    owned.Copies++;
                }
            }

            if (amount == 1)
            {
                Console.Clear();
                Console.WriteLine("Single Pull");
                Console.WriteLine();

                PrintCharacter(results[0]);

                Console.WriteLine();
                Console.Write("Press Enter to continue...");
                Console.ReadLine();
            }
            else
            {
                ShowPullsOneByOne(results);
            }

            Console.Clear();

            ShowSummary(results, amount);

            Console.WriteLine();
            Console.WriteLine($"Pity to next 5-star: {banner.Pity5}/{Banner.HardPity5}");
            Console.WriteLine($"Pity to next 4-star: {banner.Pity4}/{Banner.HardPity4}");

            Console.WriteLine();
            Console.Write("Press Enter to return...");
            Console.ReadLine();
        }

        static void AddToHistory(Character pulled)
        {
            totalPulls++;

            int? pity = null;

            if (pulled.Rarity == Rarity.FiveStar)
                pity = pulled.PulledAtPity5;
            else if (pulled.Rarity == Rarity.FourStar)
                pity = pulled.PulledAtPity4;

            history.Add(new PullHistory(pulled, totalPulls, pity));

            // Keep only the newest 400 pulls
            if (history.Count > 400)
                history.RemoveAt(0);
        }

        static void ShowCollection()
        {
            Console.Clear();

            Console.WriteLine("=== Character Collection ===");
            Console.WriteLine();

            if (collection.Count == 0)
            {
                Console.WriteLine("No characters pulled yet.");
            }
            else
            {
                foreach (var c in collection.OrderByDescending(c => c.Character.Rarity))
                {
                    PrintCollectionCharacter(c);
                }
            }

            Console.WriteLine();
            Console.Write("Press Enter to return...");
            Console.ReadLine();
        }

        static void ShowHistory()
        {
            int page = 0;

            while (true)
            {
                Console.Clear();

                int totalPages = Math.Max(1, (history.Count + 9) / 10);

                Console.WriteLine("===== Pull History =====");
                Console.WriteLine($"Page {page + 1}/{totalPages}");
                Console.WriteLine();

                var pageEntries = history
                    .AsEnumerable()
                    .Reverse()
                    .Skip(page * 10)
                    .Take(10);

                foreach (var h in pageEntries)
                {
                    string stars = new string('*', (int)h.Character.Rarity);

                    string pity = h.Pity.HasValue
                        ? $"Pity {h.Pity.Value}"
                        : "";

                    Console.Write($"#{h.PullNumber,-4} ");

                    Console.ForegroundColor = GetRarityColor(h.Character.Rarity);
                    Console.Write($"[{stars}] ");
                    Console.ResetColor();

                    Console.Write($"{h.Character.Name,-35}");

                    if (h.Pity.HasValue)
                        Console.Write($" Pity {h.Pity.Value}");

                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine("[A] Previous   [D] Next   [Q] Back");

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.A:
                        if (page > 0)
                            page--;
                        break;

                    case ConsoleKey.D:
                        if (page < totalPages - 1)
                            page++;
                        break;

                    case ConsoleKey.Q:
                        return;
                }
            }
        }

        static void ShowLoadingBar(List<Character> results)
        {
            Rarity highest = results.Max(c => c.Rarity);

            Console.WriteLine("Summoning...");

            for (int i = 0; i <= 100; i += 2)
            {
                // Default starts white
                Console.ForegroundColor = ConsoleColor.White;

                if (highest == Rarity.ThreeStar)
                {
                    // White -> Blue
                    if (i >= 40)
                        Console.ForegroundColor = ConsoleColor.DarkBlue;

                    if (i >= 70)
                        Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (highest == Rarity.FourStar)
                {
                    // White -> Blue -> Purple
                    if (i >= 25)
                        Console.ForegroundColor = ConsoleColor.DarkBlue;

                    if (i >= 50)
                        Console.ForegroundColor = ConsoleColor.Blue;

                    if (i >= 70)
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;

                    if (i >= 90)
                        Console.ForegroundColor = ConsoleColor.Magenta;
                }
                else // Five Star
                {
                    // White -> Blue -> Purple -> Gold
                    if (i >= 15)
                        Console.ForegroundColor = ConsoleColor.DarkBlue;

                    if (i >= 35)
                        Console.ForegroundColor = ConsoleColor.Blue;

                    if (i >= 55)
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;

                    if (i >= 70)
                        Console.ForegroundColor = ConsoleColor.Magenta;

                    if (i >= 85)
                        Console.ForegroundColor = ConsoleColor.DarkYellow;

                    if (i >= 95)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                }

                DrawBar(i);
                Thread.Sleep(15);
            }

            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }

        static void DrawBar(int percent)
        {
            int width = 40;
            int filled = width * percent / 100;
            string bar = new string('#', filled) + new string('-', width - filled);
            Console.Write($"\r[{bar}] {percent,3}%");
        }

        static ConsoleColor GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.FiveStar => ConsoleColor.Yellow,
                Rarity.FourStar => ConsoleColor.Magenta,
                _ => ConsoleColor.Cyan
            };
        }

        static void ShowPullsOneByOne(List<Character> results)
        {
            for (int i = 0; i < results.Count; i++)
            {
                Console.Clear();
                Console.WriteLine($"Pull {i + 1} of {results.Count}");
                Console.WriteLine();
                PrintCharacter(results[i]);
                Console.WriteLine();
                Console.Write(i < results.Count - 1
                    ? "Press Enter for next pull..."
                    : "Press Enter to see summary...");
                Console.ReadLine();
            }
        }

        static void PrintCharacter(Character c)
        {
            string stars = new string('*', (int)c.Rarity);
            ConsoleColor color = c.Rarity switch
            {
                Rarity.FiveStar => ConsoleColor.Yellow,
                Rarity.FourStar => ConsoleColor.Magenta,
                _ => ConsoleColor.Gray
            };
            Console.ForegroundColor = color;
            string pityText = "";

            if (c.Rarity == Rarity.FiveStar)
                pityText = $" (Pity {c.PulledAtPity5})";
            else if (c.Rarity == Rarity.FourStar)
                pityText = $" (Pity {c.PulledAtPity4})";

            Console.WriteLine($"  [{stars}] {c.Name}{pityText}");
            Console.ResetColor();
        }

        static void PrintCollectionCharacter(OwnedCharacter owned)
        {
            string stars = new string('*', (int)owned.Character.Rarity);

            ConsoleColor color = owned.Character.Rarity switch
            {
                Rarity.FiveStar => ConsoleColor.Yellow,
                Rarity.FourStar => ConsoleColor.Magenta,
                _ => ConsoleColor.Gray
            };

            Console.ForegroundColor = color;
            Console.WriteLine($"  [{stars}] {owned.Character.Name}   C{owned.Copies - 1}");
            Console.ResetColor();
        }

        static void ShowSummary(List<Character> results, int amount)
        {
            Console.WriteLine($"=== {amount}-Pull Summary ===");
            Console.WriteLine();
            foreach (var c in results.OrderByDescending(c => c.Rarity))
                PrintCharacter(c);

            int fives = results.Count(c => c.Rarity == Rarity.FiveStar);
            int fours = results.Count(c => c.Rarity == Rarity.FourStar);
            int threes = results.Count(c => c.Rarity == Rarity.ThreeStar);
            Console.WriteLine();
            Console.WriteLine($"5-star: {fives}   4-star: {fours}   3-star: {threes}");
        }
    }
}