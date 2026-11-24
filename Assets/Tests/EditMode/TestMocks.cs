using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Tests.EditMode
{
    // Đối tượng giả lập (Mock Object) dùng chung cho cả DefenseCoreTests (Linh) và BattleCoreTests (Phú).
    // Thực thi IDamageable dưới dạng lớp C# thuần (không kế thừa MonoBehaviour) để chạy Unit Test
    // trong EditMode mà không cần khởi tạo Scene hay GameObject của Unity.
    internal class MockEnemy : IDamageable
    {
        // Trạng thái còn sống của quái giả lập; bên ngoài chỉ đọc (get), chỉ lớp này tự cập nhật (private set).
        public bool IsAlive { get; private set; }

        // Thuộc tính chỉ đọc dạng expression-bodied, luôn trả về true để mô phỏng thực thể phe địch.
        public bool IsEnemy => true;

        // Lượng máu hiện tại của quái giả lập (private để bảo toàn tính đóng gói).
        private int hp;

        // Tọa độ cố định của quái trong ca kiểm thử; readonly đảm bảo vị trí không bị thay đổi sau khởi tạo.
        private readonly Vector3 pos;

        // Hàm dựng (Constructor) cho phép thiết lập nhanh trạng thái đầu vào cho từng Test Case.
        // Tham số alive có giá trị mặc định là true (optional parameter).
        public MockEnemy(int hp, Vector3 pos, bool alive = true)
        {
            this.hp = hp;
            this.pos = pos;
            this.IsAlive = alive;
        }

        // Thực thi hợp đồng IDamageable: trả về máu hiện tại.
        public int GetCurrentHealth() => hp;

        // Thực thi hợp đồng IDamageable: trả về tọa độ không gian.
        public Vector3 GetPosition() => pos;

        // Thực thi hợp đồng IDamageable: trừ máu không cho âm (Mathf.Max) và chuyển IsAlive về false khi máu về 0.
        public void TakeDamage(int damage)
        {
            hp = Mathf.Max(0, hp - damage);
            if (hp == 0) IsAlive = false;
        }
    }
}