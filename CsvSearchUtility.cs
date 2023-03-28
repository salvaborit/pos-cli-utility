using System;
using System.IO;

namespace CsvSearchUtility
{
    public class CsvSearchUtility
    {
        private string ROOT_DIR = AppDomain.CurrentDomain.BaseDirectory;
        private string CSV_NAME = "";
        private string CSV_PATH = "";
        private string INPUT_SEARCH_FIELD = "";
        private string OUTPUT_SEARCH_FIELD = "";
        public void Start()
        {
            Task.Run(() => SelectCsv()).Wait();
            Task.Run(() => SelectIOSearchFields()).Wait();
            Task.Run(() => MainProgram()).Wait();

        }

        public void Entry()
        {
            Task.Run(() => SelectCsv()).Wait();
        }

        public void SelectCsv()
        {
            Console.Clear();
            Console.WriteLine("CSV SEARCH UTILITY\n\n"
                + "Select the CSV to scan from the list below:");

            Dictionary<int, string> files = new Dictionary<int, string>();
            files = ScanCsvFiles();
            Console.WriteLine("\n0) EXIT");
            foreach (KeyValuePair<int, string> kvp in files)
                Console.WriteLine("{0}) {1}", kvp.Key, PathToFilename(kvp.Value));

            Console.Write("\nSELECT CSV FILE: ");
            string input = ReadLine();
            if (input == "0") return;

            foreach (KeyValuePair<int, string> kvp in files)
                if (int.Parse(input) == kvp.Key)
                {
                    CSV_PATH = kvp.Value;
                    CSV_NAME = PathToFilename(CSV_PATH);
                    Task.Run(() => SelectIOSearchFields()).Wait();
                    break;
                }

            Task.Run(() => SelectCsv()).Wait();
        }

        public void SelectIOSearchFields()
        {
            Console.Clear();
            Console.WriteLine("CSV SEARCH UTILITY\n\n"
                + "Reading headers from CSV: " + CSV_NAME);
            string[]? headersRead = ReadCsvHeaders(CSV_PATH);
            if (headersRead == null) return;

            Dictionary<int, string> headers = new Dictionary<int, string>();
            int count = 1;
            foreach (string header in headersRead)
                headers.Add(count++, header);

            Console.WriteLine("\n0) EXIT");
            foreach (KeyValuePair<int, string> kvp in headers)
                Console.WriteLine("{0}) {1}", kvp.Key, kvp.Value);

            Console.Write("\nSELECT INPUT FIELD: ");
            string inputField = ReadLine();
            if (inputField == "0") return;

            bool inputFieldOk = false;
            foreach (KeyValuePair<int, string> kvp in headers)
                if (int.Parse(inputField) == kvp.Key)
                {
                    INPUT_SEARCH_FIELD = kvp.Value;
                    inputFieldOk = true;
                }

            bool outputFieldOk = false;
            Console.Write("SELECT OUTPUT FIELD: ");
            string outputField = ReadLine();
            if (outputField == "0") return;
            foreach (KeyValuePair<int, string> kvp in headers)
                if (int.Parse(outputField) == kvp.Key)
                    {
                    OUTPUT_SEARCH_FIELD = kvp.Value;
                    outputFieldOk = true;
                }

            if (inputFieldOk && outputFieldOk)
                Task.Run(() => MainProgram()).Wait();
            Task.Run(() => SelectIOSearchFields()).Wait();



        }

        public void MainProgram()
        {
            Dictionary<string, string>[] terminals = CsvToArrOfDicts(CSV_PATH);
            Console.Clear();
            Console.WriteLine("CSV SEARCH UTILITY\n");
            Console.WriteLine("Usage: Input [{0}] and press ENTER. [{1}] will"
                + " show on screen below [{0}]."
                + " Type 'exit' or 'quit' to go back.\n",
                INPUT_SEARCH_FIELD.ToUpper(), OUTPUT_SEARCH_FIELD.ToUpper());

            bool run = true;
            while (run)
            {
                Console.Write("{0}: ", INPUT_SEARCH_FIELD);
                string? serial = Console.ReadLine();
                if (serial == null || serial.Trim() == "")
                {
                    Console.WriteLine("Please enter a {0}\n", INPUT_SEARCH_FIELD);
                    continue;
                }
                else if (serial.Trim() == "quit" || serial.Trim() == "exit")
                    return;

                // IEnumerable<string> found = terminals.ToList().Where(t => t["serie"] == serial).Select(t => t[headerToReturn]);

                IEnumerable<string> found = from t in terminals
                                            where t.ContainsKey(INPUT_SEARCH_FIELD) && t[INPUT_SEARCH_FIELD] == serial.Trim()
                                            select t[OUTPUT_SEARCH_FIELD];

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

        public Dictionary<int, string> ScanCsvFiles()
        {
            Dictionary<int, string> files = new Dictionary<int, string>();
            string[] filesFetched = Directory.GetFiles("./csv", "*.csv");
            int count = 1;
            foreach (string file in filesFetched)
                files.Add(count++, file);
            return files;
        }

        public string PathToFilename(string filename)
        {
            int lastSlashToken = filename.LastIndexOf("/");
            return filename.Substring(lastSlashToken + 1);
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

        public Dictionary<string, string>[] CsvToArrOfDicts(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                List<Dictionary<string, string>> terminals = new List<Dictionary<string, string>>();
                string[]? headers = ReadCsvHeaders(CSV_PATH);
                if (headers == null) return terminals.ToArray();

                for (int terminalCount = 0; !reader.EndOfStream; terminalCount++)
                {
                    if (terminalCount == 0) continue;
                    string? line = reader.ReadLine();
                    if (line == null) return terminals.ToArray();

                    string[] values = (line ?? "").Split(",");

                    Dictionary<string, string> singleTerminal = new Dictionary<string, string>();
                    for (int j = 0; j < headers.Length && j < values.Length; j++)
                    {
                        string key = (headers[j] == "" || headers[j] == null) ? "n/a" : headers[j];
                        string val = (values[j] == "" || values[j] == null) ? "n/a" : values[j];

                        singleTerminal.Add(key, val);
                    }
                    terminals.Add(singleTerminal);
                }
                return terminals.ToArray();
            }
        }


        public string ReadLine()
        {
            string? input = Console.ReadLine();
            if (input == null) return "null";
            return input.Trim();
        }
    }
}
