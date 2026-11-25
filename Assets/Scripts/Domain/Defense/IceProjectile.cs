using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain
{
    // Lớp con 3: Đạn băng gây sát thương (nếu có) và áp hiệu ứng làm chậm qua giao diện ISlowable.
    public class IceProjectile : Projectile
    {
        [Header("Thông số làm chậm")]
        [Range(0f, 0.9f)]
        [SerializeField] private float slowPercent = 0.4f;
        [SerializeField] private float slowDuration = 2.5f;

        protected override void OnHitTarget()
        {
            if (target == null || !target.IsAlive) return;

            if (damage > 0)
            {
                target.TakeDamage(damage);
            }

            // Chỉ làm chậm nếu mục tiêu vẫn sống sau khi nhận sát thương.
            // target trỏ tới EnemyHealth; tìm component thực hiện ISlowable
            // trên cùng GameObject để tương thích với EnemyMovement của Phú.
            if (target.IsAlive &&
                target is Component targetComponent &&
                targetComponent != null &&
                targetComponent.TryGetComponent<ISlowable>(out var slowable))
            {
                slowable.ApplySlow(slowPercent, slowDuration);
            }
        }
    }
}