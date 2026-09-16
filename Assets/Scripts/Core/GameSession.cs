using ChefMachine.Orders;
using ChefMachine.Player;
using ChefMachine.Stations;
using UnityEngine;

namespace ChefMachine.Core
{
    public enum GameState
    {
        Start,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Owns the session: which state the game is in, the three minute countdown,
    /// and the persistent high score. It does not own score or orders.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public const string HighScoreKey = "ChefMachine.HighScore";

        [Header("Session")]
        [SerializeField] private float gameDuration = 180f;

        [Header("References")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private OrderManager orderManager;
        [Tooltip("Player components that may only run while Playing.")]
        [SerializeField] private MonoBehaviour[] gameplayComponents;

        [Header("Kitchen Cleanup")]
        [SerializeField] private ChopTable chopTable;
        [SerializeField] private Stove stove;
        [SerializeField] private PlayerHands playerHands;

        private GameState state = GameState.Start;
        private float remainingTime;
        private int highScore;
        private bool newHighScore;
        private bool kitchenClean;

        public GameState State { get { return state; } }
        public float GameDuration { get { return gameDuration; } }
        public float RemainingTime { get { return Mathf.Max(0f, remainingTime); } }
        public int HighScore { get { return highScore; } }
        public bool IsNewHighScore { get { return newHighScore; } }
        public int CurrentScore { get { return scoreManager != null ? scoreManager.Score : 0; } }

        private void Awake()
        {
            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        private void Start()
        {
            EnterStartState();
        }

        private void Update()
        {
            if (state != GameState.Playing) return;

            // Unscaled time is not used on purpose: pausing with timeScale freezes this too.
            remainingTime -= Time.deltaTime;
            if (remainingTime > 0f) return;

            remainingTime = 0f;
            EndGame();
        }

        /// <summary>Idle state before the first Start press: no orders, no countdown.</summary>
        private void EnterStartState()
        {
            state = GameState.Start;
            remainingTime = gameDuration;
            newHighScore = false;

            Time.timeScale = 1f;
            SetGameplayComponentsEnabled(false);

            if (orderManager != null) orderManager.EndSession();
            if (scoreManager != null) scoreManager.ResetScore();
            CleanKitchen();
        }

        /// <summary>Start or restart a session: fresh score, fresh orders, full clock.</summary>
        public void StartGame()
        {
            remainingTime = gameDuration;
            newHighScore = false;
            state = GameState.Playing;

            Time.timeScale = 1f;
            // Game Over and the start screen already leave the kitchen clean.
            if (!kitchenClean) CleanKitchen();
            kitchenClean = false;

            if (scoreManager != null) scoreManager.ResetScore();
            if (orderManager != null) orderManager.BeginSession();
            SetGameplayComponentsEnabled(true);

            Debug.Log("Game started - " + Mathf.RoundToInt(gameDuration) + " seconds.", this);
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void PauseGame()
        {
            if (state != GameState.Playing) return;

            state = GameState.Paused;
            Time.timeScale = 0f;
            SetGameplayComponentsEnabled(false);
            Debug.Log("Game paused.", this);
        }

        public void ResumeGame()
        {
            if (state != GameState.Paused) return;

            state = GameState.Playing;
            Time.timeScale = 1f;
            SetGameplayComponentsEnabled(true);
            Debug.Log("Game resumed.", this);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Quit requested (ignored in the Editor).", this);
#else
            Debug.Log("Quit requested.", this);
            Application.Quit();
#endif
        }

        private void EndGame()
        {
            state = GameState.GameOver;
            Time.timeScale = 1f;

            SetGameplayComponentsEnabled(false);
            if (orderManager != null) orderManager.EndSession();
            CleanKitchen();

            int finalScore = CurrentScore;
            newHighScore = finalScore > highScore;

            if (newHighScore)
            {
                highScore = finalScore;
                PlayerPrefs.SetInt(HighScoreKey, highScore);
                PlayerPrefs.Save();
                Debug.Log("Game over. NEW HIGH SCORE: " + highScore, this);
            }
            else
            {
                Debug.Log("Game over. Final score " + finalScore + " (high score " + highScore + ").", this);
            }
        }

        /// <summary>
        /// Empties the stations and the chef's hands so no food survives into the
        /// next session. The stations own their own state; this only asks them to reset.
        /// </summary>
        private void CleanKitchen()
        {
            if (chopTable != null) chopTable.ResetStation();
            if (stove != null) stove.ResetStation();
            if (playerHands != null) playerHands.ClearIngredient();

            kitchenClean = true;
        }

        private void SetGameplayComponentsEnabled(bool enabled)
        {
            if (gameplayComponents == null) return;

            for (int i = 0; i < gameplayComponents.Length; i++)
            {
                if (gameplayComponents[i] != null) gameplayComponents[i].enabled = enabled;
            }
        }
    }
}
