using System;
namespace YahooDecoder
{
    public class Token
    {
        public enum TokenType
        {
            UNKNOWN,
            TEXT,
            FONT,
            STANDARD_COLOR,
            CUSTOM_COLOR,
            BEGIN_LINK,
            END_LINK
        }
        public enum FontFormat
        {
            BOLD,
            ITALIC,
            UNDERLINE,
            UNDO_BOLD,
            UNDO_ITALIC,
            UNDO_UNDERLINE
        }

        public TokenType Type { get; }
        public object Value { get; }

        public Token(TokenType type, object value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{{{Value}}}";
        }
    }
}
