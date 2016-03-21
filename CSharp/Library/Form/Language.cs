﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    public class Language
    {
        public static HashSet<string> StopWords = new HashSet<string>()
        {"a", "about", "above", "above", "across", "after", "afterwards", "again", "against", "all", "almost", "alone", "along", "already", "also", "although", "always", "am", "among", "amongst", "amoungst", "amount", "an", "and", "another", "any", "anyhow", "anyone", "anything", "anyway", "anywhere", "are", "around", "as", "at", "back", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "behind", "being", "below", "beside", "besides", "between", "beyond", "bill", "both", "bottom", "but", "by", "call", "can", "cannot", "cant", "co", "con", "could", "couldnt", "cry", "de", "describe", "detail", "do", "done", "down", "due", "during", "each", "eg", "eight", "either", "eleven", "else", "elsewhere", "empty", "enough", "etc", "even", "ever", "every", "everyone", "everything", "everywhere", "except", "few", "fifteen", "fify", "fill", "find", "fire", "first", "five", "for", "former", "formerly", "forty", "found", "four", "from", "front", "full", "further", "get", "give", "go", "had", "has", "hasnt", "have", "he", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "him", "himself", "his", "how", "however", "hundred", "ie", "if", "in", "inc", "indeed", "interest", "into", "is", "it", "its", "itself", "keep", "last", "latter", "latterly", "least", "less", "ltd", "made", "many", "may", "me", "meanwhile", "might", "mill", "mine", "more", "moreover", "most", "mostly", "move", "much", "must", "my", "myself", "name", "namely", "neither", "never", "nevertheless", "next", "nine", "no", "nobody", "none", "noone", "nor", "not", "nothing", "now", "nowhere", "of", "off", "often", "on", "once", "one", "only", "onto", "or", "other", "others", "otherwise", "our", "ours", "ourselves", "out", "over", "own", "part", "per", "perhaps", "please", "put", "rather", "re", "same", "see", "seem", "seemed", "seeming", "seems", "serious", "several", "she", "should", "show", "side", "since", "sincere", "six", "sixty", "so", "some", "somehow", "someone", "something", "sometime", "sometimes", "somewhere", "still", "such", "system", "take", "ten", "than", "that", "the", "their", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "thereupon", "these", "they", "thick", "thin", "third", "this", "those", "though", "three", "through", "throughout", "thru", "thus", "to", "together", "too", "top", "toward", "towards", "twelve", "twenty", "two", "un", "under", "until", "up", "upon", "us", "very", "via", "was", "we", "well", "were", "what", "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whoever", "whole", "whom", "whose", "why", "will", "with", "within", "without", "would", "yet", "you", "your", "yours", "yourself", "yourselves"};

        public static HashSet<string> Articles = new HashSet<string>()
        { "a", "an", "the" };

        public static bool NonWord(string word)
        {
            bool nonWord = true;
            foreach (var ch in word)
            {
                if (!(char.IsControl(ch) || char.IsPunctuation(ch) || char.IsWhiteSpace(ch)))
                {
                    nonWord = false;
                    break;
                }
            }
            return nonWord;
        }

        public static bool NoiseWord(string word)
        {
            double number;
            bool noiseWord = double.TryParse(word, out number);
            if (!noiseWord) noiseWord = NonWord(word);
            if (!noiseWord) noiseWord = StopWords.Contains(word.ToLower());
            return noiseWord;
        }

        public static bool NoiseResponse(string word)
        {
            bool noiseWord = NonWord(word);
            if (!noiseWord) noiseWord = StopWords.Contains(word.ToLower());
            return noiseWord;
        }

        public static bool ArticleOrNone(string word)
        {
            return NonWord(word) || Articles.Contains(word);
        }

        public static bool Ignorable(IEnumerable<string> words)
        {
            return !words.Any((word) => !NoiseWord(word));
        }

        public static IEnumerable<string> NonNoiseWords(IEnumerable<string> words)
        {
            return from word in words where !NoiseResponse(word) select word;
        }

        public static Regex WordBreaker = new Regex(@"\w+", RegexOptions.Compiled);

        public static IEnumerable<string> WordBreak(string input)
        {
            foreach(Match match in WordBreaker.Matches(input))
            {
                yield return match.Value;
            }
        }

       public static string CamelCase(string original)
        {
            var builder = new StringBuilder();
            var name = original.Trim();
            var previousUpper = Char.IsUpper(name[0]);
            var previousLetter = Char.IsLetter(name[0]);
            bool first = true;
            for (int i = 0; i < name.Length; ++i)
            {
                var ch = name[i];
                if (!first && (ch == '_' || ch == ' '))
                {
                    // Non begin _ as space
                    builder.Append(' ');
                }
                else
                {
                    var isUpper = Char.IsUpper(ch);
                    var isLetter = Char.IsLetter(ch);
                    if ((!previousUpper && isUpper)
                        || (isLetter != previousLetter)
                        || (!first && isUpper && (i + 1) < name.Length && Char.IsLower(name[i+1])))
                    {
                        // Break on lower to upper, number boundaries and Upper to lower
                        builder.Append(' ');
                    }
                    previousUpper = isUpper;
                    previousLetter = isLetter;
                    builder.Append(ch);
                    if (first)
                    {
                        first = false;
                    }
                }
            }
            return builder.ToString();
        }

        public static IEnumerable<string> OptionalPlurals(IEnumerable<string> words)
        {
            foreach (var original in words)
            {
                var word = original.ToLower();
                var newWord = word;
                if (!NoiseWord(word) && word.Length > 1)
                {
                    newWord = (word.EndsWith("s") ? word + "?" : word + "s?");
                }
                yield return newWord;
            }
        }

        public static string[] GenerateTerms(string name, int maxLength)
        {
            var phrase = CamelCase(name);
            var words = (from word in phrase.Split(' ') select word.ToLower()).ToArray();
            var terms = new List<string>();
            for (var length = 1; length <= Math.Min(words.Length, maxLength); ++length)
            {
                for (var start = 0; start <= words.Length - length; ++start)
                {
                    var ngram = new ArraySegment<string>(words, start, length);
                    if (!ArticleOrNone(ngram.First()) && !ArticleOrNone(ngram.Last()))
                    {
                        terms.Add(string.Join(" ", OptionalPlurals(ngram)));
                    }
                }
            }
            if (words.Length > maxLength)
            {
                terms.Add(string.Join(" ", words));
            }
            return terms.ToArray();
        }

         private static Regex _aOrAn = new Regex(@"\b(a|an)(?:\s+)([aeiou])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static string ANormalization(string input)
        {
            var builder = new StringBuilder();
            var last = 0;
            foreach(Match match in _aOrAn.Matches(input))
            {
                var currentWord = match.Groups[1];
                builder.Append(input.Substring(last, currentWord.Index - last));
                if (match.Groups[2].Success)
                {
                    builder.Append("an");
                }
                else
                {
                    builder.Append("a");
                }
                last = currentWord.Index + currentWord.Length;
            }
            builder.Append(input.Substring(last));
            return builder.ToString();
        }

        public static string BuildList(IEnumerable<string> values, string separator, string lastSeparator)
        {
            var builder = new StringBuilder();
            var pos = 0;
            var end = values.Count() - 1;
            foreach (var elt in values)
            {
                if (pos > 0)
                {
                    builder.Append(pos == end ? lastSeparator : separator);
                }
                builder.Append(elt);
                ++pos;
            }
            return builder.ToString();
        }
    }
}