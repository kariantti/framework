﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Utilities;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using Signum.Utilities.NaturalLanguage;

namespace Signum.Utilities
{
    public static class NaturalLanguageTools
    {
        public static Dictionary<string, IPluralizer> Pluralizers = new Dictionary<string, IPluralizer>
        {
            {"es", new SpanishPluralizer()},
            {"en", new EnglishPluralizer()},
        };

        public static Dictionary<string, IGenderDetector> GenderDetectors = new Dictionary<string, IGenderDetector>
        {
            {"es", new SpanishGenderDetector()},
        };

        public static Dictionary<string, INumberWriter> NumberWriters = new Dictionary<string, INumberWriter>
        {
            {"es", new SpanishNumberWriter()},
        };

        public static char? GetGender(string name, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            IGenderDetector detector = GenderDetectors.TryGetC(culture.TwoLetterISOLanguageName);

            if (detector == null)
                return null;

            return detector.GetGender(name);
        }
        
        public static bool HasGenders(CultureInfo cultureInfo)
        {
            IGenderDetector detector = GenderDetectors.TryGetC(cultureInfo.TwoLetterISOLanguageName);

            if (detector == null)
                return false;

            return !detector.Pronoms.IsNullOrEmpty();
        }

        public static string GetPronom(char gender, bool plural, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            IGenderDetector detector = GenderDetectors.TryGetC(culture.TwoLetterISOLanguageName);
            if (detector == null)
                return null;

            var pro = detector.Pronoms.FirstOrDefault(a => a.Gender == gender);

            if (pro == null)
                return null;

            return plural ? pro.Plural : pro.Singular;
        }

        public static string Pluralize(string singularName, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            IPluralizer pluralizer = Pluralizers.TryGetC(culture.TwoLetterISOLanguageName);
            if (pluralizer == null)
                return singularName;

            return pluralizer.MakePlural(singularName);
        }


        public static string NiceName(this string memberName)
        {
            return memberName.Contains('_') ?
              memberName.Replace('_', ' ') :
              memberName.SpacePascal();
        }

        public static string SpacePascal(this string pascalStr, CultureInfo culture = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            return SpacePascal(pascalStr, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en");
        }

        public static string SpacePascal(this string pascalStr, bool preserveUppercase)
        {
            if (string.IsNullOrEmpty(pascalStr))
                return pascalStr;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < pascalStr.Length; i++)
            {
                switch (Kind(pascalStr, i))
                {
                    case CharKind.Lowecase:
                        sb.Append(pascalStr[i]);
                        break;

                    case CharKind.StartOfWord:
                        sb.Append(" ");
                        sb.Append(preserveUppercase ? pascalStr[i] : char.ToLower(pascalStr[i]));
                        break;

                    case CharKind.StartOfSentence:
                        sb.Append(pascalStr[i]);
                        break;

                    case CharKind.Abbreviation:
                        sb.Append(pascalStr[i]);
                        break;

                    case CharKind.StartOfAbreviation:
                        sb.Append(" ");
                        sb.Append(pascalStr[i]);
                        break;
                    default:
                        break;
                }
            }

            return sb.ToString();
        }

        static CharKind Kind(string pascalStr, int i)
        {
            if (i == 0)
                return CharKind.StartOfSentence;

            if (!char.IsUpper(pascalStr[i]))
                return CharKind.Lowecase;

            if (i + 1 == pascalStr.Length)
            {
                if (char.IsUpper(pascalStr[i - 1]))
                    return CharKind.Abbreviation;

                return CharKind.StartOfWord;
            }

            if (!char.IsUpper(pascalStr[i + 1]))
                return CharKind.StartOfWord;  // Xb

            if (!char.IsUpper(pascalStr[i - 1]))
            {
                if (i + 2 == pascalStr.Length)
                    return CharKind.StartOfAbreviation; //aXB|

                if (!char.IsUpper(pascalStr[i + 2]))
                    return CharKind.StartOfWord; //aXBc

                return CharKind.StartOfAbreviation; //aXBC
            }

            return CharKind.Abbreviation; //AXB
        }

        public enum CharKind
        {
            Lowecase,
            StartOfWord,
            StartOfSentence,
            StartOfAbreviation,
            Abbreviation,
        }

        public static string ToPascal(this string str, bool firstUpper)
        {
            StringBuilder sb = new StringBuilder(str.Length);

            bool upper = true;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!char.IsLetter(c) && !char.IsNumber(c))
                    upper = true;
                else
                {
                    sb.Append(upper ? char.ToUpper(c) : char.ToLower(c));

                    if (char.IsLetter(c))
                        upper = false;
                }
            }

            return sb.ToString();
        }

        /// <param name="genderAwareText">Se ha[n] encontrado [1m:un|1f:una|m:unos|f:unas] {0} eliminad[1m:o|1f:a|m:os|f:as]</param>
        /// <param name="gender">Masculine, Femenine, Neutrum, Inanimate, Animate</param>
        public static string ForGenderAndNumber(this string genderAwareText, char? gender = null, int? number = null)
        {
            if (gender == null && number == null)
                return genderAwareText;

            if (number == null)
                return GetPart(genderAwareText, gender + ":");

            if (gender == null)
            {
                if (number.Value == 1)
                    return GetPart(genderAwareText, "1:");

                return GetPart(genderAwareText, number.Value + ":", "");
            }

            if (number.Value == 1)
                return GetPart(genderAwareText, "1" + gender.Value + ":", "1:");

            return GetPart(genderAwareText, gender.Value + number.Value + ":", gender.Value + ":", number.Value + ":", "");
        }

        static string GetPart(string textToReplace, params string[] prefixes)
        {
            return Regex.Replace(textToReplace,
              @"\[(?<part>[^\|\]]+)(\|(?<part>[^\|\]]+))*\]", m =>
              {
                  var captures = m.Groups["part"].Captures.OfType<Capture>();

                  foreach (var pr in prefixes)
                  {
                      Capture capture = captures.FirstOrDefault(c => c.Value.StartsWith(pr));
                      if (capture != null)
                          return capture.Value.RemoveStart(pr.Length);
                  }

                  return "";
              });
        }


    }

    public interface IPluralizer
    {
        string MakePlural(string singularName);
    }

    public interface IGenderDetector
    {
        char? GetGender(string name);

        ReadOnlyCollection<PronomInfo> Pronoms { get; }
    }

    public class PronomInfo
    {
        public char Gender { get; private set; }
        public string Singular { get; private set; }
        public string Plural { get; private set; }

        public PronomInfo(char gender, string singular, string plural)
        {
            this.Gender = gender;
            this.Singular = singular;
            this.Plural = plural;
        }
    }

    public interface INumberWriter
    {
        string ToNumber(decimal number, NumberWriterSettings settings);
    }

    public class NumberWriterSettings
    {
        public string Unit;
        public string UnitPlural;
        public char? UnitGender;

        public string DecimalUnit;
        public string DecimalUnitPlural; 
        public char? DecimalUnitGender;

        public int NumberOfDecimals;
        public bool OmitDecimalZeros; 
    }
}
