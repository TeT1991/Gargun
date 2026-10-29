using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class InventoryProcessor : IDisposable
{
    private readonly int _maxSlotsCountOnPage;
    private readonly Dictionary<CardSlotUIView, Card> _slots;
    private readonly CardSlotsUIViewCreator _cardSlotsUIViewCreator;

    private Card[] _cardsInInventory;
    private List<Card> _aviableCardsInInventory;
    private int _currentPageIndex;

    public event Action<int> PagesCountChanged;
    public event Action<Card> CardSelected;

    public InventoryProcessor(CardSlotUIView cardSlotPrefab, RectTransform cardSlotsRoot, int maxSlotsCountOnPage)
    {
        _maxSlotsCountOnPage = maxSlotsCountOnPage;
        _slots = new Dictionary<CardSlotUIView, Card>();
        _cardSlotsUIViewCreator = new CardSlotsUIViewCreator(cardSlotPrefab, cardSlotsRoot);

        CreateCards();
    }

    public void AddCard(Card card)
    {
        int previousPagesCount = CalculatePagesCount();

        _aviableCardsInInventory.Add(card);
        SetInventoryPage(_currentPageIndex);

        int pagesCount = CalculatePagesCount();

        if (pagesCount != previousPagesCount)
        {
            PagesCountChanged?.Invoke(pagesCount);
        }
    }

    public void RemoveCard(Card card)
    {
        int previousPagesCount = CalculatePagesCount();

        _aviableCardsInInventory.Remove(card);
        SetInventoryPage(_currentPageIndex);

        int pagesCount = CalculatePagesCount();

        if (pagesCount != previousPagesCount)
        {
            PagesCountChanged?.Invoke(pagesCount);
        }
    }

    private void CreateCards()
    {
        string path = "CardsInfo";
        CardData[] cardsData = Resources.LoadAll<CardData>(path) ?? throw new Exception("Cant load or find Cards Data in folder");

        _cardsInInventory = new Card[cardsData.Length];

        for (int i = 0; i < cardsData.Length; i++)
        {
            CardData cardData = cardsData[i];
            _cardsInInventory[i] = new Card(cardData.Id, cardData.Name, cardData.Description, cardData.Sprite);
        }

        _aviableCardsInInventory = _cardsInInventory.ToList();
    }

    public void SetInventoryPage(int index)
    {
        _currentPageIndex = index;

        int firstCardIndex = CalculateFirstCardIndex(index);
        int cardsOnPage = Mathf.Clamp(_aviableCardsInInventory.Count - firstCardIndex, 0, _maxSlotsCountOnPage);

        ClearSlots();

        for (int i = firstCardIndex; i < firstCardIndex + cardsOnPage; i++)
        {
            CardSlotUIView slot = _cardSlotsUIViewCreator.GetSlotView();
            UnityAction clickHandler = () => OnSlotClicked(slot);
            slot.Button.onClick.AddListener(clickHandler);
            Card card = _aviableCardsInInventory[i];
            slot.SetInfo(card.Id.ToString(), card.Name, card.Description);
            _slots.Add(slot, card);
        }
    }

    private void ClearSlots()
    {
        foreach (KeyValuePair<CardSlotUIView, Card> pair in _slots)
        {
            pair.Key.Button.onClick.RemoveAllListeners();
            _cardSlotsUIViewCreator.ReleaseSlotView(pair.Key);
        }

        _slots.Clear();
    }

    private int CalculateFirstCardIndex(int index) 
    {
        return index * _maxSlotsCountOnPage;
    }

    public int CalculatePagesCount()
    {
        return Mathf.CeilToInt((float)_aviableCardsInInventory.Count / _maxSlotsCountOnPage);
    }

    private void OnSlotClicked(CardSlotUIView slot)
    {
        Card card = _slots[slot];
        CardSelected?.Invoke(card);
    }

    public void Dispose()
    {
        ClearSlots();
    }
}
