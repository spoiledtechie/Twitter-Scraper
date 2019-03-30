using System.Threading.Tasks;

namespace Twitter
{
    abstract class Scraper<T>
    {

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
                //checker.Variables["MAXPOSITION"] = position;
                IsThereMoreItems = true;
            }
        }

        public Scraper(/*Checker checker*/)
        {
            //this.checker = checker;
            position = null;
            IsThereMoreItems = true;
        }

        public abstract Task<T[]> NextAsync();
    }
}
