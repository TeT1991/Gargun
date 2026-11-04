using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DecksProcessor : IDisposable
{
    private readonly DeckStorage _deckStorage;
    private List<Deck> _decks;
    private Dictionary<DeckSlot, Card> _slots;
    private int _currentDeckIndex;

    private Button _createDeckButton;
    private DeckSlot _deckSlotPrefab;
    private RectTransform _slotsRoot;

    public event Action DeckAdded;
    public event Action DeckRemoved;
    public event Action CurrentDeckChanged;
    public event Action CurrentDeckCardsChanged;
    public event Action<Card> CardSelected;

    public DecksProcessor(Button createDeckButton, DeckSlot deckSlotPrefab, RectTransform root, DeckStorage deckStorage)
    {
        _deckStorage = deckStorage;
        _createDeckButton = createDeckButton;
        _deckSlotPrefab = deckSlotPrefab;
        _slotsRoot = root;

        _decks = new List<Deck>();
        _slots = new Dictionary<DeckSlot, Card>();
        _currentDeckIndex = -1;

        _createDeckButton.onClick.AddListener(AddDeck);

        if (_deckStorage.LoadDecks(RegisterDeck))
        {
            SwitchDeck(0);
        }
        else
        {
            AddDeck();
        }
    }

    public int DecksCount => _decks.Count;
    public int CurrentDeckIndex => _currentDeckIndex;
    public Deck CurrentDeck => _currentDeckIndex >= 0 ? _decks[_currentDeckIndex] : null;

    public void SwitchDeck(int index)
    {
        if (index < 0 || index >= _decks.Count || index == _currentDeckIndex)
        {
            return;
        }

        _currentDeckIndex = index;
        SetPage();
        CurrentDeckChanged?.Invoke();
    }

    public void ClearDeck()
    {
        foreach (KeyValuePair<DeckSlot, Card> pair in _slots)
        {
            DeckSlot slot = pair.Key;
            slot.Button.onClick.RemoveAllListeners();
            slot.gameObject.SetActive(false);
            MonoBehaviour.Destroy(slot.gameObject);
        }

        _slots.Clear();
    }

    public void AddDeck()
    {
        Deck deck = new Deck();
        string defaultName = deck.Name;
        string name = defaultName;

        for (int suffix = 2; _deckStorage.ContainsDeck(name); suffix++)
        {
            name = $"{defaultName} {suffix}";
        }

        deck.Rename(name);
        _deckStorage.Save(deck);
        RegisterDeck(deck);

        _currentDeckIndex = _decks.Count - 1;
        SetPage();
        CurrentDeckChanged?.Invoke();
        DeckAdded?.Invoke();
    }

    public void RemoveDeck()
    {
        Deck deck = CurrentDeck;

        if (deck == null)
        {
            return;
        }

        _deckStorage.Delete(deck.Name);
        deck.Changed -= _deckStorage.Save;
        _decks.RemoveAt(_currentDeckIndex);
        _currentDeckIndex = _decks.Count == 0 ? -1 : Mathf.Min(_currentDeckIndex, _decks.Count - 1);
        SetPage();
        CurrentDeckChanged?.Invoke();
        DeckRemoved?.Invoke();
    }

    public bool TryRenameCurrentDeck(string name)
    {
        Deck deck = CurrentDeck;

        if (deck == null || string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (deck.Name == name)
        {
            return true;
        }

        if (_deckStorage.ContainsDeck(name))
        {
            return false;
        }

        deck.Rename(name);
        return true;
    }

    public void AddCard(Card card)
    {
        Deck currentDeck = CurrentDeck;

        if (currentDeck == null)
        {
            return;
        }

        currentDeck.AddCard(card);
        CreateCardSlot(card);
        CurrentDeckCardsChanged?.Invoke();
    }

    public void RemoveCard(Card card)
    {
        DeckSlot slot = _slots.FirstOrDefault(pair => pair.Value == card).Key;

        if (slot == null)
        {
            return;
        }

        CurrentDeck.RemoveCard(card);
        _slots.Remove(slot);
        slot.Button.onClick.RemoveAllListeners();
        slot.gameObject.SetActive(false);
        MonoBehaviour.Destroy(slot.gameObject);
        CurrentDeckCardsChanged?.Invoke();
    }

    public void SetPage()
    {
        ClearDeck();
        Deck currentDeck = CurrentDeck;

        if (currentDeck == null)
        {
            return;
        }

        for (int i = 0; i < currentDeck.CardsCount; i++)
        {
            CreateCardSlot(currentDeck.GetCard(i));
        }
    }

    private void RegisterDeck(Deck deck)
    {
        _decks.Add(deck);
        deck.Changed += _deckStorage.Save;
    }

    private void CreateCardSlot(Card card)
    {
        DeckSlot slot = MonoBehaviour.Instantiate(_deckSlotPrefab, _slotsRoot);
        slot.SetLabelText(card.Name);
        UnityAction clickHandler = () => OnSlotClicked(slot);
        slot.Button.onClick.AddListener(clickHandler);
        _slots.Add(slot, card);
    }

    private void OnSlotClicked(DeckSlot slot)
    {
        Card card = _slots[slot];
        CardSelected?.Invoke(card);
    }

    public void Dispose()
    {
        _createDeckButton.onClick.RemoveListener(AddDeck);

        foreach (Deck deck in _decks)
        {
            deck.Changed -= _deckStorage.Save;
        }

        ClearDeck();
    }
}
