using System;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain.Defense
{
    // Lớp nghiệp vụ quản lý vàng của người chơi ở tầng Domain/Defense.
    // Đóng đồng thời hai vai trò trong Observer Pattern:
    //   + Observer: nghe CombatRegistry.OnEnemyKilled để tự động cộng vàng thưởng.
    //   + Subject: phát sự kiện OnGoldChanged lên tầng Presentation (WalletUI) khi số dư thay đổi.
    public class Wallet : MonoBehaviour
    {
        // Singleton Pattern (đường truy cập dự phòng duy nhất trong mỗi Scene).
        public static Wallet Instance { get; private set; }

        // Sự kiện phát đi mỗi khi currentGold khởi tạo hoặc thay đổi, truyền kèm số dư mới (int).
        public event Action<int> OnGoldChanged;

        // Lượng vàng khởi đầu: đặt private để bảo toàn đóng gói, gắn [SerializeField] để có thể cấu hình trên Unity Inspector.
        [SerializeField] private int startingGold = 100;

        // Trạng thái cốt lõi lưu số dư vàng hiện tại (private, chỉ thay đổi qua AddGold/SpendGold).
        private int currentGold;

        // Chạy 1 lần ngay khi GameObject được khởi tạo
        private void Awake()
        {
            // Khóa tính duy nhất của Singleton: nếu đã tồn tại thể hiện khác thì hủy bản sao thừa.
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            // Kẹp để giá trị khởi tạo không âm.
            currentGold = Mathf.Max(0, startingGold);
        }

        // Chạy mỗi khi Component/GameObject được bật ngay sau Awake.
        private void OnEnable()
        {
            // Observer Pattern: tự đăng ký phương thức AddGold làm listener cho sự kiện quái chết.
            CombatRegistry.OnEnemyKilled += AddGold;
        }

        // Chạy mỗi khi Component/GameObject bị tắt hoặc trước khi bị hủy (OnDestroy).
        private void OnDisable()
        {
            // Bắt buộc hủy đăng ký khỏi sự kiện static để tránh rò rỉ bộ nhớ (memory leak)
            // tránh lỗi MissingReferenceException khi tải lại Scene.
            CombatRegistry.OnEnemyKilled -= AddGold;
        }

        // Chạy khi GameObject bị hủy (ví dụ khi chuyển/tải lại Scene).
        private void OnDestroy()
        {
            // Giải phóng tham chiếu tĩnh nếu đây chính là thể hiện Singleton đang giữ.
            if (Instance == this) Instance = null;
        }

        // Chạy 1 lần trước frame Update đầu tiên (sau Awake và OnEnable của mọi đối tượng).
        private void Start()
        {
            // Phát giá trị vàng ban đầu để các UI đã kịp đăng ký nhận đồng bộ hiển thị.
            OnGoldChanged?.Invoke(currentGold);
        }

        // Phương thức truy vấn chỉ đọc số dư hiện tại.
        public int GetGold() => currentGold;

        // Kiểm tra điều kiện đủ vàng; từ chối ngay nếu chi phí truyền vào âm.
        public bool HasEnoughGold(int cost) => cost >= 0 && currentGold >= cost;

        // Cộng vàng vào ví (dùng khi diệt quái, bán tháp hoặc hoàn tiền khi xây tháp gặp ngoại lệ).
        public void AddGold(int amount)
        {
            // Bỏ qua giá trị không dương để không làm giảm.
            if (amount <= 0) return;
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
        }

        // Quy tắc nghiệp vụ R02: Không cho phép số dư âm hoặc truyền chi phí âm.
        // Trả về true nếu giao dịch thành công, false nếu bị từ chối.
        public bool SpendGold(int cost)
        {
            if (cost < 0 || !HasEnoughGold(cost)) return false;
            currentGold -= cost;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }
    }
}