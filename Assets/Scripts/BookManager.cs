using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private GameObject finalPageButton;

    [SerializeField] private GameObject startBook;

    // Quest
    [SerializeField] private GameObject quest;
    [SerializeField] private TextMeshProUGUI questText;

    private Pages.Page currentPage;
    private int currentPageIndex;
    private int nextPageIndex;
    private NarrativeVariables.Variable currentVariable;

    private void OnEnable()
    {
        Pages.Page.OnOptionsToActive += ShowOptions;
    }

    private void OnDisable()
    {
        Pages.Page.OnOptionsToActive -= ShowOptions;
    }

    private void Start()
    {
        ResetOptions();
        Init();
    }

    private void Init()
    {
        currentPageIndex = 0;
        nextPageIndex = 1; 
        currentPage = pages.GetPage(currentPageIndex);
        finalPageButton.SetActive(false);   
        ShowNewText();
    }

    public void NextPage()
    {
        currentPageIndex = nextPageIndex;
        currentPage = pages.GetPage(currentPageIndex);

        if (currentPage.pageConnection.isAPageConnection)
        {
            nextPageIndex = currentPage.pageConnection.id;
        }
        else
        {
            nextPageIndex++;
            if (nextPageIndex >= pages.pages.Length - 1) nextPageIndex = pages.pages.Length - 1;
        }

        ShowNewText();

        //Debug.Log("Current Variable: " + currentVariable.key);
    }

    public void ShowNewText()
    {
        Debug.Log("ShowText");
        currentText.text = currentPage.ReplaceText(narrativeVariables.variables);

        if (currentPage.finalPage)
        {
            nextPageButton.SetActive(false);
            optionsToChose.SetActive(false);
            finalPageButton.SetActive(true);
        }
    }

    public void ShowOptions(List<string> options)
    {
        if (options.Count <= 0)
        {
            ShowNextPageButton(true);

            optionsToChose.gameObject.SetActive(false);
            return;
        }

        ShowNextPageButton(false);

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
        currentVariable = currentPage.GetActiveVariable();
        currentPage.SetChosenOption(optionText.text);

        if (currentVariable != null)
        {
            Debug.Log("Current Variable: " + currentVariable.key);
            Debug.Log("Current Variable Decision: " + currentVariable.decision);
            if (currentVariable.decision == false) ShowNewText();
            else optionsToChose.SetActive(false);
        }

        nextPageIndex = FindNextIndexPage(optionText.text); // Mantener esta linea la primera
        FindQuest(optionText.text);

        ShowNextPageButton(true);
    }

    private int FindNextIndexPage(string chosedOption)
    {
        var variable = currentVariable;
        int nextID = 0;

        foreach (var option in variable.options.values)
        {
            if (option.value == chosedOption) nextID = option.nextPageID;
        }

        return nextID;
    }

    private void FindQuest(string chosedOption)
    {
        var variable = currentVariable;

        if (!variable.decision) return;

        foreach (var option in variable.options.values)
        {
            if (option.value == chosedOption && option.isDecisionAndQuest) StartCoroutine(ShowQuest(option.questName));
        }
    }

    private IEnumerator ShowQuest(string questCompletedName)
    {
        // SONIDOS ANIMACION..
        questText.text = "Desafio completado: " + questCompletedName;
        quest.SetActive(true);

        yield return new WaitForSecondsRealtime(2.5f);

        quest.SetActive(false);
    }

    private void ShowNextPageButton(bool change)
    {
        nextPageButton.gameObject.SetActive(change);
    }

    public void StartBook()
    {
        startBook.SetActive(false);
    }

    public void FinishBook()
    {
        // Logica Del final 
        ResetOptions();
        startBook.SetActive(true);
        Init();
    }

    public void Exit()
    {
        //Application.Quit();
    }

    private void ResetOptions()
    {
        foreach (var varibles in narrativeVariables.variables)
        {
            varibles.hasChosen = false;
        }
    }
}
