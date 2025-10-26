using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using static NarrativeVariables;

[CreateAssetMenu(fileName = "Page", menuName = "Narrative/Page")]
public class Pages : ScriptableObject
{
    [System.Serializable]
    public class Page
    {
        public int id;
        public string text;
        public static Action<List<string>> OnOptionsToActive;

        private Variable variableActivated;

        public List<string> GetAllWordsBetweenBraces(string input)
        {
            string pattern = @"\{(.*?)\}";
            MatchCollection matches = Regex.Matches(input, pattern);

            List<string> results = new List<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value);
            }

            return results;
        }

        public string ReplaceText(Variable[] variables)
        {
            string newText = text;

            List<string> results = GetAllWordsBetweenBraces(text);
            List<string> options = new List<string>();

            if (results.Count <= 0)
            {
                OnOptionsToActive?.Invoke(options); // Enviamos la lista vacia para que se desactiven las opciones
                return text;
            }

            foreach (var variable in variables)
            {
                foreach (var word in results)
                {
                    if (variable.hasChosen && variable.key == ("{" + word + "}"))
                    {
                        newText = newText.Replace(variable.key, variable.GetChosenOption());
                        OnOptionsToActive?.Invoke(options); // Enviamos la lista vacia para que se desactiven las opciones
                    }
                    else if (variable.hasChosen == false && variable.key == ("{" + word + "}"))
                    {
                        newText = newText.Replace(variable.key, new string('_', word.Length));
                        variableActivated = variable;

                        foreach (var option in variable.options.values)
                        {
                            options.Add(option.value);
                        }

                        OnOptionsToActive?.Invoke(options);
                    }
                }
            }

            return newText;
        }

        public void SetChosenOption(string option)
        {
            variableActivated.Chosen(option);
            variableActivated = null;
        }

        public Variable GetActiveVariable()
        {
            return variableActivated;
        }
    }

    public Page[] pages;

    public Page GetPage(int id)
    {
        return pages[id];
    }
}
