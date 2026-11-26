using TMPro;
using UnityEngine;
using TowerDefense.Domain.Battle;

namespace TowerDefense.Presentation
{
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text countdownText;

        private void Start()
        {
            if (spawner == null) spawner = EnemySpawner.Instance;
            if (spawner == null) return;

            spawner.OnWaveChanged += HandleWaveChanged;
            spawner.OnCountdownTick += HandleCountdownTick;
            spawner.OnCountdownFinished += HandleCountdownFinished;

            // Đồng bộ đợt lên giao diện đề phòng game chạy trước Start()
            if (spawner.GetCurrentWave() > 0)
            {
                HandleWaveChanged(spawner.GetCurrentWave(), spawner.GetTotalWaves());
            }
        }

        private void OnDestroy()
        {
            if (spawner == null) return;
            spawner.OnWaveChanged -= HandleWaveChanged;
            spawner.OnCountdownTick -= HandleCountdownTick;
            spawner.OnCountdownFinished -= HandleCountdownFinished;
        }

        // Thay đổi đợt
        private void HandleWaveChanged(int currentWave, int totalWaves)
        {
            if (waveText != null) waveText.text = $"Đợt: {currentWave} / {totalWaves}";
        }

        // 
        private void HandleCountdownTick(int secondsLeft)
        {
            if (countdownText == null) return;
            countdownText.gameObject.SetActive(true);
            countdownText.text = $"Đợt tiếp theo trong: {secondsLeft}s";
        }

        private void HandleCountdownFinished()
        {
            if (countdownText != null) countdownText.gameObject.SetActive(false);
        }
    }
}