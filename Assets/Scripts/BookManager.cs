using NUnit.Framework;
using System;
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
    [SerializeField] private GameObject book;

    // Quest
    [SerializeField] private GameObject quest;
    [SerializeField] private TextMeshProUGUI questText;

    // Fade
    [SerializeField] private Image fadeImage;      // Fullscreen image used for fading (alpha 0–1)
    [SerializeField] private float fadeSpeed = 1f; // Higher = faster fade

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

   
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    private void Init()
    {
        currentPageIndex = 0;
        nextPageIndex = 1;
        currentPage = pages.GetPage(currentPageIndex);
        finalPageButton.SetActive(false);
        ShowNewText();
    }

   
    private IEnumerator FadeInOut(Action midAction)
    {
        yield return StartCoroutine(Fade(0f, 1f));

        midAction?.Invoke();

        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        Color color = fadeImage.color;
        float t = 0f;

        // Set initial alpha
        color.a = startAlpha;
        fadeImage.color = color;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
    }
    public void NextPage()
    {
        StartCoroutine(FadeInOut(NextPageLogic));
    }

    private void NextPageLogic()
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

        nextPageIndex = FindNextIndexPage(optionText.text); // Keep this line first
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
        // Sounds, animation, etc...
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
        StartCoroutine(FadeInOut(StartBookLogic));
    }

    private void StartBookLogic()
    {
        startBook.SetActive(false);
        book.SetActive(true);
    }

    public void FinishBook()
    {
        StartCoroutine(FadeInOut(FinishBookFade));
    }

    private void FinishBookFade()
    {
        ResetOptions();
        startBook.SetActive(true);
        book.SetActive(false);
        Init();
    }

    public void Exit()
    {
        Application.Quit();
    }

    private void ResetOptions()
    {
        foreach (var varibles in narrativeVariables.variables)
        {
            varibles.hasChosen = false;
        }
    }
}
