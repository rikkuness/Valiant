/**********************************************************************************
* Blueprint Reality Inc. CONFIDENTIAL
* 2018 Blueprint Reality Inc.
* All Rights Reserved.
*
* NOTICE:  All information contained herein is, and remains, the property of
* Blueprint Reality Inc. and its suppliers, if any.  The intellectual and
* technical concepts contained herein are proprietary to Blueprint Reality Inc.
* and its suppliers and may be covered by Patents, pending patents, and are
* protected by trade secret or copyright law.
*
* Dissemination of this information or reproduction of this material is strictly
* forbidden unless prior written permission is obtained from Blueprint Reality Inc.
***********************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BlueprintReality.Text {
	public static class Localization {
        public const string CSV_RES_PATH = "BPR_Localization";

        public const string DEFAULT_LANG = "english";

        public const string NOT_FOUND_STR_FORMAT = "#{0}#";

        private static string curLanguage;
        public static string CurrentLanguage
        {
            get
            {
                if (string.IsNullOrEmpty(curLanguage))
                {
                    curLanguage = GetCheckedLanguage(curLanguage);
                }
                return curLanguage;
            }
            set
            {
                curLanguage = GetCheckedLanguage(value);
                if (LanguageChanged != null)
                    LanguageChanged();
            }
        }
        
        public static event System.Action LanguageChanged;

        private static Dictionary<string, Dictionary<string,string>> languageTable;
        public static List<string> GetSupportedLanguages()
        {
            if (languageTable == null)
                Initialize();

            return new List<string>(languageTable.Keys);
        }
        
        static void Initialize()
        {
            TextAsset locAsset = Resources.Load<TextAsset>(CSV_RES_PATH);
            if (locAsset == null)
                Debug.Log("No localization file found at " + CSV_RES_PATH);

            ParseLocalizationData(locAsset.text);
        }

        static void ParseLocalizationData(string text)
        {
            if (languageTable == null)
                languageTable = new Dictionary<string, Dictionary<string, string>>();

            StringReader reader = new StringReader(text);

            string firstCell;
            List<string> languages;
            string readerLine = reader.ReadLine();
            if( !ParseLine(readerLine, out firstCell, out languages) )
            {
                Debug.LogError("Couldn't parse header of localization");
                return;
            }

            foreach (string language in languages)
            {
                string newLang = language.ToLower();
                if (!languageTable.ContainsKey(newLang))
                    languageTable[newLang] = new Dictionary<string, string>();
            }

            int lineCount = 0;
            while (reader.Peek() > 0)
            {
                readerLine = reader.ReadLine();
                lineCount++;

                string key;
                List<string> vals;
                if (!ParseLine(readerLine, out key, out vals))
                {
                    Debug.LogError("Unmatched quotation marks in localization file on line " + lineCount);
                    continue;
                }

                int langIndex = 0;
                foreach (string lang in languageTable.Keys)
                    if( langIndex < vals.Count )
                        languageTable[lang][key] = vals[langIndex++];
            }
        }

        static bool ParseLine(string line, out string key, out List<string> vals)
        {
            List<int> commaPositions = new List<int>();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',' && inQuotes == false)
                    commaPositions.Add(i);
                else if (line[i] == '"' && (i == 0 || line[i - 1] != '\\'))
                    inQuotes = !inQuotes;
            }
            
            if( inQuotes )
            {
                key = null;
                vals = null;
                return false;
            }

            key = line
                .Substring(0, commaPositions[0])
                .Trim('"')
                .Trim();

            vals = new List<string>();

            if (key.Length == 0)
                return true;
   
            for (int i = 0; i < commaPositions.Count; i++)
            {
                int firstBreakIndex = commaPositions[i];

                int secondBreakIndex = i < commaPositions.Count - 1 ?
                    commaPositions[i + 1] :
                    line.Length;

                string val = line
                    .Substring(firstBreakIndex + 1, secondBreakIndex - firstBreakIndex - 1)
                    .Trim('"')
                    .Trim();

                vals.Add(val);
            }

            return true;
        }

        public static string Get(string key)
        {
            if (languageTable == null)
                Initialize();

            return Get(CurrentLanguage, key);
        }

        public static string Get(string lang, string key)
        {
            if (languageTable == null)
                Initialize();

            lang = lang.ToLower();
            Dictionary<string, string> valTable = languageTable[lang];
            if (valTable.ContainsKey(key))
                return valTable[key];
            else
                return string.Format(NOT_FOUND_STR_FORMAT, key);
        }

        public static Dictionary<string, string> GetAllLanguageTextFromKey(string key)
        {
            if (languageTable == null)
                Initialize();

            Dictionary<string, string> keyTable = new Dictionary<string, string>();

            foreach(var lang in GetSupportedLanguages())
            {
                Dictionary<string, string> valTable = languageTable[lang];

                if (valTable.ContainsKey(key))
                    keyTable.Add(lang, valTable[key]);
            }

            return keyTable;

        }

        private static string checkedLanguage = null;
        private static string GetCheckedLanguage(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                if (string.IsNullOrEmpty(checkedLanguage))
                {
                    checkedLanguage = Application.systemLanguage.ToString().ToLower();
                    if (checkedLanguage.Equals("chinese"))
                    {
                        checkedLanguage = "chinesesimplified";
                    }

                    Debug.Log("LOC: Current system language is " + checkedLanguage);

                    Debug.Log("LOC: Checking if " + checkedLanguage + " is supported");

                    if (!GetSupportedLanguages().Contains(checkedLanguage))
                    {
                        Debug.Log("LOC: " + checkedLanguage + " is not supported, reverting to " + DEFAULT_LANG);
                        checkedLanguage = DEFAULT_LANG;
                    }
                }
                language = checkedLanguage;
            }
            return language;
        }
	}
}
