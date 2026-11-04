using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderProcessor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _cardsInInventoryCout;
    [SerializeField] private CardSlotUIView _cardSlotPrefab;
    [SerializeField] private RectTransform _cardSlotsRoot;
    [SerializeField] private int _maxSlotsCountOnPage = 8;

    [SerializeField] private PageButtonUIVIew _pageButtonPrefab;

    [SerializeField] private RectTransform _inventoryPageButtonsRoot;
    [SerializeField] private Button _inventoryNextPageButton;
    [SerializeField] private Button _inventoryPreviousPageButton;

    [SerializeField] private TextMeshProUGUI _cardsInDeckCout;
    [SerializeField] private TMP_InputField _deckNameInput;
    [SerializeField, Min(1)] private int _cardsMaxCountInDeck = 32;
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
    private Color _cardsInDeckDefaultColor;

    private List<IDisposable> _disposables;

    private void Awake()
    {
        _cardsInDeckDefaultColor = _cardsInDeckCout.color;
        _inventoryProcessor = new InventoryProcessor(_cardSlotPrefab, _cardSlotsRoot, _maxSlotsCountOnPage);

        int currentPage = 0;
        _inventoryPageSwitcher = new PageSwitcher(currentPage, _inventoryProcessor.CalculatePagesCount(),
            _inventoryNextPageButton, _inventoryPreviousPageButton, _inventoryPageButtonsRoot, _pageButtonPrefab);
        _inventoryPageSwitcher.CurrentPageChanged += _inventoryProcessor.SetInventoryPage;
        _inventoryProcessor.PagesCountChanged += _inventoryPageSwitcher.SetPagesCount;
        _inventoryProcessor.CardsCountChanged += UpdateInventoryCardsCount;

        DeckStorage deckStorage = new DeckStorage(_inventoryProcessor.FindCardById);
        _deckProcessor = new DecksProcessor(_addDeckButton, _deckSlotPrefab, _deckSlotRoot, deckStorage);
        _deckPageSwitcher = new PageSwitcher(_deckProcessor.CurrentDeckIndex, _deckProcessor.DecksCount,
            _deckNextPageButton, _deckPreviousPageButton, _deckPageButtonsRoot, _pageButtonPrefab);
        _deckPageSwitcher.CurrentPageChanged += _deckProcessor.SwitchDeck;

        _inventoryProcessor.CardSelected += AddCardToDeck;
        _deckProcessor.CardSelected += RemoveCardFromDeck;
        _deckProcessor.DeckAdded += UpdateDeckPages;
        _deckProcessor.DeckRemoved += UpdateDeckPages;
        _deckProcessor.CurrentDeckChanged += UpdateAvailableCards;
        _deckProcessor.CurrentDeckChanged += UpdateDeckHeader;
        _deckProcessor.CurrentDeckCardsChanged += UpdateDeckCardsCount;
        _deckNameInput.onEndEdit.AddListener(RenameCurrentDeck);
        _removeDeckButton.onClick.AddListener(_deckProcessor.RemoveDeck);

        UpdateDeckPages();
        UpdateAvailableCards();
        UpdateDeckHeader();

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

        if (_inventoryProcessor.FindCardById(card.Id) != card)
        {
            Debug.LogWarning($"У карты «{card.Name}» некорректный или повторяющийся ID {card.Id}.");
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

    private void UpdateInventoryCardsCount(int availableCardsCount, int totalCardsCount)
    {
        _cardsInInventoryCout.text = $"{availableCardsCount}/{totalCardsCount}";
    }

    private void RenameCurrentDeck(string name)
    {
        if (_deckProcessor.TryRenameCurrentDeck(name) != true)
        {
            UpdateDeckHeader();
        }
    }

    private void UpdateDeckHeader()
    {
        Deck currentDeck = _deckProcessor.CurrentDeck;
        _deckNameInput.interactable = currentDeck != null;
        _deckNameInput.SetTextWithoutNotify(currentDeck != null ? currentDeck.Name : string.Empty);
        UpdateDeckCardsCount();
    }

    private void UpdateDeckCardsCount()
    {
        Deck currentDeck = _deckProcessor.CurrentDeck;
        int cardsCurrentCount = currentDeck != null ? currentDeck.CardsCount : 0;

        _cardsInDeckCout.text = $"{cardsCurrentCount}/{_cardsMaxCountInDeck}";
        _cardsInDeckCout.color = cardsCurrentCount < _cardsMaxCountInDeck ? Color.red : _cardsInDeckDefaultColor;
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
        _inventoryProcessor.CardsCountChanged -= UpdateInventoryCardsCount;
        _deckPageSwitcher.CurrentPageChanged -= _deckProcessor.SwitchDeck;
        _inventoryProcessor.CardSelected -= AddCardToDeck;
        _deckProcessor.CardSelected -= RemoveCardFromDeck;
        _deckProcessor.DeckAdded -= UpdateDeckPages;
        _deckProcessor.DeckRemoved -= UpdateDeckPages;
        _deckProcessor.CurrentDeckChanged -= UpdateAvailableCards;
        _deckProcessor.CurrentDeckChanged -= UpdateDeckHeader;
        _deckProcessor.CurrentDeckCardsChanged -= UpdateDeckCardsCount;
        _deckNameInput.onEndEdit.RemoveListener(RenameCurrentDeck);
        _removeDeckButton.onClick.RemoveListener(_deckProcessor.RemoveDeck);

        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
