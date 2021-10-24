using System;
using System.IO;

namespace YahooDecoder
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length > 0)
            //{
                //if (args[0].EndsWith(".dat"))
                //    Process(new FileInfo(args[0]));
                Process(new DirectoryInfo("/Users/adamhumpherys/OneDrive/Yahoo/ah15wm/Archive/Messages/dmb1981"));
            //}
        }

        static void Process(FileSystemInfo file)
        {
            if (file is DirectoryInfo directory)
            {
                foreach (FileInfo fileInfo in directory.EnumerateFiles())
                {
                    if (fileInfo.Name.ToUpper() == ".DS_STORE") // handle running on macOS
                        continue;
                    ProcessFile(fileInfo);
                }
            }
            else if (file is FileInfo fileInfo)
            {
                ProcessFile(fileInfo);
            }
        }

        static void ProcessFile(FileInfo file)
        {
            string buddy = file.Directory.Name;
            string myId = "mkpowner7";// file.Directory.Parent.Parent.Parent.Name;
            FileStream mis = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            Parser parser = new Parser(buddy, myId, mis);
            try
            {
                while (true)
                {
                    Record record = parser.ReadRecord();
                    //printRecord(record, PRINT_HTML_FORMAT);
                }
            }
            catch (EndOfStreamException)
            {
            }
        }
    }
}
