using NUnit.Framework;
using UnityEngine;
using TowerDefense.Contracts;
using TowerDefense.Core;

namespace TowerDefense.Tests.EditMode
{
    // Bộ kiểm thử đơn vị phân hệ Trận đấu (Phú phụ trách — UT_05 đến UT_08).
    public class BattleCoreTests
    {
        // CombatRegistry là lớp static: dọn sạch trạng thái trước mỗi ca để các test không ảnh hưởng nhau.
        [SetUp]
        public void SetUp() => CombatRegistry.ClearAll();

        [TearDown]
        public void TearDown() => CombatRegistry.ClearAll();

        // UT_05: Công thức sinh Wave dự phòng khớp bảng dữ liệu 20 Wave (Wave 1: 5 con/100 HP; Wave 20: 24 con/955 HP).
        [Test]
        public void UT05_WaveConfig_CreateScaled_MatchesWave1AndWave20()
        {
            WaveConfig w1 = WaveConfig.CreateScaled(1);
            WaveConfig w20 = WaveConfig.CreateScaled(20);

            Assert.AreEqual(5, w1.EnemyCount);
            Assert.AreEqual(100, w1.EnemyHealth);
            Assert.AreEqual(24, w20.EnemyCount);
            Assert.AreEqual(955, w20.EnemyHealth);
        }

        // UT_06: Observer thống kê đếm đúng số quái diệt, cộng vàng và bỏ qua vàng âm.
        [Test]
        public void UT06_BattleStatisticsObserver_AccumulatesCorrectly()
        {
            var stats = new BattleStatisticsObserver();

            stats.OnWaveChangedHandler(3, 20);
            stats.OnEnemyKilledHandler(25);
            stats.OnEnemyKilledHandler(25);
            stats.OnEnemyKilledHandler(-10); // Không cộng vàng âm

            Assert.AreEqual(3, stats.HighestWaveReached);
            Assert.AreEqual(3, stats.TotalEnemiesKilled);
            Assert.AreEqual(50, stats.TotalGoldEarned);
        }

        // UT_07: CombatRegistry gỡ quái đã chết khỏi danh sách sống và phát OnEnemyKilled đúng số vàng.
        [Test]
        public void UT07_CombatRegistry_NotifiesKilledAndRemovesEnemy()
        {
            var enemy = new MockEnemy(100, Vector3.zero);
            int receivedGold = 0;
            System.Action<int> listener = g => receivedGold += g;

            CombatRegistry.RegisterEnemy(enemy);
            CombatRegistry.OnEnemyKilled += listener;
            try
            {
                CombatRegistry.NotifyEnemyKilled(enemy, 25);
            }
            finally
            {
                // Luôn hủy đăng ký để listener không rò sang ca kiểm thử khác.
                CombatRegistry.OnEnemyKilled -= listener;
            }

            Assert.AreEqual(0, CombatRegistry.AliveEnemies.Count);
            Assert.AreEqual(25, receivedGold);
        }

        // UT_08: ClearAll() xóa sạch danh sách tĩnh khi Restart.
        [Test]
        public void UT08_CombatRegistry_ClearAll_ResetsRegistry()
        {
            CombatRegistry.RegisterEnemy(new MockEnemy(100, Vector3.zero));
            CombatRegistry.RegisterEnemy(new MockEnemy(150, Vector3.one));
            CombatRegistry.IsInputLocked = true;

            CombatRegistry.ClearAll();

            Assert.AreEqual(0, CombatRegistry.AliveEnemies.Count);
            Assert.IsFalse(CombatRegistry.IsInputLocked);
        }
    }
}
