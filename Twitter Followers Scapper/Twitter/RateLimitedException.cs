using System;

namespace Twitter
{
    class RateLimitedException : Exception
    {
        public RateLimitedException()
        {
        }

        public RateLimitedException(string message)
            : base(message)
        {
        }

        public RateLimitedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
