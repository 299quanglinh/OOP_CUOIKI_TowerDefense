using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TowerDefense.Contracts;
using TowerDefense.Core;

namespace TowerDefense.Domain.Battle
{
    public class GameManager : MonoBehaviour
    {
        // Singleton
        public static GameManager Instance { get; private set; }

        public event Action<bool, string> OnGameEnded; // true = Thắng, false = Thua
        public event Action<bool> OnPauseChanged;

        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private EnemySpawner enemySpawner;

        private const string BestWaveKey = "TowerDefense_BestWave";
        private readonly BattleStatisticsObserver statistics = new BattleStatisticsObserver();
        private bool isGameOver;
        private bool isGameWon;
        private bool isPaused;

        public bool IsGameOver() => isGameOver;
        public bool IsGameWon() => isGameWon;
        public bool IsPaused() => isPaused;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            StartGameSession();
        }

        // Hàm điều khiển khởi tạo phiên chơi mới
        public void StartGameSession()
        {
            Time.timeScale = 1f;
            isGameOver = false;
            isGameWon = false;
            isPaused = false;
            CombatRegistry.IsInputLocked = false;
            statistics.Reset();
            if (enemySpawner != null) enemySpawner.StartNextWave();
        }

        private void OnEnable()
        {
            CombatRegistry.OnEnemyKilled += statistics.OnEnemyKilledHandler;
            EnemyMovement.OnEnemyReachedBase += HandleFallbackBaseDamage;
            if (baseHealth != null) baseHealth.OnBaseDestroyed += HandleLoseGame;
            if (enemySpawner != null)
            {
                enemySpawner.OnWaveChanged += statistics.OnWaveChangedHandler;
                enemySpawner.OnAllWavesCompleted += HandleWinGame;
            }
        }

        private void OnDisable()
        {
            CombatRegistry.OnEnemyKilled -= statistics.OnEnemyKilledHandler;
            EnemyMovement.OnEnemyReachedBase -= HandleFallbackBaseDamage;
            if (baseHealth != null) baseHealth.OnBaseDestroyed -= HandleLoseGame;
            if (enemySpawner != null)
            {
                enemySpawner.OnWaveChanged -= statistics.OnWaveChangedHandler;
                enemySpawner.OnAllWavesCompleted -= HandleWinGame;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void HandleFallbackBaseDamage(int dmg)
        {
            if (baseHealth != null) baseHealth.TakeDamage(dmg);
        }

        // Thua ngay lập tức khi máu căn cứ về 0
        private void HandleLoseGame()
        {
            if (isGameOver || isGameWon) return;
            isGameOver = true;
            if (enemySpawner != null) enemySpawner.StopWaves();
            CombatRegistry.IsInputLocked = true;
            Time.timeScale = 0f;
            SaveBestWaveSafely(statistics.HighestWaveReached);
            OnGameEnded?.Invoke(false, BuildEndSummary());
        }

        // Thắng khi diệt sạch Wave 20 và căn cứ còn máu
        private void HandleWinGame()
        {
            if (isGameOver || isGameWon) return;
            if (baseHealth != null && !baseHealth.IsAlive) return;
            isGameWon = true;
            CombatRegistry.IsInputLocked = true;
            Time.timeScale = 0f;
            SaveBestWaveSafely(statistics.HighestWaveReached);
            OnGameEnded?.Invoke(true, BuildEndSummary());
        }

        // Tạm dừng / Tiếp tục
        public void TogglePause()
        {
            if (isGameOver || isGameWon) return;
            isPaused = !isPaused;
            CombatRegistry.IsInputLocked = isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            OnPauseChanged?.Invoke(isPaused);
        }

        // Chơi lại game
        public void RestartGame()
        {
            try
            {
                Time.timeScale = 1f;
                CombatRegistry.ClearAll();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Lỗi tải lại màn chơi: {ex.Message}");
            }
        }

        // Lấy đợt cao nhất
        public int GetBestWave()
        {
            try
            {
                return PlayerPrefs.GetInt(BestWaveKey, 0);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameManager] Không thể đọc kỷ lục Wave: {ex.Message}");
                return statistics.HighestWaveReached;
            }
        }

        private string BuildEndSummary()
        {
            return $"{statistics.GetSummaryText()} | Kỷ lục: Wave {GetBestWave()}";
        }

        private void SaveBestWaveSafely(int wave)
        {
            try
            {
                int prevBest = PlayerPrefs.GetInt(BestWaveKey, 0);
                if (wave > prevBest)
                {
                    PlayerPrefs.SetInt(BestWaveKey, wave);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameManager] Không thể lưu kỷ lục Wave: {ex.Message}");
            }
        }
    }
}
