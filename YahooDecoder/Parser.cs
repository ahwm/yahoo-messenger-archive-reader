using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace YahooDecoder
{
    public class Parser
    {
        private readonly string Buddy;
        private readonly string MyID;
        private readonly char[] Key;
        private readonly Stream InputStream;

        public class StandardColors
        {
            public readonly static StandardColors BLACK = new StandardColors(0, 0, 0);
            public readonly static StandardColors BLUE = new StandardColors(0, 0, 255);
            public readonly static StandardColors TEAL = new StandardColors(0, 128, 128);
            public readonly static StandardColors SKY_BLUE_4 = new StandardColors(238, 243, 246); //EEF3F6
            public readonly static StandardColors GREEN = new StandardColors(0, 128, 0);
            public readonly static StandardColors MAGENTA = new StandardColors(255, 0, 128);
            public readonly static StandardColors PURPLE = new StandardColors(128, 0, 128);
            public readonly static StandardColors ORANGE = new StandardColors(255, 128, 0);
            public readonly static StandardColors RED = new StandardColors(255, 0, 0);
            public readonly static StandardColors OLIVE = new StandardColors(128, 128, 0);

            private static Color color;

            public StandardColors(int r, int g, int b)
            {
                color = Color.FromArgb(r, g, b);
            }

            public Color GetColor()
            {
                return color;
            }

            public static StandardColors[] GetValues()
            {
                return new StandardColors[]
                {
                    BLACK,
                    BLUE,
                    TEAL,
                    SKY_BLUE_4,
                    GREEN,
                    MAGENTA,
                    PURPLE,
                    ORANGE,
                    RED,
                    OLIVE

                };
            }
        }

        public Parser(string buddyId, string myId, Stream inputStream)
        {
            Buddy = buddyId;
            MyID = myId;
            Key = myId.ToCharArray();
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
            var mis = new MessageStream(Key, mesgLen);
            InputStream.CopyTo(mis);
            mis.Position = 0;
            int r;
            StringBuilder buff = new StringBuilder();
            while ((r = mis.Read()) != -1)
            {
                if (r == 0x1b)
                {
                    r = mis.Read();
                    if (r != 0x5b)
                    {
                        //throw new FormatException($"Expected 0x5b, returned {r:x}");
                    }
                    else
                    {
                        if (buff.Length > 0)
                        {
                            record.AddToken(new Token(Token.TokenType.TEXT, buff.ToString()));
                            buff.Clear();
                        }
                        record.AddToken(ReadFontAttribute(mis));
                    }
                    continue;
                }
                buff.Append((char)r);
            }
            if (buff.Length > 0)
            {
                record.AddToken(new Token(Token.TokenType.TEXT, buff.ToString()));
                buff.Clear();
            }

            int unknown2 = ReadInt();
            if (unknown2 != 0 && unknown2 != 6)
            {
                throw new NotSupportedException($"unknown2: {unknown2}");
            }
            return record;
        }

        private Token ReadFontAttribute(MessageStream mis)
        {
            int r = mis.Read();
            try
            {
                switch (r)
                {
                    case '#'/*0x23*/:
                        return ReadCustomColor(mis);
                    case '0'/*0x30*/:
                        return new Token(Token.TokenType.UNKNOWN, r);
                    case '1'/*0x31*/:
                        return new Token(Token.TokenType.FONT, Token.FontFormat.BOLD);
                    case '2'/*0x32*/:
                        return new Token(Token.TokenType.FONT, Token.FontFormat.ITALIC);
                    case '3'/*0x33*/:
                        return ReadStandardColor(mis);
                    case '4'/*0x34*/:
                        return new Token(Token.TokenType.FONT, Token.FontFormat.UNDERLINE);
                    case 'l'/*0x6c*/:
                        //NOTE: Seems to come only from linux machines.
                        //      See: xxxsurya\20071226*.dat
                        //           cprxxxxxxreddy\20060320*.dat
                        //           navxxxxxnth_r\20060118*.dat
                        return new Token(Token.TokenType.BEGIN_LINK, r);
                    case 'x'/*0x78*/:
                        r = mis.Read();
                        switch (r)
                        {
                            case '0'/*0x30*/:
                                return new Token(Token.TokenType.UNKNOWN, r);
                            case '1'/*0x31*/:
                                return new Token(Token.TokenType.FONT, Token.FontFormat.UNDO_BOLD);
                            case '2'/*0x32*/:
                                return new Token(Token.TokenType.FONT, Token.FontFormat.UNDO_ITALIC);
                            case '4'/*0x34*/:
                                return new Token(Token.TokenType.FONT, Token.FontFormat.UNDO_UNDERLINE);
                            case 'l'/*0x6c*/:
                                return new Token(Token.TokenType.END_LINK, r);
                            default:
                                throw new FormatException($"Unexpected value for {nameof(r)}: {r:x}");
                        }
                    default:
                        throw new FormatException($"Unexpected value for {nameof(r)}: {r:x}");
                }
            }
            finally
            {
                if ((r = mis.Read()) != 'm')
                {
                    throw new FormatException($"Unexpected value for {nameof(r)}: {r:x}");
                }
            }
        }

        private Token ReadStandardColor(MessageStream mis)
        {
            StandardColors[] standardColors = StandardColors.GetValues();
            int r = mis.Read();
            int idx = r - '0';
            if (idx >= 0 && idx < standardColors.Length)
            {
                return new Token(Token.TokenType.STANDARD_COLOR, standardColors[idx]);
            }
            else
            {
                throw new FormatException($"Unexpected value for {nameof(r)}: {r:x}"); ;
            }
        }

        private Token ReadCustomColor(MessageStream mis)
        {
            int r = ((c1(mis) << 4) | c1(mis));
            int g = ((c1(mis) << 4) | c1(mis));
            int b = ((c1(mis) << 4) | c1(mis));
            Color color;
            try
            {
                color = Color.FromArgb(r, g, b);
            }
            catch (ArgumentException)
            {
                //System.err.println("Bad color: [" + r + ", " + g + ", " + b + "]");
                color = Color.Black;
            }
            return new Token(Token.TokenType.CUSTOM_COLOR, color);
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

        private int c1(MessageStream mis)
        {
            int v = mis.Read();
            v ^= '0';
            if (v > 0xF)
            {
                if (v - 'G' > 0xF)
                {
                    //NOTE: Seems to come when we get messages from a linux machine. See: xxxsurya\20071210xxx.dat
                    v -= 'g';
                }
                else
                {
                    v -= 'G';
                }
            }
            return v;
        }
    }
}
