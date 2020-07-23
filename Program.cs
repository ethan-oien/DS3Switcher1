//ECHO778'S DARK SOULS 3 SWITCHER

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace DS3_Switcher
{
    //global variables
    public static class Globals
    {
        public static string path = Directory.GetCurrentDirectory() + "\\";
        //public static string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\DS3Switcher\";
        public static long steamID64;

        public static bool config = false;
        public static string ds3Path = "";
        public static string ds3EXE = "";
        public static string arguments = "";
    }

    //handles methods and fields related to the ids text file
    static class IDList
    {
        private static List<long> ids = new List<long>();
        private static List<char> modded = new List<char>();
        private static List<string> list = new List<string>();
        private static string file = Globals.path + @"switcherIDs.txt";

        //writes new user to text file
        private static bool UserInList(long id)
        {
            if(ids != null)
            {
                foreach(long i in ids)
                {
                    if (i == id)
                        return true;
                }
            }
            return false;
        }

        //creates a new user in the text file with their choice of modded or unmodded DS3
        private static void NewUser(long id)
        {
            char modChoice;
            char sure;
            string newRecord;

            do
            {
                Console.WriteLine("Steam User \"" + Globals.steamID64 + "\" is not registered.\nIs this account modded or unmodded?");
                do
                {
                    Console.WriteLine("Please make your selection using the m(odded) or u(nmodded) key.");
                    modChoice = Console.ReadLine()[0];
                }
                while (modChoice != 'm' && modChoice != 'u');

                if (modChoice == 'm')
                {
                    Console.WriteLine("You have chosen MODDED. Are you sure?");
                }
                else if (modChoice == 'u')
                {
                    Console.WriteLine("You have chosen UNMODDED. Are you sure? ");
                }
                do
                {
                Console.WriteLine("Please respond with the y(es) or n(o) key.");
                sure = Console.ReadLine()[0];
                }
                while (sure != 'y' && sure != 'n');
            } while (sure == 'n');

            newRecord = modChoice + Convert.ToString(id);

            if (modChoice == 'm')
            {
                Console.WriteLine("User " + Globals.steamID64 + " added as a MODDED user.");
            }
            else if (modChoice == 'u')
            {
                Console.WriteLine("User " + Globals.steamID64 + " added as a UNMODDED user.");
            }

            list.Add(newRecord);
            File.WriteAllLines(file, list);
            ParseID();
        }

        //reads ids text file into two arrays above
        private static void ParseID()
        {
            if (!Directory.Exists(Globals.path) || !File.Exists(file))
            {
                Directory.CreateDirectory(Globals.path);
                File.Create(file).Close();
            }

            list = File.ReadAllLines(file).ToList();

            modded.Clear();
            ids.Clear();

            for(int i = 0; i < list.Count; i++)
            {
                modded.Add(list[i][0]);
                ids.Add(Convert.ToInt64(list[i].Substring(1, 17)));
            }
        }

        //initializes the list, called once in main
        public static void InitList(long id)
        {
            ParseID();
            if(!UserInList(id))
            {
                NewUser(id);
            }
        }
        
        //returns -1 if id doesn't exist, otherwise returns id index
        public static int IDInList(long id)
        {
            for(int i = 0; i < ids.Count; i++)
            {
                if(ids[i] == id)
                {
                    return i;
                }
            }
            return -1;
        }

        //returns true or false, depending on if the id is marked as modded or not
        public static bool IDIsModded(long id)
        {
            int i = IDInList(id);

            if (i == -1)
                throw new Exception("ID does not exist in list.");

            if (modded[i] == 'm')
                return true;
            else
                return false;
        }

        //changes passed ID to modded
        public static void ModID(long id)
        {
            for(int i=0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    modded[i] = 'm';
                    list[i] = modded[i] + Convert.ToString(ids[i]);
                }
            }

            File.WriteAllLines(file, list);
            ParseID();
        }

        //changes passed ID to unmodded
        public static void UnmodID(long id)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    modded[i] = 'u';
                    list[i] = modded[i] + Convert.ToString(ids[i]);
                }
            }

            File.WriteAllLines(file, list);
            ParseID();
        }
    }

    //handles methods and fields related to the dll in the DS3 folder
    static class DLLChecker
    {
        private static string dll = @"dinput8.dll";

        //checks for the dll in specified dark souls folder
        private static bool DLLInDS3()
        {
            if (File.Exists(Globals.ds3Path + dll))
                return true;
            else
                return false;
        }

        //removes the dll from the dark souls folder
        public static int RemoveDLL()
        {
            if (!DLLInDS3())
                return 0;

            if (!File.Exists(Globals.path + dll))
                File.Copy(Globals.ds3Path + dll, Globals.path + dll);

            File.Delete(Globals.ds3Path + dll);

            if (!DLLInDS3())
                return 0;
            else
                return 1;
        }

        //adds the dll to the dark souls folder
        public static int InsertDLL()
        {
            if (DLLInDS3())
                return 0;

            try
            {
                File.Copy(Globals.path + dll, Globals.ds3Path + dll);
            }
            catch
            {
                throw new Exception("DLL not found! Are there any mods installed?");
            }

            if (DLLInDS3())
                return 0;
            else
                return 1;
        }
    }

    //starts DS3 with command line arguments specified in config
    static class AppStarter
    {
        public static void StartDS3()
        {
            try
            {
                if (Globals.arguments == "")
                    Process.Start(Globals.ds3EXE);
                else
                    Process.Start(Globals.ds3EXE, Globals.arguments);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw new Exception("Dark Souls Executable not found! Did you choose the right path to DarkSoulsIII.exe?");
            }
        }
    }

    //handles steamid related methods
    static class Steam
    {
        private static long SteamID3toSteamID64(long steamid3)
        {
            return Convert.ToInt64("7656119" + (steamid3 + 7960265728));
        }

        private static long FetchSteamID3()
        {
            string regExpCmd = @"/C reg export HKCU\Software\Valve\Steam\ActiveProcess " + "\"" + Globals.path + "steamsession.reg\"";
            List<string> temp = new List<string>();
            long steamID3 = -1;

            File.Delete(Globals.path + "steamsession.reg");

            ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
            cmdsi.Arguments = regExpCmd;
            Process cmd = Process.Start(cmdsi);
            cmd.WaitForExit();

            try
            {
                temp = File.ReadAllLines(Globals.path + "steamsession.reg").ToList();
            }
            catch
            {
                throw new Exception("Registry export failed!");
            }

            foreach(string i in temp)
            {
                if (i.Contains("ActiveUser"))
                {
                    steamID3 = Convert.ToInt32((i.Substring(i.IndexOf(":"))).Remove(0,1), 16);
                }
            }

            return steamID3;
        }

        public static long FetchSteamID64()
        {
            return SteamID3toSteamID64(FetchSteamID3());
        }
    }

    //
    static class Config
    {
        //main call for config
        public static void Menu()
        {
            int option = -1;

            Console.WriteLine("Program started in " + Directory.GetCurrentDirectory());
            Console.WriteLine("Launching config...");

            while (true)
            {
                Console.WriteLine("\nCONFIG");
                Console.WriteLine("Current User: " + Globals.steamID64);
                if(IDList.IDIsModded(Globals.steamID64))
                    Console.WriteLine("Status: MODDED\n");
                else
                    Console.WriteLine("Status: UNMODDED\n");
                Console.Write("1) Change path to DS3 Folder");
                if (Globals.ds3Path == "")
                    Console.Write(" (NEEDS TO BE SET)\n");
                else
                    Console.Write("\n");
                Console.WriteLine("2) Change path to DS3 Executable");
                Console.WriteLine("3) Change arguments for launching DS3");
                if (IDList.IDIsModded(Globals.steamID64))
                    Console.WriteLine("4) Switch current account to UNMODDED");
                else
                    Console.WriteLine("4) Switch current account to MODDED");
                Console.WriteLine("5) Exit and Save\n");

                Console.WriteLine("Select an option: ");

                do
                {

                    try
                    {
                        option = Convert.ToInt32(Console.ReadLine()[0]) - 48;
                        if (option < 1 || option > 5)
                        {
                            option = -1;
                            throw new Exception("Option outside of range.");
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Please enter one numbered option and hit enter.");
                    }
                } while (option == -1);

                switch (option)
                {
                    case 1:
                        FolderChange();
                        break;
                    case 2:
                        EXEChange();
                        break;
                    case 3:
                        ArgumentChange();
                        break;
                    case 4:
                        ModdedToggle(Globals.steamID64);
                        break;
                    case 5:
                        if (Globals.ds3Path != "")
                            Globals.config = true;
                        RewriteGlobals();
                        Environment.Exit(0);
                        break;
                }
            }
        }

        //called at the end of program to rewrite globals into config file
        public static void RewriteGlobals()
        {
            string file = Globals.path + "switcher.config";

            List<string> newLines = new List<string>();

            newLines.Add("config=" + Globals.config);
            newLines.Add("ds3Path=" + Globals.ds3Path);
            newLines.Add("ds3EXE=" + Globals.ds3EXE);
            newLines.Add("arguments=" + Globals.arguments);

            File.WriteAllLines(file, newLines);
        }

        //called when ds3 folder change option is selected
        private static void FolderChange()
        {
            char sure;
            string newFolder;

            Console.WriteLine("Current Dark Souls 3 Folder path: " + Globals.ds3Path);
            Console.WriteLine("Would you like to change this setting?");
            do
            {
                Console.WriteLine("Please indicate your response with the y(es) or n(o) key.");
                sure = Console.ReadLine()[0];
            } while (sure != 'y' && sure != 'n');

            if (sure=='n')
                return;

            Console.WriteLine("Please write the path to your Dark Souls 3 Game Folder:");
            Console.WriteLine("ex. \"" + @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\" + "\"");
            newFolder = Console.ReadLine();
            while (newFolder.Contains("\""))
            {
                newFolder = newFolder.Remove(newFolder.IndexOf("\""), 1);
            }
            if (!newFolder.EndsWith("\\"))
            {
                newFolder = newFolder + "\\";
            }

            Globals.ds3Path = newFolder;
            if (Globals.ds3EXE == "")
                Globals.ds3EXE = Globals.ds3Path + "DarkSoulsIII.exe";
            Console.WriteLine("New Dark Souls 3 Game Folder Location: " + newFolder);
        }

        //called when ds3 exe change option is selected
        private static void EXEChange()
        {
            char sure;
            string newPath;

            Console.WriteLine("Current Dark Souls 3 Executable path: " + Globals.ds3EXE);
            Console.WriteLine("Would you like to change this setting?");
            do
            {
                Console.WriteLine("Please indicate your response with the y(es) or n(o) key.");
                sure = Console.ReadLine()[0];
            } while (sure != 'y' && sure != 'n');

            if (sure == 'n')
                return;

            Console.WriteLine("Please write the path to your Dark Souls 3 Executable:");
            Console.WriteLine("ex. \"" + @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game\DarkSoulsIII.exe" + "\"");
            newPath = Console.ReadLine();
            while (newPath.Contains("\""))
            {
                newPath = newPath.Remove(newPath.IndexOf("\""), 1);
            }
            if (newPath.EndsWith("\\"))
            {
                newPath = newPath.Remove(newPath.Length - 1, 1);
            }

            Globals.ds3EXE = newPath;
            Console.WriteLine("New Dark Souls 3 Executable Location: " + newPath);
        }

        //called when ds3 folder change option is selected
        private static void ArgumentChange()
        {
            char sure;
            string newArgs;

            Console.WriteLine("Current arguments to running Dark Souls 3: " + Globals.arguments);
            Console.WriteLine("Would you like to change this setting?");
            do
            {
                Console.WriteLine("Please indicate your response with the y(es) or n(o) key.");
                sure = Console.ReadLine()[0];
            } while (sure != 'y' && sure != 'n');

            if (sure == 'n')
                return;

            Console.WriteLine("Please write new run arguments (separated by spaces):");
            Console.WriteLine("ex. \"" + "arg1 arg2" + "\"");
            newArgs = Console.ReadLine();
            while (newArgs.Contains("\""))
            {
                newArgs = newArgs.Remove(newArgs.IndexOf("\""), 1);
            }

            Globals.arguments = newArgs;
            Console.WriteLine("New Dark Souls 3 Run Arguments: " + newArgs);
        }

        //called when modded toggle is selected
        private static void ModdedToggle(long id)
        {
            char sure;
            string toggle;

            if (IDList.IDIsModded(id))
                toggle = "UNMODDED";
            else
                toggle = "MODDED";

            Console.WriteLine("Are you sure you would like to change the current user to " + toggle + "?");
            do
            {
                Console.WriteLine("Please indicate your response with the y(es) or n(o) key.");
                sure = Console.ReadLine()[0];
            } while (sure != 'y' && sure != 'n');

            if (sure == 'n')
                return;

            if (IDList.IDIsModded(id))
                IDList.UnmodID(id);
            else
                IDList.ModID(id);

            Console.WriteLine("Steam User \"" + Globals.steamID64 + "\" changed to " + toggle + " user.");
        }
    }

    //main class
    static class Switcher
    {
        private static void ReadGlobals()
        {
            string file = Globals.path + "switcher.config";
            List<string> lines = new List<string>();

            if (!Directory.Exists(Globals.path) || !File.Exists(file))
            {
                Directory.CreateDirectory(Globals.path);
                File.Create(file).Close();
            }

            lines = File.ReadAllLines(file).ToList();

            foreach(string i in lines)
            {
                string field = i.Substring(0, i.IndexOf("="));
                string value = i.Substring(i.IndexOf("=") + 1);

                //quite possibly the shittiest code ever, couldnt find a better way to do it with my knowledge
                switch(field)
                {
                    case "config":
                        Globals.config = Convert.ToBoolean(value);
                        break;
                    case "ds3Path":
                        Globals.ds3Path = value;
                        break;
                    case "ds3EXE":
                        Globals.ds3EXE = value;
                        break;
                    case "arguments":
                        Globals.arguments = value;
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            ReadGlobals();
            Globals.steamID64 = Steam.FetchSteamID64();
            if (Globals.steamID64 == 76561197960265728)
                throw new Exception("Not signed into Steam! Please sign in.");
            IDList.InitList(Globals.steamID64);

            if (args.Length > 1)
            {
                throw new Exception("Only 1 argument accepted. Use \"config\" to enter config mode.");
            }
            else if(args.Length == 1)
            {
                if (args[0] == "config")
                    Config.Menu();
            }

            if (!Globals.config)
                Config.Menu();

            if (IDList.IDIsModded(Globals.steamID64))
            {
                if (DLLChecker.InsertDLL() == 0)
                {
                    AppStarter.StartDS3();
                    Environment.Exit(0);
                }

                else
                    throw new Exception("Unable to insert DLL into Dark Souls 3 folder, game not started.");
            }
            else
            {
                if (DLLChecker.RemoveDLL() == 0)
                {
                    AppStarter.StartDS3();
                    Environment.Exit(0);
                }
                else
                    throw new Exception("Unable to remove DLL from Dark Souls 3 folder, game not started.");

            }
        }
    }
}