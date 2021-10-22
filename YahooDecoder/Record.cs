using System;
namespace YahooDecoder
{
    public class Record
    {
        public DateTime Date { get; }
        public string Author { get; }
        public bool IsFromBuddy { get; }

        public Record(DateTime date, string author, bool isFromBuddy)
        {
            Date = date;
            Author = author;
            IsFromBuddy = isFromBuddy;
        }
    }
}
