using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SearchDBF
{
    static class StringExtention
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
                if (String.IsNullOrEmpty(value))
                    MessageBox.Show("the string to find may not be empty", "value");

                List<int> indexes = new List<int>();
                for (int index = 0; ; index += value.Length)
                {
                    index = str.IndexOf(value, index);
                    if (index == -1)
                        return indexes;
                    indexes.Add(index);
                }
        }
    }
}
