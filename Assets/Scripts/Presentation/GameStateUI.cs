using TMPro;
using UnityEngine;
using TowerDefense.Domain.Battle;

namespace TowerDefense.Presentation
{
    // Quản lý trạng thái trận đấu trên HUD: Tạm dừng (Pause), Thắng/Thua (Win/Lose) và Chơi lại (Restart)
    public class GameStateUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private TMP_Text summaryText;

        private void Start()
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameManager == null) gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnGameEnded += ShowEndScreen;
                gameManager.OnPauseChanged += HandlePauseChanged;
            }
        }

        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameEnded -= ShowEndScreen;
                gameManager.OnPauseChanged -= HandlePauseChanged;
            }
        }

        private void HandlePauseChanged(bool isPaused)
        {
            if (pausePanel != null) pausePanel.SetActive(isPaused);
        }

        private void ShowEndScreen(bool isVictory, string statsSummary)
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(isVictory);
            if (losePanel != null) losePanel.SetActive(!isVictory);
            if (summaryText != null) summaryText.text = statsSummary;
        }

        public void OnRestartClicked()
        {
            if (gameManager != null) gameManager.RestartGame();
        }

        public void OnPauseToggleClicked()
        {
            if (gameManager != null) gameManager.TogglePause();
        }
    }
}
