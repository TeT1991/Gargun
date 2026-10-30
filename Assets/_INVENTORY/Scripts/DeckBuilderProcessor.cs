using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderProcessor : MonoBehaviour
{
    [SerializeField] private CardSlotUIView _cardSlotPrefab;
    [SerializeField] private RectTransform _cardSlotsRoot;
    [SerializeField] private int _maxSlotsCountOnPage = 8;

    [SerializeField] private PageButtonUIVIew _pageButtonPrefab;

    [SerializeField] private RectTransform _inventoryPageButtonsRoot;
    [SerializeField] private Button _inventoryNextPageButton;
    [SerializeField] private Button _inventoryPreviousPageButton;

    [SerializeField] private RectTransform _deckPageButtonsRoot;
    [SerializeField] private RectTransform _deckSlotRoot;
    [SerializeField] private DeckSlot _deckSlotPrefab;
    [SerializeField] private Button _addDeckButton;
    [SerializeField] private Button _removeDeckButton;
    [SerializeField] private Button _deckNextPageButton;
    [SerializeField] private Button _deckPreviousPageButton;

    private InventoryProcessor _inventoryProcessor;
    private PageSwitcher _inventoryPageSwitcher;

    private DecksProcessor _deckProcessor;
    private PageSwitcher _deckPageSwitcher;

    private List<IDisposable> _disposables;

    private void Awake()
    {
        _inventoryProcessor = new InventoryProcessor(_cardSlotPrefab, _cardSlotsRoot, _maxSlotsCountOnPage);

        int currentPage = 0;
        _inventoryPageSwitcher = new PageSwitcher(currentPage, _inventoryProcessor.CalculatePagesCount(),
            _inventoryNextPageButton, _inventoryPreviousPageButton, _inventoryPageButtonsRoot, _pageButtonPrefab);
        _inventoryPageSwitcher.CurrentPageChanged += _inventoryProcessor.SetInventoryPage;
        _inventoryProcessor.PagesCountChanged += _inventoryPageSwitcher.SetPagesCount;

        _deckProcessor = new(_addDeckButton, _deckSlotPrefab, _deckSlotRoot);
        _deckPageSwitcher = new PageSwitcher(_deckProcessor.CurrentDeckIndex, _deckProcessor.DecksCount,
            _deckNextPageButton, _deckPreviousPageButton, _deckPageButtonsRoot, _pageButtonPrefab);
        _deckPageSwitcher.CurrentPageChanged += _deckProcessor.SwitchDeck;

        _inventoryProcessor.CardSelected += AddCardToDeck;
        _deckProcessor.CardSelected += RemoveCardFromDeck;
        _deckProcessor.DeckAdded += UpdateDeckPages;
        _deckProcessor.DeckRemoved += UpdateDeckPages;
        _deckProcessor.CurrentDeckChanged += UpdateAvailableCards;
        _removeDeckButton.onClick.AddListener(_deckProcessor.RemoveDeck);

        UpdateDeckPages();
        UpdateAvailableCards();

        _disposables = new List<IDisposable>();
        _disposables.Add(_inventoryPageSwitcher);
        _disposables.Add(_deckPageSwitcher);
        _disposables.Add(_inventoryProcessor);
        _disposables.Add(_deckProcessor);
    }

    public void AddCardToDeck(Card card)
    {
        if (_deckProcessor.DecksCount == 0)
        {
            return;
        }

        _inventoryProcessor.RemoveCard(card);
        _deckProcessor.AddCard(card);
    }

    public void RemoveCardFromDeck(Card card)
    {
        _deckProcessor.RemoveCard(card);
        _inventoryProcessor.AddCard(card);
    }

    private void UpdateAvailableCards()
    {
        _inventoryProcessor.UpdateAvailableCards(_deckProcessor.CurrentDeck);
    }

    private void UpdateDeckPages()
    {
        int decksCount = _deckProcessor.DecksCount;

        _removeDeckButton.gameObject.SetActive(decksCount > 0);
        _deckPageSwitcher.SetPagesCount(decksCount);

        if (decksCount > 0)
        {
            _deckPageSwitcher.ChangePage(_deckProcessor.CurrentDeckIndex);
        }
    }

    private void OnDestroy()
    {
        _inventoryPageSwitcher.CurrentPageChanged -= _inventoryProcessor.SetInventoryPage;
        _inventoryProcessor.PagesCountChanged -= _inventoryPageSwitcher.SetPagesCount;
        _deckPageSwitcher.CurrentPageChanged -= _deckProcessor.SwitchDeck;
        _inventoryProcessor.CardSelected -= AddCardToDeck;
        _deckProcessor.CardSelected -= RemoveCardFromDeck;
        _deckProcessor.DeckAdded -= UpdateDeckPages;
        _deckProcessor.DeckRemoved -= UpdateDeckPages;
        _deckProcessor.CurrentDeckChanged -= UpdateAvailableCards;
        _removeDeckButton.onClick.RemoveListener(_deckProcessor.RemoveDeck);

        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
