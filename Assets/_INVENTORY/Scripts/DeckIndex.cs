using System;
using System.Collections.Generic;

public partial class DeckStorage
{
    [Serializable]
    private class DeckIndex //Временный класс, потом удалить. Нужен чтобы где-то хранился список досок из плеер перфс
    {
        public List<string> Names = new List<string>();
    }
}
