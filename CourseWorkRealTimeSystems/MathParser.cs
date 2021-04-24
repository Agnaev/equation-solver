using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CourseWorkRealTimeSystems
{
    class MathParser
    {
        public static double Parse<T>(string equation, params Argument<T>[] args)
        {
            foreach (Argument<T> item in args)
            {
                equation = equation.Replace(
                    oldValue: item.name, 
                    newValue: item.value.ToString().Replace(',', '.')
                );
            }
            return Parse(
                equation: equation
            );
        }

        public static double Parse(string equation)
        {
            // Парсинг функций
            string[] func = { "sin", "cos", "ctan", "tan" };
            for (int i = 0; i < func.Length; i++)
            {
                Match matchFunc = Regex.Match(equation, string.Format(@"{0}\(({1})\)", func[i], @"[1234567890\.\+\-\*\/^%]*"));
                if (matchFunc.Groups.Count > 1)
                {
                    string inner = matchFunc.Groups[0].Value.Substring(1 + func[i].Length, matchFunc.Groups[0].Value.Trim().Length - 2 - func[i].Length);
                    string left = equation.Substring(0, matchFunc.Index);
                    string right = equation.Substring(matchFunc.Index + matchFunc.Length);

                    return i switch
                    {
                        0 => Parse(left + Math.Sin(Parse(inner)) + right),
                        1 => Parse(left + Math.Cos(Parse(inner)) + right),
                        2 => Parse(left + Math.Tan(Parse(inner)) + right),
                        3 => Parse(left + 1.0 / Math.Tan(Parse(inner)) + right),
                        _ => throw new Exception("Unexpected function " + func[i])
                    };
                }
            }

            // Парсинг скобок
            Match matchSk = Regex.Match(equation, string.Format(@"\(({0})\)", @"[1234567890\.\+\-\*\/^%]*"));
            if (matchSk.Groups.Count > 1)
            {
                string inner = matchSk.Groups[0].Value.Substring(1, matchSk.Groups[0].Value.Trim().Length - 2);
                string left = equation.Substring(0, matchSk.Index);
                string right = equation.Substring(matchSk.Index + matchSk.Length);
                return Parse(left + Parse(inner) + right);
            }

            // Парсинг действий
            Match matchMulOp = Regex.Match(equation, string.Format(@"({0})\s?({1})\s?({0})\s?", RegexNum, RegexMulOp));
            Match matchAddOp = Regex.Match(equation, string.Format(@"({0})\s?({1})\s?({2})\s?", RegexNum, RegexAddOp, RegexNum));
            var match = (matchMulOp.Groups.Count > 1) ? matchMulOp : (matchAddOp.Groups.Count > 1) ? matchAddOp : null;
            if (match != null)
            {
                string left = equation.Substring(0, match.Index);
                string right = equation.Substring(match.Index + match.Length);
                string val = ParseAct(match).ToString(CultureInfo.InvariantCulture);
                return Parse(string.Format("{0}{1}{2}", left, val, right));
            }

            // Парсинг числа
            try
            {
                return double.Parse(equation, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new FormatException(string.Format("Неверная входная строка '{0}'", equation));
            }
        }

        private const string RegexNum = @"[-]?\d+\.?\d*";
        private const string RegexMulOp = @"[\*\/^%]";
        private const string RegexAddOp = @"[\+\-]";

        private static double ParseAct(Match match)
        {
            double a = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            double b = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            return match.Groups[2].Value switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => a / b,
                "^" => Math.Pow(a, b),
                "%" => a % b,
                _ => throw new FormatException(string.Format("Неверная входная строка '{0}'", match.Value)),
            };
        }
    }

    public class Argument<T>
    {
        public Argument(string name, T value)
        {
            this.name = name;
            this.value = value;
        }
        public Argument(char name, T value): this(name.ToString(), value) {}
        public string name { get; set; }
        public T value { get; set; }
    }
}
