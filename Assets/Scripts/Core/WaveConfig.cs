using System;

namespace TowerDefense.Core
{
    [Serializable]
    public class WaveConfig
    {
        [UnityEngine.SerializeField] private int waveNumber;
        [UnityEngine.SerializeField] private int enemyCount;
        [UnityEngine.SerializeField] private int enemyHealth;
        [UnityEngine.SerializeField] private int rewardGold;
        [UnityEngine.SerializeField] private float spawnInterval;

        public int WaveNumber => Math.Max(1, waveNumber);
        public int EnemyCount => Math.Max(1, enemyCount);
        public int EnemyHealth => Math.Max(1, enemyHealth);
        public int RewardGold => Math.Max(0, rewardGold);
        public float SpawnInterval => Math.Max(0.2f, spawnInterval);

        public WaveConfig(int waveNumber, int enemyCount, int enemyHealth, int rewardGold, float spawnInterval)
        {
            this.waveNumber = Math.Max(1, waveNumber);
            this.enemyCount = Math.Max(1, enemyCount);
            this.enemyHealth = Math.Max(1, enemyHealth);
            this.rewardGold = Math.Max(0, rewardGold);
            this.spawnInterval = Math.Max(0.2f, spawnInterval);
        }

        // Factory method dự phòng sinh cấu hình mặc định cho Wave 1 -> 20
        public static WaveConfig CreateScaled(int wave)
        {
            int count = 4 + wave;
            int hp = 100 + (wave - 1) * 45;
            int gold = 25;
            float interval = Math.Max(0.45f, 1.2f - (wave - 1) * 0.03f);
            return new WaveConfig(wave, count, hp, gold, interval);
        }
    }

    [Serializable]
    // Danh sách cấu hình sóng
    public class WaveConfigList
    {
        public WaveConfig[] waves;
    }

    public static class WaveConfigLoader
    {
        public static WaveConfig[] LoadWaves(int fallbackCount = 20)
        {
            try
            {   
                // Đọc dữ liệu đợt sóng từ file wave.json
                string path = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "waves.json");
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    WaveConfigList parsed = UnityEngine.JsonUtility.FromJson<WaveConfigList>(json);
                    if (parsed != null && parsed.waves != null && parsed.waves.Length > 0)
                    {
                        return parsed.waves;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[WaveConfigLoader] Lỗi đọc waves.json, dùng cấu hình mặc định: {ex.Message}");
            }

            // gọi factory method dự phòng để sinh quái với số đợt = fallbackCount
            WaveConfig[] fallback = new WaveConfig[fallbackCount];
            for (int i = 0; i < fallbackCount; i++)
            {
                fallback[i] = WaveConfig.CreateScaled(i + 1);
            }
            return fallback;
        }
    }
}
