using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Contracts
{
    // Hợp đồng nhận sát thương: dùng chung cho Quái vật (EnemyHealth) và Căn cứ (BaseHealth).
    // Cho phép đạn và quái tương tác đa hình mà không phụ thuộc vào lớp cụ thể của nhau.
    public interface IDamageable
    {
        // Trạng thái còn sống của thực thể (chỉ đọc từ bên ngoài để bảo toàn đóng gói).
        bool IsAlive { get; }

        // Phân biệt thực thể thuộc phe địch (true) hay phe ta (false),
        // dùng để lọc mục tiêu khi quét sát thương diện rộng (CannonProjectile).
        bool IsEnemy { get; }

        // Trả về lượng máu hiện tại phục vụ thuật toán chọn mục tiêu (Lowest/Highest HP).
        int GetCurrentHealth();

        // Trả về tọa độ không gian của thực thể để tính tầm bắn và hướng bay của đạn.
        Vector3 GetPosition();

        // Nhận một lượng sát thương; lớp thực thi tự chịu trách nhiệm kiểm tra hợp lệ và xử lý khi máu về 0.
        void TakeDamage(int damage);
    }

    // Hợp đồng làm chậm: tách riêng khỏi IDamageable theo nguyên lý phân tách giao diện (ISP).
    // IceProjectile gọi qua interface này mà không cần biết tới lớp cụ thể EnemyMovement.
    public interface ISlowable
    {
        // Áp dụng hiệu ứng giảm tốc độ theo tỉ lệ (slowPercent) trong khoảng thời gian (duration giây).
        void ApplySlow(float slowPercent, float duration);
    }

    // Hợp đồng chiến lược chọn mục tiêu (vai trò Strategy trong Strategy Pattern).
    // Giúp Tower ủy quyền thuật toán chọn mục tiêu và hoán đổi chiến lược tại runtime.
    public interface ITargetingStrategy
    {
        // Tên hiển thị của chiến lược cho tầng Presentation (NodeUI).
        string StrategyName { get; }

        // Chọn một mục tiêu hợp lệ trong tầm bắn (attackRange) tính từ vị trí tháp (towerPos)
        // trên danh sách ứng viên chỉ đọc (candidates). Trả về null nếu không có mục tiêu phù hợp.
        IDamageable SelectTarget(Vector2 towerPos, float attackRange, IReadOnlyList<IDamageable> candidates);
    }
}