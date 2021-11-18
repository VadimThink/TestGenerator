using System;

namespace Lib
{
    public class SyntaxException:Exception
    {
        public SyntaxException(string msg) : base(msg) { }
    }
}