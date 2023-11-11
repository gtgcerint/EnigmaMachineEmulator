using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace EnigmaMachinePrj
    {
    internal class Program
        {
        static void Main(string[] args)
            {
            EnigmaMachine machine = new EnigmaMachine();
            EnigmaSettings eSettings = new EnigmaSettings();

            querySettings(eSettings);

            string message = "";
            Console.Write("Enter message to encrypt: ");
            message = Console.ReadLine();
            while (!Regex.IsMatch(message, @"^[a-zA-Z ]+$"))
                {
                Console.Write("Only letters A-Z is allowed, try again: ");
                message = Console.ReadLine();
                }
            message = message.Replace(" ", "").ToUpper();

            // Enter settings on machine
            machine.setSettings(eSettings.rings, eSettings.grund, eSettings.order, eSettings.reflector);

            // The plugboard settings
            foreach (string plug in eSettings.plugs)
                {
                char[] p = plug.ToCharArray();
                machine.addPlug(p[0], p[1]);
                }

            // Message encrypt
            Console.WriteLine();
            Console.WriteLine("Plain text:\t" + message);
            string enc = machine.runEnigma(message);
            Console.WriteLine("Encrypted:\t" + enc);

            // Reset the settings before decrypting!
            machine.setSettings(eSettings.rings, eSettings.grund, eSettings.order, eSettings.reflector);

            // Message decrypt
            string dec = machine.runEnigma(enc);
            Console.WriteLine("Decrypted:\t" + dec);
            Console.WriteLine();


            //Set up all possible options. 
            //Need to limit the possible plug board connections. Accoring to the Enigma manual, there should be up to 13 pairs of plug board cables.
            List<string> plugElements = GenerateCombinations();



            var plugs = GetAllCombinations(plugElements.Take(3).ToList());
            foreach (var plug in plugs)
                {
                Console.WriteLine(string.Join(", ", plug));
                }

            char[] rings = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            char[] grund = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string[] elements = { "I", "II", "III" };
            List<string> rotorOrders = new List<string>();
            foreach (string element1 in elements)
                {
                foreach (string element2 in elements)
                    {
                    foreach (string element3 in elements)
                        {
                        if (element1 != element2 && element2 != element3 && element1 != element3)
                            {
                            rotorOrders.Add(element1 + "-" + element2 + "-" + element3);
                            }
                        }
                    }
                }
            char[] reflectors = "ABC".ToCharArray();


            SingleThread(rings, grund, rotorOrders, reflectors, enc, machine, dec, plugs);

            try
                {
                MultyThread(rings, grund, rotorOrders, reflectors, enc, dec, plugs);
                }
            finally
                {
                Console.WriteLine("--ALL DONE--");
                }

            Console.ReadLine();
            }

        private static void querySettings(EnigmaSettings e)
            {
            string r;
            Console.WriteLine("Enigma Machine Emulator\n");
            Console.Write("Do you want to: [1] Specify settings [2] Use default settings? (Default: [2]): ");
            r = Console.ReadLine();
            while (r != "1" && r != "2" && r != "")
                {
                Console.Write("Invalid input, enter 1, 2 or 3 ");
                r = Console.ReadLine();
                }
            if (r == "1")
                {
                Console.Write("Enter the ring settings (Ex. AAA, MCK, Default: AAA): ");
                r = Console.ReadLine();
                if (r == "")
                    e.rings = new char[] { 'A', 'A', 'A' };
                else
                    e.rings = r.ToCharArray();

                Console.Write("Enter the inital rotor start settings (Ex. AAA, MCK, Default: AAA): ");
                r = Console.ReadLine();
                if (r == "")
                    e.grund = new char[] { 'A', 'A', 'A' };
                else
                    e.grund = r.ToCharArray();

                Console.Write("Enter the order of the rotors (Ex. I-II-III, III-I-II, Default: I-II-III): ");
                r = Console.ReadLine();
                if (r == "")
                    e.order = "I-II-III";
                else
                    e.order = r;

                Console.Write("Enter the reflector to use (A, B, or C, Default: B): ");
                r = Console.ReadLine();
                if (r == "")
                    e.reflector = 'B';
                else
                    e.reflector = r.ToCharArray()[0];

                Console.Write("Enter the plugboard configuration (Ex. KH AB CE IJ, Default: None): ");
                r = Console.ReadLine();
                if (r == "")
                    e.plugs.Clear();
                else
                    {
                    string[] plugs = r.Split(' ');
                    foreach (string s in plugs)
                        {
                        e.plugs.Add(s);
                        }
                    }

                }
            else if (r == "2" || r == "")
                {
                e.setDefault();
                }

            Console.WriteLine();
            }

        public static List<List<T>> GetAllCombinations<T>(List<T> list)
            {
            List<List<T>> result = new List<List<T>>();

            for (long i = 0; i < (1L << list.Count); i++)
                {
                result.Add(new List<T>());
                for (int j = 0; j < list.Count; j++)
                    {
                    if ((i & (1L << j)) != 0)
                        {
                        result[(int)i].Add(list[j]);
                        }
                    }
                }

            return result;
            }

        private static void SingleThread(char[] rings, char[] grund, List<string> rotorOrders, char[] reflectors, string encryptedText, EnigmaMachine machine, string dec, List<List<string>> plugs)
            {


            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            foreach (var reflector in reflectors)
                {
                foreach (var rotorOrder in rotorOrders)
                    {
                    char[] ringSettings = new char[3];
                    foreach (var ring0 in rings)
                        {
                        foreach (var ring1 in rings)
                            {
                            foreach (var ring2 in rings)
                                {
                                ringSettings[0] = ring0;
                                ringSettings[1] = ring1;
                                ringSettings[2] = ring2;


                                char[] grundSettings = new char[3];
                                foreach (var grunt0 in grund)
                                    {
                                    foreach (var grunt1 in grund)
                                        {
                                        foreach (var grunt2 in grund)
                                            {

                                            grundSettings[0] = grunt0;
                                            grundSettings[1] = grunt1;
                                            grundSettings[2] = grunt2;

                                            machine.setSettings(ringSettings, grundSettings, rotorOrder, reflector);

                                            machine.clearPlugBoard();                                           
                                            string attempt = machine.runEnigma(encryptedText);

                                            if (attempt == dec)
                                                {
                                                stopwatch.Stop();
                                                Console.WriteLine("----");
                                                Console.WriteLine("----");
                                                Console.WriteLine("Found!");
                                                Console.WriteLine(attempt);
                                                Console.WriteLine(stopwatch.ElapsedTicks.ToString());
                                                Console.ReadLine();
                                                }


                                            foreach (var plugList in plugs)
                                                {                                                
                                                foreach (var combo in plugList)
                                                    {
                                                    machine.clearPlugBoard();
                                                    char[] p = combo.ToCharArray();
                                                    machine.addPlug(p[0], p[1]);

                                                    attempt = machine.runEnigma(encryptedText);

                                                    if (attempt == dec)
                                                        {
                                                        stopwatch.Stop();
                                                        Console.WriteLine("----");
                                                        Console.WriteLine("----");
                                                        Console.WriteLine("Found!");
                                                        Console.WriteLine(attempt);
                                                        Console.WriteLine(stopwatch.ElapsedTicks.ToString());
                                                        Console.ReadLine();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }



            }

        private static void MultyThread(char[] rings, char[] grund, List<string> rotorOrders, char[] reflectors, string encryptedText, string dec, List<List<string>> plugs)
            {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Parallel.ForEach(reflectors, new ParallelOptions { CancellationToken = token }, (reflector) =>
            {
                foreach (var rotorOrder in rotorOrders)
                    {
                    char[] ringSettings = new char[3];
                    Parallel.ForEach(rings, new ParallelOptions { CancellationToken = token }, (ring0) =>
                    {
                        foreach (var ring1 in rings)
                            {
                            foreach (var ring2 in rings)
                                {
                                ringSettings[0] = ring0;
                                ringSettings[1] = ring1;
                                ringSettings[2] = ring2;

                                char[] grundSettings = new char[3];
                                Parallel.ForEach(grund, new ParallelOptions { CancellationToken = token }, (grunt0) =>
                                {
                                    foreach (var grunt1 in grund)
                                        {
                                        foreach (var grunt2 in grund)
                                            {
                                            grundSettings[0] = grunt0;
                                            grundSettings[1] = grunt1;
                                            grundSettings[2] = grunt2;

                                            EnigmaMachine machine = new EnigmaMachine();
                                            machine.setSettings(ringSettings, grundSettings, rotorOrder, reflector);
                                            string attempt = machine.runEnigma(encryptedText);

                                            if (attempt == dec)
                                                {
                                                stopwatch.Stop();
                                                Console.WriteLine("----");
                                                Console.WriteLine("Settings:");
                                                Console.WriteLine(ringSettings);
                                                Console.WriteLine(grundSettings);
                                                Console.WriteLine(rotorOrder);
                                                Console.WriteLine(reflector);
                                                Console.WriteLine("----");
                                                Console.WriteLine("Found!");
                                                Console.WriteLine(attempt);
                                                Console.WriteLine((stopwatch.ElapsedTicks / Stopwatch.Frequency).ToString());
                                                cts.Cancel(); // Request cancellation
                                                return; // Exit current iteration
                                                }

                                            // Check for cancellation
                                            token.ThrowIfCancellationRequested();
                                            }
                                        }
                                });
                                }
                            }
                    });
                    }
            });



            }


        public static List<string> GenerateCombinations()
            {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            List<string> combinations = new List<string>();

            foreach (char letter1 in alphabet)
                {
                foreach (char letter2 in alphabet)
                    {
                    if (letter1 != letter2)
                        {
                        combinations.Add(letter1.ToString() + letter2.ToString());
                        }
                    }
                }

            return combinations;
            }

        // Short class to hold the settings
        private class EnigmaSettings
            {
            public char[] rings { get; set; }
            public char[] grund { get; set; }
            public string order { get; set; }
            public char reflector { get; set; }
            public List<string> plugs = new List<string>();

            public EnigmaSettings()
                {
                setDefault();
                }

            public void setDefault()
                {
                rings = new char[] { 'A', 'A', 'A' };
                grund = new char[] { 'A', 'A', 'A' };
                order = "I-II-III";
                reflector = 'B';
                plugs.Clear();
                }
            }


        }

    }