using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Hacknet
{
    public class ProgressionFlags
    {
        private readonly List<string> Flags = new List<string>();

        public bool HasFlag(string flag)
        {
            return Flags.Contains(flag);
        }

        public void AddFlag(string flag)
        {
            if (HasFlag(flag))
                return;
            Flags.Add(flag);
        }

        public void RemoveFlag(string flag)
        {
            Flags.Remove(flag);
        }

        public void Load(XmlReader rdr)
        {
            Flags.Clear();
            while (!(rdr.Name == "Flags") || !rdr.IsStartElement())
            {
                rdr.Read();
                if (rdr.EOF)
                    throw new InvalidOperationException("XML reached End of file too fast!");
            }
            var num1 = (int) rdr.MoveToContent();
            var str1 = rdr.ReadElementContentAsString();
            var separator = new char[1]
            {
                ','
            };
            var num2 = 1;
            foreach (var str2 in str1.Split(separator, (StringSplitOptions) num2))
                Flags.Add(str2.Replace("[%%COMMAREPLACED%%]", ","));
        }

        public string GetSaveString()
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < Flags.Count; ++index)
            {
                stringBuilder.Append(Flags[index].Replace(",", "[%%COMMAREPLACED%%]"));
                stringBuilder.Append(",");
            }
            if (stringBuilder.Length > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return "<Flags>" + stringBuilder + "</Flags>\r\n";
        }
    }
}