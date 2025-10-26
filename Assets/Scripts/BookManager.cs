using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Pages;

public class BookManager : MonoBehaviour
{
    public NarrativeVariables narrativeVariables;
    public Pages pages;

    [SerializeField] private TextMeshProUGUI currentText;
    [SerializeField] private GameObject optionsToChose;

    [SerializeField] private GameObject nextPageButton;

    private Page currentPage;
    private int currentPageIndex;
    private int nextPageIndex; 

    private void OnEnable()
    {
        Page.OnOptionsToActive += ShowOptions;
    }

    private void OnDisable()
    {
        Page.OnOptionsToActive -= ShowOptions;
    }

    private void Start()
    {
        ResetOptions();
        Init();
    }

    private void Init()
    {
        currentPage = pages.GetPage(currentPageIndex);
        ShowNewText();
    }

    public void NextPage()
    {
        currentPageIndex = nextPageIndex; 
        currentPage = pages.GetPage(currentPageIndex);
        ShowNewText();
    }

    public void ShowNewText()
    {
        currentText.text = currentPage.ReplaceText(narrativeVariables.variables);
    }

    public void ShowOptions(List<string> options)
    {
        if(options.Count <= 0)
        {
            ShowChangePageButton(true);

            optionsToChose.gameObject.SetActive(false);
            return;
        }

        ShowChangePageButton(false);

        Button[] optionButtons = optionsToChose.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);

                TextMeshProUGUI textComp = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                if (textComp != null)
                    textComp.text = options[i];
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        optionsToChose.SetActive(true);
    }

    public void ChosenOption(TextMeshProUGUI optionText)
    {
        nextPageIndex = FindNextIndexPage(optionText.text); // Mantener esta linea la primera
        currentPage.SetChosenOption(optionText.text);

        ShowNewText();
        ShowChangePageButton(true);
    }

    private int FindNextIndexPage(string chosedOption)
    {
        var variable = currentPage.GetActiveVariable();
        int nextID = 0; 

        foreach (var option in variable.options.values)
        {
            if(option.value == chosedOption) nextID = option.nextPageID;
        }

        return nextID;
    }

    private void ShowChangePageButton(bool change)
    {
        nextPageButton.gameObject.SetActive(change);
    }

    private void ResetOptions()
    {
        foreach (var varibles in narrativeVariables.variables)
        {
            varibles.hasChosen = false;
        }
    }
}
