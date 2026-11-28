using UnityEngine;
using TowerDefense.Contracts;
using TowerDefense.Core;
using TowerDefense.Domain;

namespace TowerDefense.Domain.Defense
{
    public class Tower : MonoBehaviour
    {
        [Header("Chỉ số chiến đấu")]
        [SerializeField] private float attackRange = 3.5f; //Phạm vi bắn
        [SerializeField] private float attackInterval = 1f; //Khoảng thời gian giữa 2 lần bắn
        [SerializeField] private int attackDamage = 25; // sát thương mỗi lần bắn

        // Prefab đạn: ArrowProjectile, CannonProjectile hoặc IceProjectile.
        [SerializeField] private Projectile projectilePrefab;

        [Header("Cấp độ & Chi phí")]
        [SerializeField] private int buildCost = 50;
        [SerializeField] private int level = 1;
        [SerializeField] private int maxLevel = 4;
        [SerializeField] private int upgradeCost = 40;
        [SerializeField] private float refundRate = 0.5f        ;

        // Tổng tiền xây và nâng cấp đã đầu tư vào tháp.
        private int totalInvestedGold;

        // Thời gian đã tích lũy để chờ phát bắn tiếp theo.
        private float attackTimer;

        // Mục tiêu được Strategy chọn ở nhịp bắn hiện tại.
        private IDamageable currentTarget;

        // Đối tượng thực hiện thuật toán chọn mục tiêu.
        private ITargetingStrategy targetingStrategy;

        private void Awake()
        {
            targetingStrategy = TargetingStrategyCatalog.LowestHealth;

            // Đủ thời gian ngay từ đầu để có thể bắn phát đầu tiên.
            attackTimer = GetEffectiveInterval();
        }

        private float GetEffectiveInterval()
        {
            // Khoảng chờ thực tế tối thiểu là 0,1 giây.
            return Mathf.Max(0.1f, attackInterval);
        }

        private void Update()
        {
            if (CombatRegistry.IsInputLocked) return;

            float interval = GetEffectiveInterval();

            // Chưa đủ thời gian: tiếp tục tích lũy và chờ frame sau.
            if (attackTimer < interval)
            {
                attackTimer += Time.deltaTime;
                return;
            }

            // Đủ thời gian: đánh giá lại mục tiêu theo Strategy.
            FindTarget();

            if (currentTarget != null)
            {
                Shoot();

                // Bắt đầu chu kỳ chờ mới sau phát bắn.
                attackTimer = 0f;
            }
        }

        private void FindTarget()
        {
            currentTarget = targetingStrategy != null
                ? targetingStrategy.SelectTarget(
                    transform.position,
                    attackRange,
                    CombatRegistry.AliveEnemies)
                : null;
        }

        private void Shoot()
        {
            if (projectilePrefab == null || currentTarget == null)
                return;

            // Unity tạo một viên đạn mới từ Prefab.
            // pro..fab: lấy mẫu viên đạn, tran...tion: vi trí sinh ra viên đạn, Q...ity: góc xoay
            Projectile projectile = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity);

            // Truyền dữ liệu để viên đạn tự xử lý phần bay và va chạm.
            projectile.Initialize(currentTarget, attackDamage);
        }

        public void SetTargetingStrategy(ITargetingStrategy newStrategy)
        {
            if (newStrategy == null) return;

            targetingStrategy = newStrategy;
            currentTarget = null;
        }

        public int GetBuildCost() => Mathf.Max(0, buildCost);
        public int GetLevel() => level;
        public int GetUpgradeCost() => upgradeCost;
        public bool IsMaxLevel() => level >= maxLevel;

        // Hoàn lại 50% tổng tiền đã đầu tư.
        public int GetSellPrice() =>
            Mathf.RoundToInt(totalInvestedGold * refundRate);

        // BuildManager gọi sau khi tạo tháp để ghi nhận tiền xây thực tế.
        public void InitializeCost(int initialBuildCost)
        {
            totalInvestedGold = Mathf.Max(0, initialBuildCost);
        }

        // BuildManager kiểm tra và trừ vàng trước khi gọi hàm này.
        public void Upgrade()
        {
            if (IsMaxLevel()) return;

            // Ghi nhận chi phí hiện tại trước khi tăng giá lần sau.
            totalInvestedGold += upgradeCost;

            level++;
            attackDamage += 15;
            attackRange += 0.5f;
            upgradeCost += 30;
        }
    }
}