using System;
using System.Collections;
using UnityEngine;
using TowerDefense.Contracts;
using TowerDefense.Core;

namespace TowerDefense.Domain.Battle
{

    public class EnemySpawner : MonoBehaviour
    {
        // Singleton
        public static EnemySpawner Instance { get; private set; }

        public event Action<int, int> OnWaveChanged; // Thay đổi wave
        public event Action<int> OnCountdownTick; // Đếm ngược khi bắt đầu đợt
        public event Action OnCountdownFinished; // Kết thúc đếm ngược
        public event Action OnAllWavesCompleted; // Phát thông báo chiến thắng

        [Header("Cấu hình Quái & Tuyến đường")]
        [SerializeField] private EnemyMovement enemyPrefab;
        [SerializeField] private Transform[] waypoints;

        [Header("Cấu hình 20 Wave")]
        [SerializeField] private int totalWaves = 20;
        [SerializeField] private int secondsBetweenWaves = 5;

        private WaveConfig[] waveConfigs;
        private int currentWave;
        private bool isSpawning;
        private bool isWaveRunning;
        private bool allWavesCompleted;

        public int GetCurrentWave() => currentWave;
        public int GetTotalWaves() => totalWaves;
        public bool IsAllWavesCompleted() => allWavesCompleted;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            waveConfigs = WaveConfigLoader.LoadWaves(totalWaves);
            totalWaves = waveConfigs.Length;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // UC06_P
        public void StartNextWave()
        {
            if (isWaveRunning || allWavesCompleted) return;
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("[EnemySpawner] Chưa gán danh sách Waypoints, hủy kích hoạt Wave để tránh treo!");
                return;
            }

            // Bật cờ ngay trước StartCoroutine để đóng hoàn toàn khe hở 1 frame (chống chạy 2 Wave song song)
            isWaveRunning = true;
            StartCoroutine(WaveLifecycleRoutine());
        }

        // Vá triệt để lỗi kết thúc Wave sớm: chỉ kiểm tra AliveEnemies == 0 SAU KHI sinh xong toàn bộ quái
        private IEnumerator WaveLifecycleRoutine()
        {
            // Chờ 1 frame đầu để toàn bộ UI.Start() hoàn tất đăng ký sự kiện trước khi phát Wave 1
            yield return null;
            currentWave++;
            OnWaveChanged?.Invoke(currentWave, totalWaves);

            // Đếm ngược 5 giây nghỉ giữa 2 đợt
            for (int sec = secondsBetweenWaves; sec > 0; sec--)
            {
                OnCountdownTick?.Invoke(sec);
                yield return new WaitForSeconds(1f);
            }
            OnCountdownFinished?.Invoke();

            // Sinh đủ toàn bộ quái theo WaveConfig
            WaveConfig config = (waveConfigs != null && currentWave - 1 < waveConfigs.Length)
                ? waveConfigs[currentWave - 1]
                : WaveConfig.CreateScaled(currentWave);

            isSpawning = true;
            for (int i = 0; i < config.EnemyCount; i++)
            {
                SpawnSingleEnemy(config);
                yield return new WaitForSeconds(config.SpawnInterval);
            }
            isSpawning = false;

            // Chờ đến khi không còn quái sống trên bản đồ mới chốt xong Wave
            yield return new WaitUntil(() => !isSpawning && CombatRegistry.AliveEnemies.Count == 0);
            isWaveRunning = false;

            if (currentWave >= totalWaves)
            {
                allWavesCompleted = true;
                OnAllWavesCompleted?.Invoke();
            }
            else
            {
                StartNextWave();
            }
        }

        private void SpawnSingleEnemy(WaveConfig config)
        {
            if (enemyPrefab == null || waypoints == null || waypoints.Length == 0) return;
            // Unity chạy EnemyHealth.Awake() (RegisterEnemy) ngay trong lệnh Instantiate trước khi gọi Configure()
            EnemyMovement enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            enemy.Initialize(waypoints);
            if (enemy.TryGetComponent<EnemyHealth>(out var health))
            {
                health.Configure(config.EnemyHealth, config.RewardGold);
            }
        }
    }
}
