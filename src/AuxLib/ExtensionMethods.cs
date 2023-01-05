﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Math;

namespace audiamus.aux.ex
{
    public static class ExtDirInfo
    {
        public static void Clear(this DirectoryInfo di)
        {
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                try
                {
                    dir.Clear();
                    dir.Delete(true);
                }
                catch (IOException)
                {
                }
            }
            foreach (FileInfo file in di.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (IOException)
                {
                }
            }
        }
    }


    public static class ExInt
    {
        public static int Digits(this Int32 n) =>
              n == 0 ? 1 : 1 + (int)Math.Log10(Math.Abs(n));
        public static int Digits(this UInt32 n) =>
              n == 0 ? 1 : 1 + (int)Math.Log10(Math.Abs(n));
    }

    public static class ExDouble
    {
        public static double MinMax(this double value, double min, double max) =>
          Min(Max(value, min), max);
    }

    public static class ExNullable
    {
        public static bool IsNullOrWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s);
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> e) => e is null || e.Count() == 0;
        public static bool IsNull(this object o) => o is null;
    }

    public static class ExEnumerable
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (T item in items)
                action(item);
        }
    }

    public static class ExString
    {
        public const char SEPARATOR = ';';
        public static string Combine(this IEnumerable<string> values, char separator = SEPARATOR)
        {
            if (values is null)
                return null;
            var sb = new StringBuilder();
            foreach (string v in values)
            {
                if (string.IsNullOrWhiteSpace(v))
                    continue;
                if (sb.Length > 0)
                    sb.Append($"{separator} ");
                sb.Append(v);
            }
            return sb.ToString();
        }

        public static string[] SplitTrim(this string value, char separator) => value.SplitTrim(new[] { separator });

        public static string[] SplitTrim(this string value, char[] separators = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];
            if (separators is null)
                separators = new[] { ',', ';' };

            var values = value.Split(separators);
            values = values.Select(v => v.Trim()).ToArray();
            return values;
        }

        static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        static readonly char[] DoubtfulFileNameChars = {
      '¡', '¢', '£', '¤', '¥', '¦', '§', '¨', '©', 'ª', '«', '¬', '®', '¯', '°', '±',
      '²', '³', '´', 'µ', '¶', '·', '¸', '¹', 'º', '»', '¼', '½', '¾', '¿', '×', '÷',
      '‘', '’', 'ƒ', '„', '…', '†', '‡', 'ˆ', '‰', '‹', '’', '“', '”', '•', '–', '—',
      '˜', '™', '›'
    };

        public static string Prune(this string s, char[] invalid)
        {
            char[] doubtful = null;
            if (s is null)
                return null;
            if (invalid is null)
            {
                invalid = InvalidFileNameChars;
                doubtful = DoubtfulFileNameChars;
            }
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (invalid.Contains(c))
                    continue;
                //sb.Append (',');
                else if (doubtful?.Contains(c) ?? false)
                    continue;
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static string Prune(this string s)
        {
            if (s is null)
                return null;
            string pruned = s.Prune(null);
            pruned = pruned.Trim('.');
            return pruned;
        }

        public static string SubstitUser(this string s)
        {
            if (s is null)
                return null;
            string userdir = ApplEnv.UserDirectoryRoot;
            if (!s.Contains(userdir))
                return s;
            string userdir1 = userdir.Replace(ApplEnv.UserName, "USER");
            string s1 = s.Replace(userdir, userdir1);
            return s1;
        }

        const int MAXLEN_SHORTSTRING = 40;

        public static string Shorten(this string s, int maxlen = 0)
        {
            if (s is null)
                return null;
            if (maxlen == 0)
                maxlen = MAXLEN_SHORTSTRING;
            if (maxlen < 0 || s.Length <= maxlen)
                return s;

            int partLen1 = maxlen * 2 / 3;
            int partLen2 = maxlen - partLen1 - 1;

            int p2 = s.Length - partLen2;
            return s.Substring(0, partLen1).Trim() + '…' + s.Substring(p2).Trim();
        }


    }

    public static class ExTimeSpan
    {
        public static string ToStringHMS(this TimeSpan value)
        {
            string sgn = value < TimeSpan.Zero ? "-" : string.Empty;
            int hours = Abs(value.Days) * 24 + Abs(value.Hours);
            return $"{sgn}{Abs(hours):D2}:{Abs(value.Minutes):D2}:{Abs(value.Seconds):D2}";
        }
        public static string ToStringHMSm(this TimeSpan value) => $"{value.ToStringHMS()}.{Abs(value.Milliseconds):D3}";
    }

    public static class ExUnc
    {
        private const string UNC = @"UNC\";
        private const string UNC_PFX = @"\\?\";
        private const string UNC_NET = UNC_PFX + UNC;

        public static bool IsUnc(this string path)
        {
            string root = Path.GetPathRoot(path);

            if (root.StartsWith(UNC_PFX))
                return true;

            return false;
        }

        public static string AsUncIfLong(this string path)
        {
            if (path.IsUnc())
                return path;
            path = Path.GetFullPath(path);
            if (path.Length < 250)
                return path;
            return path.AsUnc();
        }

        public static string AsUnc(this string path)
        {
            if (path.IsUnc())
                return path;
            else
            {
                string root = Path.GetPathRoot(path);

                if (root.StartsWith(@"\\"))
                {
                    string s = path.Substring(2);
                    return UNC_NET + s;
                }
                else
                    return UNC_PFX + path;
            }
        }

        public static string StripUnc(this string path)
        {
            if (!path.IsUnc())
                return path;
            else
            {
                string root = Path.GetPathRoot(path);

                if (root.StartsWith(UNC_NET))
                {
                    string s = path.Substring(UNC_NET.Length);
                    return @"\\" + s;
                }
                else
                    return path.Substring(UNC_PFX.Length);
            }
        }
    }

    public static class ExException
    {
        public static string ToShortString(this Exception exc, bool withCRLF = false) =>
          $"{exc.GetType().Name}:{(withCRLF ? Environment.NewLine : " ")}\"{exc.Message.SubstitUser()}\"";
    }
}
