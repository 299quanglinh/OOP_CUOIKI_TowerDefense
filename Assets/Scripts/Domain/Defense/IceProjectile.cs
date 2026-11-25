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

            // Kiểm tra đa hình qua interface ISlowable 
            if (target.IsAlive && target is ISlowable slowable)
            {
                slowable.ApplySlow(slowPercent, slowDuration);
            }
        }
    }
}