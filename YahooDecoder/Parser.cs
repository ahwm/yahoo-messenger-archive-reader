using System;
using System.IO;

namespace YahooDecoder
{
    public class Parser
    {
        private readonly string Buddy;
        private readonly string MyID;
        private readonly char[] Key;
        private readonly Stream InputStream;
                 
        public Parser(string buddyId, string myId, Stream inputStream)
        {
            Buddy = buddyId;
            MyID = myId;
            InputStream = inputStream;
        }

        public Record ReadRecord()
        {
            var date = ReadDate();
            int unknown1 = ReadInt();
            if (unknown1 != 0 && unknown1 != 6)
            {
                throw new NotSupportedException($"unknown1: {unknown1}");
            }

            int source = ReadInt();
            bool isFromBuddy = source != 0;
            Record record = new Record(date, isFromBuddy ? Buddy : MyID, isFromBuddy);

            int mesgLen = ReadInt();
            Stream mis = new MemoryStream(InputStream, Key, mesgLen);
            int r;
            StringBuffer buff = new StringBuffer();
            while ((r = mis.read()) != -1)
            {
                if (r == 0x1b)
                {
                    r = mis.read();
                    if (r != 0x5b)
                    {
                        throw badFormatError(r);
                    }
                    else
                    {
                        if (buff.length() > 0)
                        {
                            record.addToken(new Token(Token.TokenTypes.TEXT, buff.toString()));
                            buff.setLength(0);
                        }
                        record.addToken(readFontAttribute(mis));
                    }
                    continue;
                }
                buff.append((char)r);
            }
        }

        private DateTime ReadDate()
        {
            long time = ReadInt() * 1000;
            return DateTimeOffset.FromUnixTimeSeconds(time).DateTime;
        }

        private int ReadInt()
        {
            int[] dt = new int[4];
            for (int i = dt.Length - 1; i >= 0; i--)
            {
                dt[i] = InputStream.ReadByte();
                if (dt[i] == -1)
                {
                    throw new EndOfStreamException();
                }
            }
            int time = 0;
            for (int i = 0; i < dt.Length; i++)
            {
                time <<= 8;
                time |= dt[i];
            }
            return time;
        }
    }
}
