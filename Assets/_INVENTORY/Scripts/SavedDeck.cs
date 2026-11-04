using System;

public partial class DeckStorage
{
    [Serializable]
    private class SavedDeck
    {
        public string Name;
        public int[] CardIds;
    }
}
