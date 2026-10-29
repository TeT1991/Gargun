using System.Collections.Generic;

public class Deck
{
    public List<Card> _cards;

    public Deck()
    {
        _cards = new List<Card>();
    }

    public int CardsCount => _cards.Count;

    public Card GetCard(int index)
    {
        return _cards[index];
    }

    public void AddCard(Card card)
    {
        _cards.Add(card);
    }

    public void RemoveCard(Card card)
    {
        _cards.Remove(card);
    }
}