using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Neos07.Checking;

namespace Twitter
{
    abstract class Scraper<T>
    {
        protected readonly Checker checker;

        public bool IsThereMoreItems { get; set; }

        protected string position;
        public string Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                checker.Variables["MAXPOSITION"] = position;
                IsThereMoreItems = true;
            }
        }

        public Scraper(Checker checker)
        {
            this.checker = checker;
            position = null;
            IsThereMoreItems = true;
        }

        public abstract Task<T[]> NextAsync();
    }
}
