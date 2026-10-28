using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderProcessor : MonoBehaviour
{
    [SerializeField] private CardSlotUIView _cardSlotPrefab;
    [SerializeField] private RectTransform _cardSlotsRoot;
    [SerializeField] private int _maxSlotsCountOnPage = 8;

    [SerializeField] private RectTransform _pageButtonsRoot;
    [SerializeField] private PageButtonUIVIew _pageButtonPrefab;
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private Button _previousPageButton;

    private Card[] _cardsInInventory;
    private Dictionary<CardSlotUIView, Card> _pageSlots;

    private CardSlotsUIViewCreator _cardSlotsUIViewCreator;
    private PageSwitcher _pageSwitcher;

    private List<IDisposable> _disposables;

    private void Awake()
    {
        _pageSlots = new Dictionary<CardSlotUIView, Card>();
        _cardSlotsUIViewCreator = new CardSlotsUIViewCreator(_cardSlotPrefab, _cardSlotsRoot);

        CreateCards();

        int currentPage = 0;
        _pageSwitcher = new PageSwitcher(currentPage,CalculatePagesCount(), _nextPageButton, _previousPageButton, _pageButtonsRoot, _pageButtonPrefab);
        _pageSwitcher.CurrentPageChanged += SetPage;
        SetPage(0);

        _disposables = new List<IDisposable>();
        _disposables.Add(_pageSwitcher);
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
    }

    private void SetPage(int index)
    {
        int firstCardIndex = CalculateFirstCardIndex(index);
        int cardsOnPage = Mathf.Clamp(_cardsInInventory.Length - firstCardIndex, 0, _maxSlotsCountOnPage);

        ClearSlots();

        for (int i = firstCardIndex; i < firstCardIndex + cardsOnPage; i++)
        {
            CardSlotUIView slot = _cardSlotsUIViewCreator.GetSlotView();
            Card card = _cardsInInventory[i];
            slot.SetInfo(card.Id.ToString(), card.Name, card.Description);
            _pageSlots.Add(slot, card);
        }
    }

    private void ClearSlots()
    {
        foreach (KeyValuePair<CardSlotUIView, Card> pair in _pageSlots)
        {
            _cardSlotsUIViewCreator.ReleaseSlotView(pair.Key);
        }

        _pageSlots.Clear();
    }

    private int CalculateFirstCardIndex(int index) //Временно. потом будем рассчитывать из текущей страницы
    {
        return index * _maxSlotsCountOnPage;
    }

    private int CalculatePagesCount()
    {
        return Mathf.CeilToInt((float)_cardsInInventory.Length / 8);
    }

    private void OnDestroy()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}