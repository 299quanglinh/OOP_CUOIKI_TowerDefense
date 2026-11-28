using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain
{
    // Lớp con 2: Đạn pháo gây sát thương lan (AoE) lên toàn bộ quái trong bán kính nổ.
    public class CannonProjectile : Projectile
    {
        [Header("Thông số nổ lan (AoE)")]
        [SerializeField] private float splashRadius = 2.0f;

        protected override void OnHitTarget()
        {
            Vector2 impactPos = transform.position;
            IReadOnlyList<IDamageable> enemies = CombatRegistry.AliveEnemies;

            // Duyệt NGƯỢC từ cuối về đầu danh sách:
            // Khi TakeDamage() làm quái chết, CombatRegistry sẽ lập tức gỡ quái đó khỏi AliveEnemies.
            // Duyệt ngược đảm bảo việc xóa phần tử không làm lệch chỉ số hay bỏ sót quái kế tiếp.
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count) continue;

                IDamageable enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || !enemy.IsEnemy) continue;

                float distance = Vector2.Distance(impactPos, (Vector2)enemy.GetPosition());
                if (distance <= splashRadius)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}