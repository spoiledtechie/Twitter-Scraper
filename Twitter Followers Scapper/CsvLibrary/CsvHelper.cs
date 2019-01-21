using System;
using System.Collections;
using System.Reflection;

namespace CsvLibrary
{
    static class CsvHelper
    {
        public static string GetCsvHeader(this object o)
        {
            Type type = o.GetType();
            string header = "";
            IEnumerator enumerator = type.GetProperties().GetEnumerator();
            bool flag = enumerator.MoveNext();
            while (flag)
            {
                header += ((PropertyInfo)enumerator.Current).Name;
                flag = enumerator.MoveNext();
                if (flag)
                    header += ",";
            }
            return header;
        }

        public static string ToCsv(this object o)
        {
            Type type = o.GetType();
            string csv = "";
            IEnumerator enumerator = type.GetProperties().GetEnumerator();
            bool flag = enumerator.MoveNext();
            while (flag)
            {
                csv += "\"" + ((PropertyInfo)enumerator.Current).GetValue(o) + "\"";
                flag = enumerator.MoveNext();
                if (flag)
                    csv += ",";
            }
            return csv;
        }
    }
}
