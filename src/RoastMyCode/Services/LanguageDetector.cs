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
                    (@"<\?php", 2.0), (@"\becho\b", 1.5), (@"\bvar_dump\b", 1.5), (@"\$[a-zA-Z_]\w*", 1.2),
                    (@"\bfunction\s+\w+\s*\(", 1.4), (@"\bforeach\s*\(", 1.3), (@"->\w+\s*\(", 1.3),
                    (@"\b(include|require)(_once)?\b", 1.4), (@"\breturn\s+\$", 1.3), (@"=>", 1.2),
                    (@"\$_(GET|POST|SERVER|SESSION|COOKIE)", 1.5), (@"mysqli_", 1.4), (@"PDO", 1.4)
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
            },
            
            { "go", new List<(string, double)>
                {
                    (@"\bpackage\s+main\b", 2.0), (@"\bfunc\b", 1.5), (@"\bimport\s+\(", 1.3),
                    (@"\bfmt\.Print", 1.3), (@"\btype\s+\w+\s+struct\b", 1.3), (@"\bgo\s+func\b", 1.2),
                    (@"\bdefer\b", 1.2), (@"\bvar\b", 0.9), (@"\bconst\b", 0.9), (@"\binterface\{\}", 1.2)
                }
            },
            
            { "rust", new List<(string, double)>
                {
                    (@"\bfn\s+\w+\b", 2.0), (@"\blet\s+mut\b", 1.5), (@"\bRust\b", 1.3),
                    (@"\bmod\b", 1.3), (@"\buse\s+std::", 1.5), (@"\bstruct\b", 1.2), (@"\benum\b", 1.2),
                    (@"\bimpl\b", 1.4), (@"\bpub\b", 1.0), (@"::\b", 1.0), (@"\btrait\b", 1.3),
                    (@"\bResult<", 1.2), (@"\bOption<", 1.2), (@"\bunwrap\(\)", 1.4)
                }
            },
            
            { "ruby", new List<(string, double)>
                {
                    (@"\bdef\s+\w+\b", 1.5), (@"\bend\b", 1.3), (@"\bmodule\b", 1.3),
                    (@"\brequire\b", 1.2), (@"\battr_", 1.4), (@"\bdo\s+\|\w+\|\b", 1.5),
                    (@"\bclass\b\s+\w+\s*<\s*\w+", 1.5), (@"::new", 1.2), (@"\bnil\b", 1.2),
                    (@"\bputs\b", 1.3), (@"\.each\s+do\b", 1.4)
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
            
            // Debug scoring for troubleshooting issues with language detection
            Console.WriteLine($"Language Scores: {string.Join(", ", sorted.Select(x => $"{x.Key}={x.Value:F2}"))}");

            // If no matches at all, return text
            if (sorted[0].Value == 0)
                return "text";
                
            // If we have a clear winner with at least 2.0 points, return it regardless of the difference
            if (sorted[0].Value >= 2.0)
                return sorted[0].Key;
                
            // If the difference between top scores is small, be more cautious
            if (sorted.Count > 1 && sorted[0].Value - sorted[1].Value < 1.2)
                return "text";

            return sorted[0].Key;
        }

        private static bool LooksLikePlainText(string code)
        {
            // Extended pattern to catch common code constructs across multiple languages including Go, Rust, Ruby, and PHP
            bool noCodeSymbols = !Regex.IsMatch(code, @"[{}();<>:\[\]]|class|function|def|console|print|echo|func|fn|impl|trait|module|package|use|\blet\b|\bvar\b|\$[a-zA-Z_]|->|=>|::", RegexOptions.IgnoreCase);
            
            // Check if common language-specific patterns exist
            bool hasCommonCodePatterns = Regex.IsMatch(code, @"\bfunc\b|\bpackage\b|\bimport\b|\bfn\b|\blet\b|\bmod\b|\bimpl\b|\bdef\b|\bend\b|\bdo\b|\bclass\b|\bmodule\b|<\?php|\$[a-zA-Z_]\w*|\bpub\b|\bconst\b|\bstruct\b", RegexOptions.IgnoreCase);
            
            bool manyWords = code.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 10;
            bool fewLines = code.Count(c => c == '\n') < 5;
            
            // If it has common code patterns, it's definitely not plain text
            if (hasCommonCodePatterns) return false;
            
            return noCodeSymbols && manyWords && fewLines;
        }
    }
}
