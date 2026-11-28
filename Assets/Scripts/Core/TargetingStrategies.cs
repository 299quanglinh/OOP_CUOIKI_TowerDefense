using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Core
{
    // Kho chứa ba đối tượng chiến lược dùng chung cho các tháp.
    // Các chiến lược không lưu trạng thái riêng của từng tháp.
    public static class TargetingStrategyCatalog
    {
        // Đối tượng chọn mục tiêu ít máu nhất.
        public static readonly ITargetingStrategy LowestHealth =
            new LowestHealthStrategy();

        // Đối tượng chọn mục tiêu gần nhất.
        public static readonly ITargetingStrategy Nearest =
            new NearestStrategy();

        // Đối tượng chọn mục tiêu nhiều máu nhất.
        public static readonly ITargetingStrategy HighestHealth =
            new HighestHealthStrategy();
    }
    // Chiến lược 1: Ưu tiên mục tiêu ít máu nhất trong tầm bắn.
    public class LowestHealthStrategy : ITargetingStrategy
    {
        public string StrategyName => "Ít máu nhất";

        public IDamageable SelectTarget(Vector2 towerPos, float attackRange, IReadOnlyList<IDamageable> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            IDamageable bestTarget = null;// Con quái đang được chọn.
            int minHp = int.MaxValue;// Máu của con đang được chọn.
            float bestDistance = float.MaxValue; // Khoảng cách từ tháp đến con đó.
            
            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || !candidate.IsEnemy) continue;

                float distance = Vector2.Distance(towerPos, (Vector2)candidate.GetPosition());
                if (distance > attackRange) continue;

                int hp = candidate.GetCurrentHealth();

                // Đổi mục tiêu nếu con này ít máu hơn,
                // hoặc nếu nó bằng máu với mục tiêu hiện tại nhưng ở gần tháp hơn.
                if (hp < minHp || (hp == minHp && distance < bestDistance))
                {
                    bestTarget = candidate;
                    minHp = hp;
                    bestDistance = distance;
                }
            }

            return bestTarget;
        }
    }

    // Chiến lược 2: Ưu tiên mục tiêu gần tháp nhất trong tầm bắn.
    public class NearestStrategy : ITargetingStrategy
    {
        public string StrategyName => "Gần nhất";

        public IDamageable SelectTarget(Vector2 towerPos, float attackRange, IReadOnlyList<IDamageable> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            IDamageable bestTarget = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || !candidate.IsEnemy) continue;

                float distance = Vector2.Distance(towerPos, (Vector2)candidate.GetPosition());
                if (distance > attackRange) continue;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }
    }

    // Chiến lược 3: Ưu tiên mục tiêu nhiều máu nhất trong tầm bắn.
    public class HighestHealthStrategy : ITargetingStrategy
    {
        public string StrategyName => "Nhiều máu nhất";

        public IDamageable SelectTarget(Vector2 towerPos, float attackRange, IReadOnlyList<IDamageable> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            IDamageable bestTarget = null;
            int maxHp = int.MinValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || !candidate.IsEnemy) continue;

                float distance = Vector2.Distance(towerPos, (Vector2)candidate.GetPosition());
                if (distance > attackRange) continue;

                int hp = candidate.GetCurrentHealth();
                if (hp > maxHp)
                {
                    maxHp = hp;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }
    }
}