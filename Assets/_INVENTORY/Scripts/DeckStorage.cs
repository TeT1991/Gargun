using System;
using UnityEngine;

public class DeckStorage
{
    private const string DeckKeyPrefix = "deck_";
    private const string DeckIndexKey = "decks_index";

    private readonly Func<int, Card> _findCardById;

    public DeckStorage(Func<int, Card> findCardById)
    {
        _findCardById = findCardById;
    }

    public bool LoadDecks(Action<Deck> registerDeck)
    {
        if (PlayerPrefs.HasKey(DeckIndexKey) != true)
        {
            return false;
        }

        DeckIndex index = new();
        ReadIndex(index);

        foreach (string name in index.Names)
        {
            Deck deck = LoadDeck(name);

            if (deck != null)
            {
                registerDeck(deck);
            }
        }

        return true;
    }

    public bool ContainsDeck(string name)
    {
        return PlayerPrefs.HasKey(GetDeckKey(name));
    }

    public void Save(Deck deck, string previousName = null)
    {
        SavedDeck savedDeck = new()
        {
            Name = deck.Name,
            CardIds = new int[deck.CardsCount]
        };

        for (int i = 0; i < deck.CardsCount; i++)
        {
            savedDeck.CardIds[i] = deck.GetCard(i).Id;
        }

        DeckIndex index = new();
        ReadIndex(index);
        int position = index.Names.IndexOf(previousName ?? deck.Name);

        if (position >= 0)
        {
            index.Names[position] = deck.Name;
        }
        else
        {
            index.Names.Add(deck.Name);
        }

        string deckJson = JsonUtility.ToJson(savedDeck);
        string indexJson = JsonUtility.ToJson(index);
        PlayerPrefs.SetString(GetDeckKey(deck.Name), deckJson);
        PlayerPrefs.SetString(DeckIndexKey, indexJson);

        if (previousName != null && previousName != deck.Name)
        {
            PlayerPrefs.DeleteKey(GetDeckKey(previousName));
        }

        PlayerPrefs.Save();
    }

    public void Delete(string name)
    {
        DeckIndex index = new();
        ReadIndex(index);
        index.Names.Remove(name);

        PlayerPrefs.SetString(DeckIndexKey, JsonUtility.ToJson(index));
        PlayerPrefs.DeleteKey(GetDeckKey(name));
        PlayerPrefs.Save();
    }

    private Deck LoadDeck(string name)
    {
        string json = PlayerPrefs.GetString(GetDeckKey(name));
        SavedDeck savedDeck = JsonUtility.FromJson<SavedDeck>(json);

        if (savedDeck?.CardIds == null)
        {
            Debug.LogWarning($"Не удалось загрузить колоду «{name}».");
            return null;
        }

        Deck deck = new Deck();
        deck.Rename(name);

        foreach (int id in savedDeck.CardIds)
        {
            Card card = _findCardById(id);

            if (card == null)
            {
                Debug.LogWarning($"Колода «{name}»: не удалось найти единственную карту с ID {id}.");
                continue;
            }

            deck.AddCard(card);
        }

        return deck;
    }

    private static void ReadIndex(DeckIndex index)
    {
        string json = PlayerPrefs.GetString(DeckIndexKey);

        if (string.IsNullOrEmpty(json) != true)
        {
            JsonUtility.FromJsonOverwrite(json, index);
        }
    }

    private static string GetDeckKey(string name)
    {
        return DeckKeyPrefix + name;
    }
}
