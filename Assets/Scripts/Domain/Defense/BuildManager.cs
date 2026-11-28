using System;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain.Defense
{
    public class BuildManager : MonoBehaviour
    {
        // Tham chiếu dùng chung tới Manager đang hoạt động.
        // Bên ngoài được đọc, chỉ lớp này được gán lại.
        public static BuildManager Instance { get; private set; }

        // Thông báo lý do thao tác bị từ chối.
        // NodeUI sẽ đăng ký để hiển thị thông báo này.
        public event Action<string> OnActionFailed;

        private void Awake()
        {
            // Giữ một Manager trong Scene.
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            // Chỉ giải phóng Instance nếu Manager bị hủy
            // chính là đối tượng đang được Instance giữ.
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool BuildTowerOn(BuildNode node, Tower towerPrefab)
        {
            // Không xây khi Battle đang Pause hoặc đã kết thúc.
            if (CombatRegistry.IsInputLocked) return false;

            // Theo dõi giao dịch để xử lý nếu có ngoại lệ.
            bool goldSpent = false;
            int buildCost = 0;
            Tower newTower = null;

            try
            {
                // Kiểm tra dữ liệu cần thiết.
                if (node == null)
                {
                    throw new ArgumentNullException(
                        nameof(node), "Chưa chọn ô đất.");
                }

                if (towerPrefab == null)
                {
                    throw new ArgumentNullException(
                        nameof(towerPrefab), "Chưa gán Prefab tháp.");
                }

                // Mỗi ô chỉ được chứa một tháp.
                if (node.IsOccupied)
                {
                    OnActionFailed?.Invoke("Ô đất đã có tháp!");
                    return false;
                }

                if (Wallet.Instance == null)
                {
                    throw new InvalidOperationException(
                        "Chưa có Wallet trong Scene.");
                }

                // Đọc giá từ Prefab và yêu cầu ví trừ tiền.
                buildCost = towerPrefab.GetBuildCost();

                if (!Wallet.Instance.SpendGold(buildCost))
                {
                    OnActionFailed?.Invoke("Không đủ vàng để xây tháp!");
                    return false;
                }

                // Ví đã trừ tiền thành công.
                goldSpent = true;

                // Tạo tháp tại vị trí ô đất và đặt làm con của ô.
                newTower = Instantiate(
                    towerPrefab,
                    node.transform.position,
                    Quaternion.identity,
                    node.transform);

                // Ghi nhận tiền xây thực tế vào tháp mới.
                newTower.InitializeCost(buildCost);

                // Cho ô đất giữ tham chiếu tới tháp mới.
                node.SetTower(newTower);

                return true;
            }
            catch (Exception ex)
            {
                // Dọn tháp nếu đã tạo nhưng quá trình xây gặp ngoại lệ.
                if (newTower != null)
                {
                    Destroy(newTower.gameObject);
                }

                // Hoàn tiền nếu giao dịch đã trừ vàng.
                if (goldSpent && Wallet.Instance != null)
                {
                    Wallet.Instance.AddGold(buildCost);
                }

                Debug.LogError(
                    $"[BuildManager] Lỗi khi xây tháp: {ex.Message}");

                OnActionFailed?.Invoke("Không thể khởi tạo tháp.");
                return false;
            }
        }

        public bool UpgradeTowerOn(BuildNode node)
        {
            // Cần một ô có tháp và một ví vàng hợp lệ.
            if (CombatRegistry.IsInputLocked ||
                node == null ||
                !node.IsOccupied ||
                Wallet.Instance == null)
            {
                return false;
            }

            // Lấy tháp đang nằm trên ô.
            Tower tower = node.CurrentTower;

            if (tower.IsMaxLevel())
            {
                OnActionFailed?.Invoke("Tháp đã đạt cấp tối đa!");
                return false;
            }

            // Giá nâng cấp 
            int cost = tower.GetUpgradeCost();

            if (!Wallet.Instance.SpendGold(cost))
            {
                OnActionFailed?.Invoke("Không đủ vàng để nâng cấp!");
                return false;
            }

            // Chỉ nâng cấp sau khi ví trừ tiền thành công.
            tower.Upgrade();
            return true;
        }

        public bool SellTowerOn(BuildNode node)
        {
            if (CombatRegistry.IsInputLocked ||
                node == null ||
                !node.IsOccupied ||
                Wallet.Instance == null)
            {
                return false;
            }

            // Phải lấy giá bán trước khi xóa tháp khỏi ô.
            int refund = node.CurrentTower.GetSellPrice();

            // Xóa tháp và đưa ô về trạng thái trống.
            node.ClearTower();

            // Cộng tiền hoàn lại vào ví.
            Wallet.Instance.AddGold(refund);

            return true;
        }

        // Điểm gọi để mã điều phối thay đổi chiến lược của tháp.
        public void SetTowerStrategy(
            BuildNode node,
            ITargetingStrategy strategy)
        {
            if (node == null || !node.IsOccupied || strategy == null)
                return;

            node.CurrentTower.SetTargetingStrategy(strategy);
        }
    }
}