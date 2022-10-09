using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GraphicalMirai
{
    class ConsoleFormatTransfer
    {
        public static Paragraph ToParagraph(string s, Brush defaultForeground)
        {
            Paragraph p = new();
            p.Foreground = defaultForeground;
            AppendTo(p, s);
            return p;
        }
        public static void AppendTo(TextBlock tb, string s)
        {
            AppendTo(tb.Inlines, s, tb.Foreground);
        }
        public static void AppendTo(Paragraph p, string s)
        {
            AppendTo(p.Inlines, s, p.Foreground);
        }

        public static void AppendTo(InlineCollection inlines, string s, Brush defaultForeground)
        {
            Regex regex = new Regex("\u001b\\[[0-9a-zA-Z;]*?m");
            List<string> control = new();
            string[] split = regex.Split(s);
            Match match = regex.Match(s);
            while (match.Success)
            {
                control.Add(match.Value);
                match = match.NextMatch();
            }
            Dictionary<string, string> dict_color = Config.Instance.dict_color;
            for (int i = 0; i <= split.Length; i++)
            {
                Brush color = defaultForeground;
                if (i % 2 == 1 && control.Count > 0)
                {
                    string ctrl = control[0].Substring(1);
                    control.RemoveAt(0);
                    if (dict_color.ContainsKey(ctrl))
                    {
                        color = App.hexBrush(dict_color[ctrl]);
                    }
                }
                if (i >= split.Length) continue;
                string str = split[i];
                if (str.Length == 0) continue;
                inlines.Add(new Run(str) { Foreground = color });
            }
        }
    }
}
