/*
 * POS Control Tool
 */


using System;
using System.IO;
using System.Linq;


namespace PosControlUtility
{
    class Program
    {

        public static string CSV_PATH = "control.csv";

        static void Main(string[] args)
        {
            bool displayMenu = true;

            while (displayMenu == true)
                displayMenu = MainMenu();
        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("MENU: MAIN");
            Console.WriteLine("Type the option number you wish to choose.\n");
            Console.WriteLine("0) EXIT");
            Console.WriteLine("1) Search by serial\n");
            Console.Write("Option: ");
            string? input = Console.ReadLine();

            if (input == "0")
                return false;
            else if (input == "1")
            {
                Task.Run(() => SearchBySerialMenu()).Wait();
                return true;
            }
            else
            {
                Console.WriteLine("Try again:");
                return true;
            }
        }

        private static void SearchBySerialMenu()
        {
            string[]? headers = ReadCsvHeaders(CSV_PATH);
            if (headers == null)
                return;
            Dictionary<int, string> options = ArrToDictIntStr(headers);

            bool displayMenu = true;
            while (displayMenu)
            {
                Console.Clear();
                Console.WriteLine("MENU: SEARCH BY SERIAL");
                Console.WriteLine("Usage: Pick the number for the data you wish to request. Input POS serial code when prompted and the requested data will appear on-screen. Type 'exit' or 'quit' to go back.\n");

                Console.WriteLine("0) EXIT");
                foreach (KeyValuePair<int, string> option in options)
                    Console.WriteLine("{0}) {1}", option.Key, option.Value);

                Console.Write("\nOption: ");


                string? input = Console.ReadLine();
                if (input == null || input.Trim() == "")
                {
                    Console.WriteLine("Try again");
                    continue;
                }
                else if (input.Trim() == "0") return;

                foreach (KeyValuePair<int, string> option in options)
                    if (int.Parse(input) == option.Key)
                        Task.Run(() => SearchBySerialRun(option.Value)).Wait();
            }
        }

        private static void SearchBySerialRun(string headerToReturn)
        {
            Dictionary<string, string>[] terminalList = CsvToArrOfDicts(CSV_PATH);
            Console.Clear();
            Console.WriteLine("SEARCH BY SERIAL NUMBER");
            Console.WriteLine("Usage: Input serial number and press ENTER. Result will show on screen below serial number. Type 'exit' or 'quit' to go back.\n");
            bool run = true;
            while (run)
            {
                Console.Write("Serial: ");
                string? serial = Console.ReadLine();
                if (serial == null || serial.Trim() == "")
                {
                    Console.WriteLine("Please enter a serial number\n");
                    continue;
                }
                else if (serial.Trim() == "quit" || serial.Trim() == "exit")
                    return;

                // IEnumerable<string> found = terminalList.ToList().Where(t => t["serie"] == serial).Select(t => t[headerToReturn]);

                IEnumerable<string> found = from t in terminalList
                                            where t.ContainsKey("serie") && t["serie"] == serial.Trim()
                                            select t[headerToReturn];

                if (found.Any())
                {
                    foreach (string item in found)
                    {
                        Console.WriteLine(item + "\n");
                        break;
                        // If break is removed, gnarly error appears
                    }
                }
                else
                {
                    Console.WriteLine("Not found\n");
                    continue;
                }

            }
        }

        private static Dictionary<int, string> ArrToDictIntStr(string[] arr)
        {
            int count = 1;
            Dictionary<int, string> dict = new Dictionary<int, string>();
            foreach (string? item in arr)
            {
                dict.Add(count, item);
                count++;
            }
            return dict;
        }

        private static Dictionary<string, string>[] CsvToArrOfDicts(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                List<Dictionary<string, string>> terminalList = new List<Dictionary<string, string>>();
                string[]? headers = ReadCsvHeaders(CSV_PATH);
                if (headers == null) return terminalList.ToArray();

                for (int terminalCount = 0; !reader.EndOfStream; terminalCount++)
                {
                    // if (i == 0) continue;
                    string? line = reader.ReadLine();
                    if (line == null) return terminalList.ToArray();

                    string[] values = (line ?? "").Split(",");

                    Dictionary<string, string> singleTerminal = new Dictionary<string, string>();
                    for (int j = 0; j < headers.Length && j < values.Length; j++)
                    {
                        string key = (headers[j] == "" || headers[j] == null) ? "n/a" : headers[j];
                        string val = (values[j] == "" || values[j] == null) ? "n/a" : values[j];

                        singleTerminal.Add(key, val);
                    }

                    terminalList.Add(singleTerminal);
                }

                // PrintArrOfDicts(terminalList);
                return terminalList.ToArray();
            }
        }

        private static string[]? ReadCsvHeaders(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? headerLine = reader.ReadLine();
                if (headerLine == null)
                    return null;
                string[] headerList = headerLine.Split(",");
                return headerList;
            }

        }

        private static void PrintArrOfDicts(List<Dictionary<string, string>> arr)
        {
            foreach (Dictionary<string,string> dict in arr)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                        Console.Write("{0}: {1}, ", kvp.Key, kvp.Value);
                    Console.Write("\n");
                }
                Console.ReadLine();
        }
    }
}
