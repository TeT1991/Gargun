using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CardIdValidator
{
    [MenuItem("Tools/Cards/Validate and Fix IDs")]
    private static void ValidateAndFixIds()
    {
        CardData[] cards = AssetDatabase.FindAssets("t:CardData", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.Ordinal)
            .ThenBy(path => path, StringComparer.Ordinal)
            .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<CardData>()
            .ToArray();

        HashSet<int> usedIds = new HashSet<int>();
        List<CardData> cardsToFix = new List<CardData>();

        foreach (CardData card in cards)
        {
            if (card.Id < 0 || usedIds.Add(card.Id) != true)
            {
                cardsToFix.Add(card);
            }
        }

        int nextId = 0;

        foreach (CardData card in cardsToFix)
        {
            while (usedIds.Contains(nextId))
            {
                nextId++;
            }

            using SerializedObject serializedCard = new SerializedObject(card);
            serializedCard.FindProperty("_id").intValue = nextId;
            serializedCard.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(card);

            usedIds.Add(nextId);
            nextId++;
        }

        Debug.Log($"Проверено карт: {cards.Length}. Исправлено ID: {cardsToFix.Count}.");
    }
}
