namespace TowerDefense.Domain
{
    // Lớp con 1: Đạn bắn đơn mục tiêu (dùng cho Tháp Cung / Bắn tỉa).
    public class ArrowProjectile : Projectile
    {
        protected override void OnHitTarget()
        {
            if (target != null && target.IsAlive)
            {
                target.TakeDamage(damage);
            }
        }
    }
}