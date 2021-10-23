using System;
using System.IO;

namespace YahooDecoder
{
    public class MessageStream : MemoryStream
    {
        private char[] key;
        private int mesgLen;
        private int read = 0;

        public MessageStream(char[] key, int mesgLen)
        {
            this.key = key;
            this.mesgLen = mesgLen;
        }

        public int Read()
        {
            if (read == mesgLen)
            {
                return -1;
            }
            int c = base.ReadByte();
            int r = c ^ key[read % key.Length];
            read++;
            //            System.out.println("("+Integer.toHexString(r)+"/"+Integer.toHexString(c)+")");
            return r;
        }
    }
}
