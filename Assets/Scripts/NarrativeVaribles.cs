using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "NarrativeVariables", menuName = "Narrative/Narrative Variables")]
public class NarrativeVariables : ScriptableObject
{
    [System.Serializable]
    public class Variable
    {

        public string key;
        public bool hasChosen = false;
        public bool decision = false; 

        public OptionsToChose options;

        private string chosenOption; 

        public void Chosen(string _chosenOption) 
        {
            hasChosen = true; 
            chosenOption = _chosenOption;
        }    

        public string GetChosenOption()
        {
            return chosenOption;
        }
    }

    [System.Serializable]
    public class OptionsToChose
    {
        public ValuesToChose[] values;
    }

    [System.Serializable]
    public class ValuesToChose
    {
        public string value;
        public int nextPageID;
        public bool isDecisionAndQuest = false;
        public string questName;
    }

    public Variable[] variables;    
}
