using System;
using System.Collections.Generic;

public class Deck
{
    private readonly List<Card> _cards;
    private string _name = "Колода";

    public event Action<Deck, string> Changed;

    public Deck()
    {
        _cards = new List<Card>();
    }

    public string Name => _name;
    public int CardsCount => _cards.Count;

    public void Rename(string name)
    {
        if (_name == name)
        {
            return;
        }

        string previousName = _name;
        _name = name;
        Changed?.Invoke(this, previousName);
    }

    public Card GetCard(int index)
    {
        return _cards[index];
    }

    public void AddCard(Card card)
    {
        _cards.Add(card);
        Changed?.Invoke(this, null);
    }

    public void RemoveCard(Card card)
    {
        if (_cards.Remove(card))
        {
            Changed?.Invoke(this, null);
        }
    }
}
