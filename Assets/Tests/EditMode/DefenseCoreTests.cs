using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TowerDefense.Contracts;
using TowerDefense.Core;

namespace TowerDefense.Tests.EditMode
{
    [TestFixture]
    public class DefenseCoreTests
    {
        // Chạy tự động TRƯỚC mỗi ca test để đảm bảo CombatRegistry luôn sạch.
        [SetUp]
        public void SetUp()
        {
            CombatRegistry.ClearAll();
        }

        // Chạy tự động SAU mỗi ca test để dọn dẹp dữ liệu, tránh rò rỉ sang test sau.
        [TearDown]
        public void TearDown()
        {
            CombatRegistry.ClearAll();
        }

        // UT_01: LowestHealthStrategy chọn đúng quái ít máu nhất TRONG tầm bắn.
        [Test]
        public void UT01_LowestHealthStrategy_SelectsLowestHpWithinRange()
        {
            ITargetingStrategy strategy = new LowestHealthStrategy();
            Vector2 towerPos = Vector2.zero;
            float range = 5f;

            MockEnemy highHpNear = new MockEnemy(100, new Vector3(1f, 0f, 0f));
            MockEnemy lowHpInRange = new MockEnemy(30, new Vector3(4f, 0f, 0f));
            MockEnemy lowerHpOutOfRange = new MockEnemy(10, new Vector3(6f, 0f, 0f));

            var candidates = new List<IDamageable> { highHpNear, lowHpInRange, lowerHpOutOfRange };

            IDamageable selected = strategy.SelectTarget(towerPos, range, candidates);

            Assert.AreSame(lowHpInRange, selected);
        }

        // UT_02: NearestStrategy chọn đúng quái gần tháp nhất trong tầm bắn.
        [Test]
        public void UT02_NearestStrategy_SelectsClosestEnemyWithinRange()
        {
            ITargetingStrategy strategy = new NearestStrategy();
            Vector2 towerPos = Vector2.zero;
            float range = 5f;

            MockEnemy farEnemy = new MockEnemy(20, new Vector3(4.5f, 0f, 0f));
            MockEnemy nearEnemy = new MockEnemy(150, new Vector3(1.5f, 1f, 0f));
            MockEnemy outOfRangeEnemy = new MockEnemy(10, new Vector3(0f, 5.1f, 0f));

            var candidates = new List<IDamageable> { farEnemy, nearEnemy, outOfRangeEnemy };

            IDamageable selected = strategy.SelectTarget(towerPos, range, candidates);

            Assert.AreSame(nearEnemy, selected);
        }

        // UT_03: HighestHealthStrategy chọn đúng quái nhiều máu nhất trong tầm bắn.
        [Test]
        public void UT03_HighestHealthStrategy_SelectsHighestHpWithinRange()
        {
            ITargetingStrategy strategy = new HighestHealthStrategy();
            Vector2 towerPos = Vector2.zero;
            float range = 5f;

            MockEnemy weakEnemy = new MockEnemy(40, new Vector3(1f, 0f, 0f));
            MockEnemy tankEnemy = new MockEnemy(250, new Vector3(3f, 0f, 0f));
            MockEnemy bossOutOfRange = new MockEnemy(999, new Vector3(8f, 0f, 0f));

            var candidates = new List<IDamageable> { weakEnemy, tankEnemy, bossOutOfRange };

            IDamageable selected = strategy.SelectTarget(towerPos, range, candidates);

            Assert.AreSame(tankEnemy, selected);
        }

        // UT_04: Bỏ qua quái đã chết, phần tử null, danh sách null hoặc rỗng.
        [Test]
        public void UT04_Strategies_IgnoreDeadNullAndOutOfRangeEnemies()
        {
            ITargetingStrategy strategy = new LowestHealthStrategy();
            Vector2 towerPos = Vector2.zero;
            float range = 5f;

            MockEnemy deadEnemy = new MockEnemy(10, new Vector3(1f, 0f, 0f), alive: false);
            MockEnemy outOfRangeEnemy = new MockEnemy(20, new Vector3(10f, 0f, 0f), alive: true);

            var candidates = new List<IDamageable> { null, deadEnemy, outOfRangeEnemy };

            Assert.IsNull(strategy.SelectTarget(towerPos, range, candidates));
            Assert.IsNull(strategy.SelectTarget(towerPos, range, null));
            Assert.IsNull(strategy.SelectTarget(towerPos, range, new List<IDamageable>()));
        }

        // UT_05: Chiếu tọa độ 2D (giá trị trục Z lớn không làm sai lệch tầm bắn).
        [Test]
        public void UT05_NearestStrategy_IgnoresZAxisDepth()
        {
            ITargetingStrategy strategy = new NearestStrategy();
            Vector2 towerPos = Vector2.zero;
            float range = 3f;

            MockEnemy enemyWithDeepZ = new MockEnemy(50, new Vector3(2f, 0f, 100f));
            var candidates = new List<IDamageable> { enemyWithDeepZ };

            IDamageable selected = strategy.SelectTarget(towerPos, range, candidates);

            Assert.AreSame(enemyWithDeepZ, selected);
        }

        // UT_06: CombatRegistry chống đăng ký trùng và gỡ quái TRƯỚC khi phát sự kiện thưởng vàng.
        [Test]
        public void UT06_CombatRegistry_NotifyEnemyKilled_UnregistersBeforeInvokingEvent()
        {
            MockEnemy enemy = new MockEnemy(50, Vector3.zero);
            CombatRegistry.RegisterEnemy(enemy);
            CombatRegistry.RegisterEnemy(enemy);

            Assert.AreEqual(1, CombatRegistry.AliveEnemies.Count);

            int receivedGold = 0;
            int aliveCountDuringCallback = -1;

            Action<int> handler = gold =>
            {
                receivedGold = gold;
                aliveCountDuringCallback = CombatRegistry.AliveEnemies.Count;
            };

            CombatRegistry.OnEnemyKilled += handler;
            try
            {
                CombatRegistry.NotifyEnemyKilled(enemy, 25);
            }
            finally
            {
                CombatRegistry.OnEnemyKilled -= handler;
            }

            Assert.AreEqual(25, receivedGold);
            Assert.AreEqual(0, aliveCountDuringCallback);
            Assert.AreEqual(0, CombatRegistry.AliveEnemies.Count);
        }

        // UT_07: IsInputLocked chỉ phát sự kiện OnInputLockChanged khi giá trị thực sự đổi.
        [Test]
        public void UT07_CombatRegistry_IsInputLocked_FiresEventOnlyOnValueChange()
        {
            int triggerCount = 0;
            bool lastState = false;

            Action<bool> handler = locked =>
            {
                triggerCount++;
                lastState = locked;
            };

            CombatRegistry.OnInputLockChanged += handler;
            try
            {
                CombatRegistry.IsInputLocked = true;
                CombatRegistry.IsInputLocked = true;
                CombatRegistry.IsInputLocked = false;
            }
            finally
            {
                CombatRegistry.OnInputLockChanged -= handler;
            }

            Assert.AreEqual(2, triggerCount);
            Assert.IsFalse(lastState);
        }
    }
}