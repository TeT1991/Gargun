using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PageSwitcher : IDisposable
{
    private int _pagesCount;
    private readonly Button _nextPageButton;
    private readonly Button _previousButton;
    private readonly RectTransform _pageButtonsRoot;

    private readonly List<PageButtonUIVIew> _pageButtons;
    private readonly PageButtonUIVIew _pageButtonPrefab;

    private readonly List<(PageButtonUIVIew view, UnityAction action)> _listeners = new();
    private readonly UnityAction _nextPageDelegate;
    private readonly UnityAction _previousDelegate;

    private int _currentPageIndex;

    public event Action<int> CurrentPageChanged;

    public PageSwitcher(int currentPageIndex,int pagesCount, Button nextPageButton, Button previousButton, RectTransform pageButtonsRoot, PageButtonUIVIew pageButtonPrefab)
    {
        _pageButtons = new List<PageButtonUIVIew>();

        _pagesCount = Mathf.Max(0, pagesCount);
        _currentPageIndex = Mathf.Clamp(currentPageIndex, 0, Mathf.Max(0, _pagesCount - 1));


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

    public void SetPagesCount(int pagesCount)
    {
        int newPagesCount = Mathf.Max(0, pagesCount);

        if (_pagesCount == newPagesCount)
        {
            return;
        }

        int previousPageIndex = _currentPageIndex;
        _pagesCount = newPagesCount;
        _currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, Mathf.Max(0, _pagesCount - 1));

        for (int i = _pageButtons.Count - 1; i >= _pagesCount; i--)
        {
            PageButtonUIVIew pageButton = _pageButtons[i];
            pageButton.Button.onClick.RemoveListener(_listeners[i].action);
            pageButton.gameObject.SetActive(false);
            MonoBehaviour.Destroy(pageButton.gameObject);

            _pageButtons.RemoveAt(i);
            _listeners.RemoveAt(i);
        }

        CreatePages();
        MarkSelectedPage();
        HideSwitchButtons();

        if (_currentPageIndex != previousPageIndex)
        {
            CurrentPageChanged?.Invoke(_currentPageIndex);
        }
    }

    private void CreatePages()
    {
        for (int i = _pageButtons.Count; i < _pagesCount; i++)
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

        if (_pagesCount > 0)
        {
            _pageButtons[_currentPageIndex].MarkAsSelected();
        }
    }

    private void HideSwitchButtons()
    {
        _nextPageButton.gameObject.SetActive(_currentPageIndex < _pagesCount - 1);
        _previousButton.gameObject.SetActive(_currentPageIndex > 0);
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
