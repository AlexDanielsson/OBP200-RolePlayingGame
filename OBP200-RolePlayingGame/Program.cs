using System.Text;

namespace OBP200_RolePlayingGame;


class Program
{
    static Player Player;
    
    // Rum: [type, label]
    // types: battle, treasure, shop, rest, boss
    static List<string[]> Rooms = new List<string[]>();

    // Fiendemallar: [type, name, HP, ATK, DEF, XPReward, GoldReward]
    static List<string[]> EnemyTemplates = new List<string[]>();

    // Status för kartan
    static int CurrentRoomIndex = 0;

    // Random
    static Random Rng = new Random();

    // ======= Main =======

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }

    // ======= Meny & Init =======

    static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    static void StartNewGame()
    {
        string playerClass;
        int maxHp;
        int currentHp;
        int attackStat;
        int defenceStat;
        int potionsInventory;
        int goldInventory;
        
        
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Namnlös";

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var playerChoice = (Console.ReadLine() ?? "").Trim();
        
        playerClass = "Warrior"; // Warrior: Låg damage, hög defence
        maxHp = 40; 
        currentHp = 40; 
        attackStat = 7;
        defenceStat = 5; 
        potionsInventory = 2; 
        goldInventory = 15;
        
        switch (playerChoice)
        {
            case "2": // Mage: Hög damage, låg defence
                playerClass = "Mage";
                maxHp = 28;
                currentHp = 28;
                attackStat = 10;
                defenceStat = 2;
                potionsInventory = 2;
                goldInventory = 15;
                break;
            case "3": // Rogue: krit-chans
                playerClass = "Rogue";
                maxHp = 32;
                currentHp = 32;
                attackStat = 8;
                defenceStat = 3;
                potionsInventory = 3;
                goldInventory = 20;
                break;
        }
        
        Player = new Player
        
        {
            PlayerName = name,
            PlayerClass = playerClass,
            CurrentHP = currentHp,
            MaxHP = maxHp,
            AttackStat = attackStat,
            DefenceStat = defenceStat,
            PlayerExp = 0,
            PlayerLevel = 1,
            GoldInventory = goldInventory,
            PotionsInventory = potionsInventory,
            Inventory = new List<string>
            {
                "Wooden Sword",
                "Cloth Armor"
            }
        };

        // Initiera karta (linjärt äventyr)
        Rooms.Clear();
        Rooms.Add(new[] { "battle", "Skogsstig" });
        Rooms.Add(new[] { "treasure", "Gammal kista" });
        Rooms.Add(new[] { "shop", "Vandrande köpman" });
        Rooms.Add(new[] { "battle", "Grottans mynning" });
        Rooms.Add(new[] { "rest", "Lägereld" });
        Rooms.Add(new[] { "battle", "Grottans djup" });
        Rooms.Add(new[] { "boss", "Urdraken" });

        CurrentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {Player.PlayerName} the {Player.PlayerClass}!");
        ShowStatus();
    }

    static void RunGameLoop()
    {
        while (true)
        {
            var room = Rooms[CurrentRoomIndex];
            Console.WriteLine($"--- Rum {CurrentRoomIndex + 1}/{Rooms.Count}: {room[1]} ({room[0]}) ---");

            bool continueAdventure = EnterRoom(room[0]);
            
            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }
            
            if (!continueAdventure)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            CurrentRoomIndex++;
            
            if (CurrentRoomIndex >= Rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }
            
            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }

    // ======= Rumshantering =======

    static bool EnterRoom(string type)
    {
        switch ((type ?? "battle").Trim())
        {
            case "battle":
                return DoBattle(isBoss: false);
            case "boss":
                return DoBattle(isBoss: true);
            case "treasure":
                return DoTreasure();
            case "shop":
                return DoShop();
            case "rest":
                return DoRest();
            default:
                Console.WriteLine("Du vandrar vidare...");
                return true;
        }
    }

    // ======= Strid =======

    static bool DoBattle(bool isBoss)
    {
        var enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy[1]} dyker upp! (HP {enemy[2]}, ATK {enemy[3]}, DEF {enemy[4]})");

        int enemyHp = ParseInt(enemy[2], 10);
        int enemyAtk = ParseInt(enemy[3], 3);
        int enemyDef = ParseInt(enemy[4], 0);

        while (enemyHp > 0 && !IsPlayerDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy[1]} HP={enemyHp}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
            if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
            Console.Write("Val: ");

            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemyDef);
                enemyHp -= damage;
                Console.WriteLine($"Du slog {enemy[1]} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemyDef, isBoss);
                enemyHp -= special;
                Console.WriteLine($"Special! {enemy[1]} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
                {
                    Console.WriteLine("Du flydde!");
                    return true; // fortsätt äventyr
                }
                else
                {
                    Console.WriteLine("Misslyckad flykt!");
                }
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (enemyHp <= 0) break;

            // Fiendens tur
            int enemyDamage = CalculateEnemyDamage(enemyAtk);
            ApplyDamageToPlayer(enemyDamage);
            Console.WriteLine($"{enemy[1]} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false; // avsluta äventyr
        }

        // Vinstrapporter, XP, guld, loot
        int xpReward = ParseInt(enemy[5], 5);
        int goldReward = ParseInt(enemy[6], 3);

        AddPlayerXp(xpReward);
        AddPlayerGold(goldReward);

        Console.WriteLine($"Seger! +{xpReward} XP, +{goldReward} guld.");
        MaybeDropLoot(enemy[1]);

        return true;
    }

    static string[] GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            // Boss-mall
            return new[] { "boss", "Urdraken", "55", "9", "4", "30", "50" };
        }
        else
        {
            // Slumpa bland templates
            var template = EnemyTemplates[Rng.Next(EnemyTemplates.Count)];
            
            // Slmumpmässig justering av stats
            int hp = ParseInt(template[2], 10) + Rng.Next(-1, 3);
            int atk = ParseInt(template[3], 3) + Rng.Next(0, 2);
            int def = ParseInt(template[4], 0) + Rng.Next(0, 2);
            int xp = ParseInt(template[5], 4) + Rng.Next(0, 3);
            int gold = ParseInt(template[6], 2) + Rng.Next(0, 3);
            return new[] { template[0], template[1], hp.ToString(), atk.ToString(), def.ToString(), xp.ToString(), gold.ToString() };
        }
    }

    static void InitEnemyTemplates()
    {
        EnemyTemplates.Clear();
        EnemyTemplates.Add(new[] { "beast", "Vildsvin", "18", "4", "1", "6", "4" });
        EnemyTemplates.Add(new[] { "undead", "Skelett", "20", "5", "2", "7", "5" });
        EnemyTemplates.Add(new[] { "bandit", "Bandit", "16", "6", "1", "8", "6" });
        EnemyTemplates.Add(new[] { "slime", "Geléslem", "14", "3", "0", "5", "3" });
    }

    static int CalculatePlayerDamage(int enemyDef)
    {
        string playerClass = Player.PlayerClass;

        // Beräkna grundskada
        int baseDmg = Math.Max(1, Player.AttackStat - (enemyDef / 2));
        int roll = Rng.Next(0, 3); // liten variation

        switch (playerClass.Trim())
        {
            case "Warrior":
                baseDmg += 1; // warrior buff
                break;
            case "Mage":
                baseDmg += 2; // mage buff
                break;
            case "Rogue":
                baseDmg += (Rng.NextDouble() < 0.2) ? 4 : 0; // rogue crit-chans
                break;
            default:
                baseDmg += 0;
                break;
        }

        return Math.Max(1, baseDmg + roll);
    }

    static int UseClassSpecial(int enemyDef, bool vsBoss)
    {
        string playerClass = Player.PlayerClass ?? "Warrior";
        int specialDmg = 0;

        // Hantering av specialförmågor
        if (playerClass == "Warrior")
        {
            // Heavy Strike: hög skada men självskada
            Console.WriteLine("Warrior använder Heavy Strike!");
            specialDmg = Math.Max(2, Player.AttackStat + 3 - enemyDef);
            ApplyDamageToPlayer(2); // självskada
        }
        else if (playerClass == "Mage")
        {
            // Fireball: stor skada, kostar guld
            if (Player.GoldInventory >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
                Player.GoldInventory -= 3;
                specialDmg = Math.Max(3, Player.AttackStat + 5 - (enemyDef / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                return 0;
            }
        }
        else if (playerClass == "Rogue")
        {
            // Backstab: chans att ignorera försvar, hög risk/hög belöning
            if (Rng.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                specialDmg = Math.Max(4, Player.AttackStat + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDmg = 1;
            }
        }
        else
        {
            specialDmg = 0;
        }

        // Dämpa skada mot bossen
        if (vsBoss)
        {
            specialDmg = (int)Math.Round(specialDmg * 0.8);
        }

        return Math.Max(0, specialDmg);
    }

    static int CalculateEnemyDamage(int enemyAtk)
    {
        int def = Player.DefenceStat;
        int roll = Rng.Next(0, 3);

        int dmg = Math.Max(1, enemyAtk - (def / 2)) + roll;

        // Liten chans till "glancing blow" (minskad skada)
        if (Rng.NextDouble() < 0.1)
        {
            dmg = Math.Max(1, dmg - 2);
        }

        return dmg;
    }

    static void ApplyDamageToPlayer(int damage)
    {
        if(damage < 0)
            return;
        
        Player.CurrentHP = Math.Max(0, Player.CurrentHP - damage);
    }

    static void UsePotion()
    {
        if (Player.PotionsInventory <= 0)
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }
        
        // Helning av spelaren
        int healAmount = 12;
        
        Player.CurrentHP = Math.Min(Player.MaxHP, Player.CurrentHP + healAmount);
        Player.PotionsInventory--;

        Console.WriteLine($"Du dricker en dryck och återfår {healAmount} HP. Nu har du {Player.CurrentHP} HP.");
    }

    static bool TryRunAway()
    {
        // Flyktschans baserad på karaktärsklass
        string playerClass = Player.PlayerClass ?? "Warrior";
        double chance = 0.25;
        if (playerClass == "Rogue") chance = 0.5;
        if (playerClass == "Mage") chance = 0.35;
        return Rng.NextDouble() < chance;
    }

    static bool IsPlayerDead()
    {
        return Player.CurrentHP <= 0;
    }

    static void AddPlayerXp(int amount)
    {
        Player.PlayerExp += Math.Max(0, amount);
        MaybeLevelUp();
    }

    static void AddPlayerGold(int amount)
    {
        Player.GoldInventory += Math.Max(0, amount);
    }

    static void MaybeLevelUp()
    {
        // Nivåtrösklar
        int exp = Player.PlayerExp;
        int lvl = Player.PlayerLevel;
        
        int nextThreshold = lvl == 1 ? 10 : (lvl == 2 ? 25 : (lvl == 3 ? 45 : lvl * 20));
        if (exp < nextThreshold)
            return;
        
        
            Player.PlayerLevel++;
            Player.PlayerExp = 0;
            
            switch (Player.PlayerClass)
            {
                case "Warrior":
                    Player.MaxHP += 6;
                    Player.AttackStat += 2;
                    Player.DefenceStat += 2;
                    break;
                case "Mage":
                    Player.MaxHP += 4;
                    Player.AttackStat+= 4;
                    Player.DefenceStat += 1;
                    break;
                case "Rogue":
                    Player.MaxHP += 5;
                    Player.AttackStat += 3;
                    Player.DefenceStat += 1;
                    break;
            }

            Player.CurrentHP = Player.MaxHP;

            Console.WriteLine($"Du når nivå {Player.PlayerLevel}! Värden ökade och HP återställd.");
        
    }

    static void MaybeDropLoot(string enemyName)
    {
        // Enkel loot-regel
        if (Rng.NextDouble() < 0.35)
        {
            string item = "Minor Gem";
            
            if (enemyName.Contains("Urdraken"))
            {
                item = "Dragon Scale";
            }

            Player.Inventory.Add(item);
            
            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Rumshändelser =======

    static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        
        if (Rng.NextDouble() < 0.5)
        {
            int gold = Rng.Next(8, 15);
            AddPlayerGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            
            string found = items[Rng.Next(items.Length)];
            
            Player.Inventory.Add(found);
            
            Console.WriteLine($"Du plockar upp: {found}");
        }
        return true;
    }

    static bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            Console.WriteLine($"Guld: {Player.GoldInventory} | Drycker: {Player.PotionsInventory}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var val = (Console.ReadLine() ?? "").Trim();

            switch (val)
            {
                case "1":
                    BuyPotion();
                    break;
                
                case "2":
                    BuyWeapon();
                    break;
                
                case "3":
                    BuyArmor();
                    break;
                
                case "4":
                    SellMinorGems();
                    break;
                
                case "5": 
                    Console.WriteLine("Du säger adjö till köpmannen.");
                    return true;
                
                default:
                    Console.WriteLine("Köpmannen förstår inte ditt val.");
                    break;
                    
            }
        }
    }

    static void BuyPotion()
    {
        if (Player.GoldInventory < 10)
        {
            Console.WriteLine("Du har inte råd.");
            return;
        }
        Player.GoldInventory -= 10;
        Player.PotionsInventory++;
        Console.WriteLine("Du köper en dryck.");
    }

    static void BuyWeapon()
    {
        if (Player.GoldInventory < 25)
        {
            Console.WriteLine("Du har inte råd.");
            return; 
        }
        Player.GoldInventory -= 25;
        Player.AttackStat += 2;
        Console.WriteLine("Du köper ett bättre vapen (+2 ATK).");
    }

    static void BuyArmor()
    {
        if (Player.GoldInventory < 25)
        {
            Console.WriteLine("Du har inte råd.");
            return;
        }
        Player.GoldInventory -= 25;
        Player.DefenceStat += 2;
        Console.WriteLine("Du köper bättre rustning.(+2 DEF)");
    }

    static void SellMinorGems()
    {
        int count = 0;
        foreach (var item in Player.Inventory)
        {
            if (item == "Minor Gem")
            { 
                count++;
            }
        }
        if (count == 0)
        {
            Console.WriteLine("Du har inga Minor Gems i väskan.");
            return;
        }

        Player.Inventory.RemoveAll(item => item == "Minor Gem");
        int goldEarned = count * 5;
        Player.GoldInventory += goldEarned;
        
        Console.WriteLine($"Du säljer {count} st Minor Gem för {count * 5} guld.");
    }

    static bool DoRest()
    {
        Console.WriteLine("Du slår läger och vilar.");
        Player.CurrentHP = Player.MaxHP;
        Console.WriteLine("HP återställt till max.");
        return true;
    }

    // ======= Status =======

    static void ShowStatus()
    {
        Console.WriteLine($"[{Player.PlayerName} | {Player.PlayerClass}] " +
                          $"HP {Player.CurrentHP}/{Player.MaxHP} " +
                          $"ATK {Player.AttackStat} " +
                          $"DEF {Player.DefenceStat} " +
                          $"LVL {Player.PlayerLevel} " +
                          $"XP {Player.PlayerExp}" +
                          $"Guld {Player.GoldInventory}" +
                          $"Drycker {Player.PotionsInventory}"
                          );
        
        if (Player.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: {string.Join(", ", Player.Inventory)}");
        }
    }
    
    // ======= Hjälpmetoder =======

    static int ParseInt(string s, int fallback)
    {
        try
        {
            int value = Convert.ToInt32(s);
            return value;
        }
        catch (Exception e)
        {
            return fallback;
        }
    }
}
