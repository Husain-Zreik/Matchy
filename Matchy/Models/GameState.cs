using System;

namespace Matchy.Models
{

    public class GameState
    {
        public int Mistakes { get; set; }
        public int CorrectMatches { get; set; }
        public int ElapsedTime { get; set; }
        public int CurrentLevel { get; set; }
        public int LiveScore { get; set; }
        public string PlayerName { get; set; }
        public int TotalCardPairs { get; set; }
        public int TotalCards { get; set; }
        public string[] CardValues { get; set; }
        public string[] ShuffledCardValues { get; set; }
        public string FirstClickedCard { get; set; }
        public int FirstClickedIndex { get; set; }
        public string SecondClickedCard { get; set; }
        public int SecondClickedIndex { get; set; }
        public bool IsGameActive { get; set; }
        public bool IsMemorizationPhase { get; set; }
        public DateTime GameStartTime { get; set; }

        public GameState()
        {
            Mistakes = 0;
            CorrectMatches = 0;
            ElapsedTime = 0;
            CurrentLevel = 1;
            LiveScore = 0;
            IsGameActive = false;
            IsMemorizationPhase = false;
        }
    }
}