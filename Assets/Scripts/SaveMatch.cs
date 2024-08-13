using System;
using System.Collections.Generic;

public class SaveMatch
{
    public string currentBoard;
    public int currentScore;
    public int currentMatch;
    public int currentCombo;
    public List<Card> cards = new();

    [Serializable]
    public class Card
    {
        public string name;
        public int x;
        public int y;
        public bool isFaceUp;
    }
}
