using System;
using System.Collections.Generic;

namespace YahooDecoder
{
    public class Record
    {
        public DateTime Date { get; }
        public string Author { get; }
        public bool IsFromBuddy { get; }
        private List<Token> Tokens { get; }

        public Record(DateTime date, string author, bool isFromBuddy)
        {
            Date = date;
            Author = author;
            IsFromBuddy = isFromBuddy;
            Tokens = new List<Token>();
        }

        public void AddToken(Token token)
        {
            Tokens.Add(token);
        }
    }
}
