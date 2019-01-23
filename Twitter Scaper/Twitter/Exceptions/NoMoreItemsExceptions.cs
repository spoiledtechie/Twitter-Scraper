using System;
using System.Collections.Generic;
using System.Text;

namespace Twitter
{
    class NoMoreItemsExceptions : Exception
    {
        public NoMoreItemsExceptions()
        {
        }

        public NoMoreItemsExceptions(string message)
            : base(message)
        {
        }

        public NoMoreItemsExceptions(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
