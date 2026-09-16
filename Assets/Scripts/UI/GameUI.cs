using ChefMachine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ChefMachine.UI
{
    /// <summary>
    /// Screen-space UI for the session: start screen, HUD, pause and game over
    /// panels. It reads state from GameSession and ScoreManager and owns no timers.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private GameSession session;
        [SerializeField] private ScoreManager scoreManager;

        [Header("Panels")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD")]
        [SerializeField] private Text scoreLabel;
        [SerializeField] private Text highScoreLabel;
        [SerializeField] private Text timeLabel;

        [Header("Game Over")]
        [SerializeField] private Text finalScoreLabel;
        [SerializeField] private Text finalHighScoreLabel;
        [SerializeField] private GameObject newHighScoreLabel;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button[] quitButtons;

        private void Awake()
        {
            if (session == null) session = FindFirstObjectByType<GameSession>();
            if (scoreManager == null) scoreManager = ScoreManager.Instance;

            if (startButton != null) startButton.onClick.AddListener(OnStartPressed);
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPausePressed);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumePressed);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartPressed);

            if (quitButtons != null)
            {
                for (int i = 0; i < quitButtons.Length; i++)
                {
                    if (quitButtons[i] != null) quitButtons[i].onClick.AddListener(OnQuitPressed);
                }
            }
        }

        private void LateUpdate()
        {
            if (session == null) return;

            GameState state = session.State;

            Show(startPanel, state == GameState.Start);
            Show(hudPanel, state == GameState.Playing || state == GameState.Paused);
            Show(pausePanel, state == GameState.Paused);
            Show(gameOverPanel, state == GameState.GameOver);

            int score = scoreManager != null ? scoreManager.Score : 0;

            if (scoreLabel != null) scoreLabel.text = "Score: " + score;
            if (highScoreLabel != null) highScoreLabel.text = "High Score: " + session.HighScore;
            if (timeLabel != null) timeLabel.text = "Time: " + FormatTime(session.RemainingTime);

            if (state != GameState.GameOver) return;

            if (finalScoreLabel != null) finalScoreLabel.text = "Final Score: " + score;
            if (finalHighScoreLabel != null) finalHighScoreLabel.text = "High Score: " + session.HighScore;
            Show(newHighScoreLabel, session.IsNewHighScore);
        }

        /// <summary>Seconds as mm:ss, e.g. 179.4 -> "02:59".</summary>
        public static string FormatTime(float seconds)
        {
            int whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
        }

        private void OnStartPressed() { if (session != null) session.StartGame(); }
        private void OnPausePressed() { if (session != null) session.PauseGame(); }
        private void OnResumePressed() { if (session != null) session.ResumeGame(); }
        private void OnRestartPressed() { if (session != null) session.RestartGame(); }
        private void OnQuitPressed() { if (session != null) session.QuitGame(); }

        private static void Show(GameObject target, bool visible)
        {
            if (target == null || target.activeSelf == visible) return;
            target.SetActive(visible);
        }
    }
}
