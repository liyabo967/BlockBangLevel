using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class LocalLanguage : MonoBehaviour
    {
        [SerializeField] private TextAsset languageAsset;

        private static readonly string[] ColumnSplitSeparator = new string[] { "\t" };
        public static LocalLanguage Instance;

        private Dictionary<string, string> _dictionary = new();

        private void Awake()
        {
            Instance = this;
            if (languageAsset)
            {
                ParseData(languageAsset.text);
            }
        }

        private Dictionary<SystemLanguage, int> ParseLanguageIndex(string dictionaryLineString)
        {
            Dictionary<SystemLanguage, int> languageDictionary = new();
            string[] languages = dictionaryLineString.Split(ColumnSplitSeparator, StringSplitOptions.None);
            for (int i = 2; i < languages.Length; i++)
            {
                if (Enum.TryParse<SystemLanguage>(languages[i], true, out SystemLanguage language))
                {
                    languageDictionary[language] = i;
                }
            }

            return languageDictionary;
        }
        
        private bool ParseData(string dictionaryString)
        {
            try
            {
                var currentLanguage = Application.systemLanguage;
                // currentLanguage = SystemLanguage.Hindi;
                int position = 0;
                string dictionaryLineString = null;
                int line = -1;
                Dictionary<SystemLanguage, int> languageDictionary = new();
                while ((dictionaryLineString = dictionaryString.ReadLine(ref position)) != null)
                {
                    line++;
                    if (dictionaryLineString[0] == '#')
                    {
                        if (line == 1)
                        {
                            languageDictionary =  ParseLanguageIndex(dictionaryLineString);
                        }
                        continue;
                    }

                    string[] splitedLine = dictionaryLineString.Split(ColumnSplitSeparator, StringSplitOptions.None);
                    // if (splitedLine.Length != ColumnCount)
                    // {
                    //     Log.Warning("Can not parse dictionary line string '{0}' which column count is invalid.", dictionaryLineString);
                    //     return false;
                    // }

                    int valueKey = languageDictionary.TryGetValue(currentLanguage, out valueKey) ? valueKey : 4;
                    string dictionaryKey = splitedLine[1];
                    string dictionaryValue = splitedLine[valueKey];
                    if (!AddString(dictionaryKey, dictionaryValue))
                    {
                        Debug.LogError($"Can not add key '{dictionaryKey}', which may be invalid or duplicate.");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse dictionary string with exception '{0}'.", exception);
                return false;
            }
        }

        private bool AddString(string key, string value)
        {
            if (_dictionary.ContainsKey(key))
            {
                return false;
            }

            _dictionary.Add(key, value ?? string.Empty);
            return true;
        }

        public string GetString(string key)
        {
            return _dictionary.GetValueOrDefault(key, key);
        }
    }
}