using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckBuilderProcessor : MonoBehaviour
{
    [SerializeField] private CardSlotUIView _cardSlotPrefab;
    [SerializeField] private RectTransform _cardSlotsRoot;
    [SerializeField] private int _maxSlotsCountOnPage = 8;

    private Card[] _cardsInInventory;
    private Dictionary<CardSlotUIView, Card> _pageSlots;

    private CardSlotsUIViewCreator _cardSlotsUIViewCreator;

    private void Awake()
    {
        _pageSlots = new Dictionary<CardSlotUIView, Card>();
        _cardSlotsUIViewCreator = new CardSlotsUIViewCreator(_cardSlotPrefab, _cardSlotsRoot);

        CreateCards();

        SetPage();
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

    private void SetPage()
    {
        int firstCardIndex = CalculateFirstCardIndex();
        int cardsOnPage = Mathf.Clamp(_cardsInInventory.Length - firstCardIndex, 0, 8);

        for (int i = firstCardIndex; i < cardsOnPage; i++)
        {
            CardSlotUIView slot = _cardSlotsUIViewCreator.GetSlotView();
            Card card = _cardsInInventory[i];
            slot.SetInfo(card.Id.ToString(), card.Name, card.Description);
        }
    }

    private int CalculateFirstCardIndex() //Временно. потом будем рассчитывать из текущей страницы
    {
        return 0;
    }
}