using System;
using System.Collections.Generic;

namespace TowerDefense.Contracts
{
    // Lớp tĩnh trung gian (Registry / Event Bus) đặt tại tầng Contracts/.
    // Đóng vai trò cầu nối giúp phân hệ Defense (Linh) và Battle (Phú) giao tiếp gián tiếp
    // mà không cần khai báo using hoặc tham chiếu trực tiếp vào lớp cụ thể của nhau.
    public static class CombatRegistry
    {
        // Danh sách nội bộ lưu các thực thể quái đang còn sống trên bản đồ.
        // Đặt private để ngăn mã bên ngoài tự ý thêm/xóa trực tiếp; readonly để khóa tham chiếu List.
        private static readonly List<IDamageable> aliveEnemies = new List<IDamageable>();

        // Trường nền (backing field) lưu trạng thái khóa thao tác bản đồ (khi Pause hoặc kết thúc trận).
        private static bool isInputLocked;

        // Cung cấp góc nhìn chỉ đọc (read-only view) ra bên ngoài.
        // Các lớp đọc (Tower, EnemySpawner) chỉ có thể duyệt và đếm phần tử, không thể gọi Add/Remove/Clear.
        public static IReadOnlyList<IDamageable> AliveEnemies => aliveEnemies;

        // Sự kiện phát đi khi một quái bị tiêu diệt giữa đường, mang theo tham số lượng vàng thưởng (int).
        // Bên phát: EnemyHealth (qua NotifyEnemyKilled). Bên lắng nghe: Wallet, GameManager (BattleStatisticsObserver).
        public static event Action<int> OnEnemyKilled;

        // Sự kiện phát đi khi trạng thái IsInputLocked thay đổi (true = khóa, false = mở).
        // Giúp NodeUI tự ẩn panel khi Pause/Kết thúc mà không cần tham chiếu trực tiếp tới GameManager.
        public static event Action<bool> OnInputLockChanged;

        // Ủy quyền hàm (delegate) trả về bool kiểm tra con trỏ chuột có đang nằm trên phần tử UI hay không.
        // Tầng Presentation (NodeUI) gán cài đặt từ EventSystem; tầng Domain (BuildNode) gọi mà không dính UnityEngine.EventSystems.
        public static Func<bool> IsPointerOverUI { get; set; }

        // Thuộc tính bọc trường isInputLocked; chỉ phát sự kiện OnInputLockChanged khi giá trị thực sự thay đổi.
        public static bool IsInputLocked
        {
            get => isInputLocked;
            set
            {
                if (isInputLocked == value) return;
                isInputLocked = value;
                OnInputLockChanged?.Invoke(isInputLocked);
            }
        }

        // Đăng ký một thực thể quái sống vào danh sách; kiểm tra null và chống trùng lặp phần tử.
        public static void RegisterEnemy(IDamageable enemy)
        {
            if (enemy != null && !aliveEnemies.Contains(enemy))
            {
                aliveEnemies.Add(enemy);
            }
        }

        // Gỡ một thực thể quái khỏi danh sách khi bị tiêu diệt hoặc bị hủy (OnDestroy).
        public static void UnregisterEnemy(IDamageable enemy)
        {
            if (enemy != null)
            {
                aliveEnemies.Remove(enemy);
            }
        }

        // Điều phối nguyên tử khi quái bị diệt: gỡ khỏi danh sách sống trước, rồi mới phát sự kiện thưởng vàng.
        public static void NotifyEnemyKilled(IDamageable enemy, int goldReward)
        {
            UnregisterEnemy(enemy);
            OnEnemyKilled?.Invoke(goldReward);
        }

        // Đặt lại toàn bộ trạng thái tĩnh khi tải lại Scene (RestartGame) hoặc trước mỗi ca Unit Test ([SetUp]).
        // Bắt buộc có vì dữ liệu static không tự bị hủy khi Unity nạp lại Scene.
        public static void ClearAll()
        {
            aliveEnemies.Clear();
            isInputLocked = false;
            IsPointerOverUI = null;
        }
    }
}