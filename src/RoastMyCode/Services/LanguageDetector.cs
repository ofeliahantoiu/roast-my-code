using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RoastMyCode.Services
{
    public class LanguageDetector
    {
        private static readonly Dictionary<string, List<(string Pattern, double Weight)>> LanguagePatterns = new()
        {
            { "csharp", new List<(string, double)>
                {
                    (@"\busing\b", 1.0), (@"\bnamespace\b", 1.0), (@"\bclass\b", 0.9), (@"\bvoid\b", 0.8),
                    (@"\bConsole\.Write(Line)?\b", 1.2), (@"\bList<", 1.1), (@"\bDictionary<", 1.1)
                }
            },

            { "javascript", new List<(string, double)>
                {
                    (@"\bfunction\b", 1.2), (@"\bconst\b", 1.0), (@"\blet\b", 1.0), (@"\bconsole\.log\b", 1.2),
                    (@"\b=>", 1.1), (@"\balert\b", 1.0), (@"\bimport\b", 0.8)
                }
            },

            { "html", new List<(string, double)>
                {
                    (@"<!DOCTYPE\s+html>", 2.0), (@"<html[^>]*>", 1.5), (@"<\w+[^>]*>", 1.0), (@"</\w+>", 0.8)
                }
            },

            { "css", new List<(string, double)>
                {
                    (@"\.[\w-]+\s*\{", 1.0), (@"#[\w-]+\s*\{", 1.0), (@"\bcolor\s*:", 1.0), (@"\bmargin\b", 0.9)
                }
            },

            { "python", new List<(string, double)>
                {
                    (@"\bdef\b", 1.5), (@"\bprint\b", 1.2), (@"\bself\b", 1.0), (@"__init__", 1.3),
                    (@"\bimport\b", 1.0)
                }
            },

            { "java", new List<(string, double)>
                {
                    (@"\bpublic\b", 1.0), (@"\bclass\b", 0.9), (@"System\.out\.print", 1.5),
                    (@"\bScanner\b", 1.2), (@"\bnew\b\s+[A-Z]\w*\(", 1.0)
                }
            },

            { "php", new List<(string, double)>
                {
                    (@"<\?php", 2.0), (@"\becho\b", 1.2), (@"\bvar_dump\b", 1.2), (@"\$[a-zA-Z_]\w*", 0.8)
                }
            },

            { "sql", new List<(string, double)>
                {
                    (@"\bSELECT\b", 2.0), (@"\bINSERT\b", 1.5), (@"\bUPDATE\b", 1.5), (@"\bJOIN\b", 1.2),
                    (@"\bWHERE\b", 1.3), (@"\bCREATE\b", 1.2), (@"\bDELETE\b", 1.4)
                }
            },

            { "typescript", new List<(string, double)>
                {
                    (@"\binterface\b", 1.3), (@"\btype\b", 1.2), (@"\bPromise\b", 1.2), (@"\breadonly\b", 1.0),
                    (@"\bimplements\b", 1.0), (@"\bconst\b", 0.9), (@"\bfunction\b", 0.9)
                }
            }
        };

        public static string DetectLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "text";

            if (LooksLikePlainText(code))
                return "text";

            var languageScores = new Dictionary<string, double>();

            foreach (var (language, patterns) in LanguagePatterns)
            {
                double score = 0;

                foreach (var (pattern, weight) in patterns)
                {
                    int matchCount = Regex.Matches(code, pattern, RegexOptions.IgnoreCase).Count;
                    score += matchCount * weight;
                }

                languageScores[language] = score;
            }

            var sorted = languageScores.OrderByDescending(x => x.Value).ToList();

            if (sorted[0].Value == 0 || (sorted.Count > 1 && sorted[0].Value - sorted[1].Value < 1.5))
                return "text";

            return sorted[0].Key;
        }

        private static bool LooksLikePlainText(string code)
        {
            bool noCodeSymbols = !Regex.IsMatch(code, @"[{}();<>]|class|function|def|console|print|echo", RegexOptions.IgnoreCase);
            bool manyWords = code.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 10;
            bool fewLines = code.Count(c => c == '\n') < 5;
            return noCodeSymbols && manyWords && fewLines;
        }
    }
}
