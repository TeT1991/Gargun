using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PageSwitcher : IDisposable
{
    private int _currentPageIndex;
    private int _pagesCount;

    private Button _nextPageButton;
    private Button _previousButton;
    private RectTransform _pageButtonsRoot;

    private List<PageButtonUIVIew> _pageButtons;
    private PageButtonUIVIew _pageButtonPrefab;

    private readonly List<(PageButtonUIVIew view, UnityAction action)> _listeners = new();
    private UnityAction _nextPageDelegate;
    private UnityAction _previousDelegate;

    public event Action<int> CurrentPageChanged;

    public PageSwitcher(int pagesCount, Button nextPageButton, Button previousButton, RectTransform pageButtonsRoot, PageButtonUIVIew pageButtonPrefab)
    {
        _pageButtons = new List<PageButtonUIVIew>();

        _currentPageIndex = 0;
        _pagesCount = pagesCount;


        _nextPageButton = nextPageButton;
        _previousButton = previousButton;

        _pageButtonsRoot = pageButtonsRoot;
        _pageButtonPrefab = pageButtonPrefab;

        CreatePages();

        _nextPageDelegate = () => OnPageSwitchPressed(1);
        _previousDelegate = () => OnPageSwitchPressed(-1);

        _nextPageButton.onClick.AddListener(_nextPageDelegate);
        _previousButton.onClick.AddListener(_previousDelegate);

        MarkSelectedPage();
        HideSwitchButtons();
    }

    public void ChangePage(int index)
    {
        if (index >= 0 && index < _pagesCount)
        {
            if (index != _currentPageIndex)
            {
                _currentPageIndex = index;
                MarkSelectedPage();
                HideSwitchButtons();
                CurrentPageChanged?.Invoke(_currentPageIndex);
            }

        }
    }

    private void CreatePages()
    {
        for (int i = 0; i < _pagesCount; i++)
        {
            PageButtonUIVIew pageButton = MonoBehaviour.Instantiate(_pageButtonPrefab, _pageButtonsRoot);
            int pageIndex = i;
            pageButton.SetLabel((pageIndex + 1).ToString());

            UnityAction action = () => ChangePage(pageIndex);

            pageButton.Button.onClick.AddListener(action);

            _listeners.Add((pageButton, action));
            _pageButtons.Add(pageButton);
        }
    }

    private void MarkSelectedPage()
    {
        foreach (PageButtonUIVIew button in _pageButtons)
        {
            button.MarkAsUnselected();
        }

        _pageButtons[_currentPageIndex].MarkAsSelected();
    }

    private void HideSwitchButtons()
    {
        _nextPageButton.gameObject.SetActive(_currentPageIndex != _pagesCount - 1);
        _previousButton.gameObject.SetActive(_currentPageIndex != 0);
    }

    private void OnPageSwitchPressed(int direction)
    {
        ChangePage(_currentPageIndex + direction);
    }

    public void Dispose()
    {
        _nextPageButton.onClick.RemoveAllListeners();
        _previousButton.onClick.RemoveAllListeners();

        foreach (PageButtonUIVIew buttonUIVIew in _pageButtons)
        {
            buttonUIVIew.Button.onClick.RemoveAllListeners();
        }
    }
}
