using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DecksProcessor
{
    private List<Deck> _decks;
    private Dictionary<DeckSlot, Card> _slots;
    private int _currentDeckIndex;

    private Button _createDeckButton;
    private Button _removeDeckButton;
    private DeckSlot _deckSlotPrefab;
    private RectTransform _slotsRoot;

    public event Action DeckAdded;
    public event Action DeckRemoved;
    public event Action<Card> CardSelected;

    public DecksProcessor(Button createDeckButton, Button removeDeckButton, DeckSlot deckSlotPrefab, RectTransform root)
    {
        _createDeckButton = createDeckButton;
        _removeDeckButton = removeDeckButton;
        _deckSlotPrefab = deckSlotPrefab;
        _slotsRoot = root;

        _decks = new List<Deck>();
        _slots = new Dictionary<DeckSlot, Card>();
        _currentDeckIndex = 0;

        _createDeckButton.onClick.AddListener(AddDeck);
        _removeDeckButton.onClick.AddListener(RemoveDeck);

        AddDeck();
        SetPage();
    }

    public void SwitchDeck(int index)
    {
        _currentDeckIndex = index;
    }

    public void ClearDeck()
    {
        foreach (KeyValuePair<DeckSlot, Card> pair in _slots)
        {
            DeckSlot slot = pair.Key;
            MonoBehaviour.Destroy(slot.gameObject);
        }

        _slots.Clear();
    }

    public void AddDeck()
    {
        _decks.Add(new Deck());
        DeckAdded?.Invoke();
    }

    public void RemoveDeck()
    {
        _decks.RemoveAt(_currentDeckIndex);
        DeckRemoved?.Invoke();
    }

    public void AddCard(Card card)
    {
        DeckSlot slot = CreateSlot();
        slot.SetLabelText(card.Name);
        UnityAction clickHandler = () => OnSlotClicked(slot);
        slot.Button.onClick.AddListener(clickHandler);
        _slots.Add(slot, card);
    }

    public void RemoveCard(Card card)
    {
        DeckSlot slot = _slots.FirstOrDefault(pair => pair.Value == card).Key;

        if (slot != null)
        {
            _slots.Remove(slot);
        }

        MonoBehaviour.Destroy(slot.gameObject);
    }

    public void SetPage()
    {
        Deck currentDeck = _decks[_currentDeckIndex];

        for (int i = 0; i < currentDeck.CardsCount; i++)
        {
            AddCard(currentDeck.GetCard(i));
        }
    }

    public DeckSlot CreateSlot()
    {
        return MonoBehaviour.Instantiate(_deckSlotPrefab, _slotsRoot);
    }

    public void OnSlotClicked(DeckSlot slot)
    {
        Card card = _slots[slot];
        CardSelected?.Invoke(card);
    }
}
