﻿namespace Twitter
{
    class User
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Handle { get; set; }

        public override string ToString()
        {
            return "Name: " + Name + "\n" +
                    "Bio: " + Bio + "\n" +
                    "Handle: " + Handle;
        }
    }
}
