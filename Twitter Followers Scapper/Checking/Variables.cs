using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Neos07.Checking
{
    class Variables : IDictionary<String, String>
    {
        private readonly Dictionary<String, String> variables = new Dictionary<string, string>();

        public Variables()
        {
        }

        public string this[string key] { get => variables[key]; set => variables[key] = value; }

        public ICollection<string> Keys => variables.Keys;

        public ICollection<string> Values => variables.Values;

        public int Count => variables.Count;

        public bool IsReadOnly => false;

        public void Add(string key, string value) => variables.Add(key, value);

        public void Add(KeyValuePair<string, string> item) => variables.Add(item.Key, item.Value);

        public void Clear() => variables.Clear();

        public bool Contains(KeyValuePair<string, string> item) => variables.ContainsKey(item.Key) && variables[item.Key] == item.Value;

        public bool ContainsKey(string key) => variables.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (KeyValuePair<String, String> variable in variables)
            {
                array[i] = variable;
                i++;
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => variables.GetEnumerator();

        public bool Remove(string key) => variables.Remove(key);

        public bool Remove(KeyValuePair<string, string> item)
        {
            if (variables.ContainsKey(item.Key) && variables[item.Key] == item.Value)
                return variables.Remove(item.Key);
            return false;
        }

        public bool TryGetValue(string key, out string value) => variables.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => variables.GetEnumerator();

        public String Transform(String Input)
        {
            String output = Input;
            Regex regex = new Regex("%([A-Za-z0-9]+)%");
            MatchCollection matches = regex.Matches(Input);
            foreach (Match match in matches)
            {
                if (ContainsKey(match.Groups[1].Value))
                {
                    output = Regex.Replace(output, match.Groups[0].Value, this[match.Groups[1].Value]);
                }
            }

            return output;
        }
    }
}
