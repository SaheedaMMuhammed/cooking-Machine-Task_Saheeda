using UnityEngine;

namespace ChefMachine.Core
{
    /// <summary>Tracks the run score. One instance lives in the scene.</summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [SerializeField] private int score;

        public int Score { get { return score; } }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple ScoreManagers in the scene; keeping the first one.", this);
                return;
            }

            Instance = this;
            score = 0;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Clears the score for a new session. High score lives elsewhere.</summary>
        public void ResetScore()
        {
            score = 0;
        }

        public void AddScore(int amount)
        {
            score += amount;
            // Orders can finish for negative points, so sign the number properly.
            Debug.Log("Score added: " + (amount >= 0 ? "+" : "") + amount);
            Debug.Log("Current Score: " + score);
        }
    }
}
