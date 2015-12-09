using UnityEngine;

namespace VRStandardAssets.Common
{
    // This class is used to keep score during a game and save
    // the highscores to PlayerPrefs.
    public static class SessionData
    {
        // This enum shows all the types of games that use scores.
        public enum GameType
        {
            FLYER,
            SHOOTER180,
            SHOOTER360
        };


        private const string k_FlyerData = "flyerData";             // These are the names given to PlayerPrefs based on game type.
        private const string k_Shooter180 = "shooter180Data";
        private const string k_Shooter360 = "shooter360Data";


        private static int s_HighScore;                             // Used to store the highscore for the current game type.
        private static int s_Score;                                 // Used to store the current game's score.
        private static string s_CurrentGame;                        // The name of the current game type.


        public static int HighScore { get { return s_HighScore; } }
        public static int Score { get { return s_Score; } }


        public static void SetGameType(GameType gameType)
        {
            // Set the name of the current game based on the game type.
            switch (gameType)
            {
                case GameType.FLYER:
                    s_CurrentGame = k_FlyerData;
                    break;

                case GameType.SHOOTER180:
                    s_CurrentGame = k_Shooter180;
                    break;

                case GameType.SHOOTER360:
                    s_CurrentGame = k_Shooter360;
                    break;

                default:
                    Debug.LogError("Invalid GameType");
                    break;
            }

            Restart();
        }


        public static void Restart()
        {
            // Reset the current score and get the highscore from player prefs.
            s_Score = 0;
            s_HighScore = GetHighScore();
        }


        public static void AddScore(int score)
        {
            // Add to the current score and check if the high score needs to be set.
            s_Score += score;
            CheckHighScore();
        }


        public static int GetHighScore()
        {
            // Get the value of the highscore from the game name.
            return PlayerPrefs.GetInt(s_CurrentGame, 0);
        }


        private static void CheckHighScore()
        {
            // If the current score is greater than the high score then set the high score.
            if (s_Score > s_HighScore)
                SetHighScore();
        }


        private static void SetHighScore()
        {
            // Make sure the name of the current game has been set.
            if (string.IsNullOrEmpty(s_CurrentGame))
                Debug.LogError("m_CurrentGame not set");

            // The high score is now equal to the current score.
            s_HighScore = s_Score;

            // Set the high score for the current game's name and save it.
            PlayerPrefs.SetInt(s_CurrentGame, s_Score);
            PlayerPrefs.Save();
        }
    }
}