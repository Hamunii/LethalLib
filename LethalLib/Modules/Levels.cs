#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace LethalLib.Modules;

public class Levels
{

    [Flags]
    public enum LevelTypes
    {
        None = 1 << 0,
        ExperimentationLevel = 1 << 2,
        AssuranceLevel = 1 << 3,
        VowLevel = 1 << 4,
        OffenseLevel = 1 << 5,
        MarchLevel = 1 << 6,
        RendLevel = 1 << 7,
        DineLevel = 1 << 8,
        TitanLevel = 1 << 9,
        AdamanceLevel = 1 << 11,
        ArtificeLevel = 1 << 12,
        EmbrionLevel = 1 << 13,
        Vanilla = ExperimentationLevel | AssuranceLevel | VowLevel | OffenseLevel | MarchLevel | RendLevel | DineLevel | TitanLevel | AdamanceLevel | ArtificeLevel | EmbrionLevel,

        /// <summary>
        /// Only modded levels
        /// </summary>
        Modded = 1 << 10,

        /// <summary>
        /// This includes modded levels!
        /// Acts as a global override
        /// </summary>
        All = ~0
    }

    internal static class Compatibility
    {
        /*
        // The following code is from LLL, but is copied here because we need to use it
        // even when LLL is not installed, because LLL alters LE(C) moon names to be
        // usable in e.g. BepInEx configuration files by removing illegal characters.
        //
        // https://github.com/IAmBatby/LethalLevelLoader
        */

        // From LLL, class: ConfigHelper
        private const string illegalCharacters = ".,?!@#$%^&*()_+-=';:'\"";

        // From LLL, class: SpanExtensions (modified: removed 'this' from ReadOnlySpan<char> span)
        private static ReadOnlySpan<char> TrimStartToLetters(ReadOnlySpan<char> span)
        {
            var startIndex = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (char.IsLetter(span[i]))
                {
                    break;
                }

                startIndex++;
            }

            return span.Slice(startIndex);
        }

        // From LLL, class: ExtendedLevel (modified to take a string as input)
        private static string SkipToLetters(string planetName)
        {
            if (planetName == null)
                return string.Empty;
            
            var inputSpan = planetName.AsSpan();
            var trimmedSpan = TrimStartToLetters(inputSpan);

            if (inputSpan.Equals(trimmedSpan, StringComparison.Ordinal))
            {
                return planetName;
            }

            return trimmedSpan.ToString();
        }

        // From LLL, class: Extensions (modified: removed 'this' from string input)
        private static string StripSpecialCharacters(string input)
        {
            var stringBuilder = new StringBuilder();

            foreach (var chr in input)
            {
                if ((!illegalCharacters.Contains(chr) && char.IsLetterOrDigit(chr))
                    || chr == ' ')
                {
                    stringBuilder.Append(chr);
                }
            }

            if (input.Length == stringBuilder.Length)
                return input;

            return stringBuilder.ToString();
        }

        // Helper Method for LethalLib
        internal static string GetLLLNameOfLevel(string levelName)
        {
            // -> 10 Example
            var newName = StripSpecialCharacters(SkipToLetters(levelName));
            // -> Example
            if (!newName.EndsWith("Level"))
                newName += "Level";
            // -> ExampleLevel
            return newName;
        }

        // Helper Method for LethalLib
        internal static Dictionary<string, int> LLLifyLevelRarityDictionary(Dictionary<string, int> keyValuePairs)
        {
            // LethalLevelLoader changes LethalExpansion level names. By applying the LLL changes always,
            // we can make sure all enemies get added to their target levels whether or not LLL is installed.
            Dictionary<string, int> LLLifiedCustomLevelRarities = new();
            var clrKeys = keyValuePairs.Keys.ToList();
            var clrValues = keyValuePairs.Values.ToList();
            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                LLLifiedCustomLevelRarities.Add(GetLLLNameOfLevel(clrKeys[i]), clrValues[i]);
            }
            return LLLifiedCustomLevelRarities;
        }

        internal static string[]? LLLifyLevelArray(string[]? levels)
        {
            if (levels is null)
                return null;

            string[] newLevelsArray = new string[levels.Length];

            for (int i = 0; i < levels.Length; i++)
                newLevelsArray[i] = GetLLLNameOfLevel(levels[i]);

            return newLevelsArray;
        }
    }

    internal static bool TryGetRarityForLevel(string levelName, Dictionary<LevelTypes, int> vanillaDict, Dictionary<string, int>? moddedDict, out int rarity)
    {
        bool isVanilla = Enum.TryParse<LevelTypes>(levelName, out var levelEnum);

        if (!isVanilla)
            levelName = Compatibility.GetLLLNameOfLevel(levelName);

        if (isVanilla && vanillaDict.TryGetValue(levelEnum, out rarity))
            return true;

        else if (isVanilla && vanillaDict.TryGetValue(LevelTypes.Vanilla, out rarity))
            return true;

        else if (!isVanilla && moddedDict != null && moddedDict.TryGetValue(levelName, out rarity))
            return true;

        else if (!isVanilla && vanillaDict.TryGetValue(LevelTypes.Modded, out rarity))
            return true;

        else if (vanillaDict.TryGetValue(LevelTypes.All, out rarity))
            return true;

        return false;
    }
}