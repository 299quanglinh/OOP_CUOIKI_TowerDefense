using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain
{
    // Lớp trừu tượng: Định nghĩa khung hành vi chung cho mọi loại đạn.
    public abstract class Projectile : MonoBehaviour
    {
        [Header("Thông số đạn cơ bản")]
        [SerializeField] protected float speed = 12f;
        [SerializeField] protected float hitRadius = 0.25f;

        protected IDamageable target;
        protected int damage;

        // Khởi tạo dữ liệu cho viên đạn ngay sau khi Tower tạo ra đạn
        public virtual void Initialize(IDamageable targetEnemy, int attackDamage)
        {
            target = targetEnemy;
            damage = Mathf.Max(0, attackDamage);
        }

        protected virtual void Update()
        {
            // Nếu mục tiêu đã chết hoặc biến mất giữa đường đi -> tự hủy đạn.
            if (target == null || !target.IsAlive)
            {
                Destroy(gameObject); // gameObject là biến thuộc tính có sẵn đại diện cho chính vật thể cụ thể đang chạy đoạn code này 
                return;
            }

            Vector2 currentPos = transform.position; // lấy tọa độ viên đạn 
            Vector2 targetPos = target.GetPosition(); //lấy tọa độ mục tiêu 
            Vector2 direction = targetPos - currentPos; //vector từ đạn tới mục tiêu 

            float step = speed * Time.deltaTime;

            // Xoay đầu viên đạn hướng về phía mục tiêu trên mặt phẳng 2D.
            if (direction.sqrMagnitude > 0.0001f) // x^2 + y^2
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // Kiểm tra va chạm: nếu bước di chuyển khung hình này đủ chạm mục tiêu hoặc nằm trong bán kính trúng.
            if (direction.magnitude <= step || direction.magnitude <= hitRadius) // magnitude ~ sqrt(x^2 + y^2)
            {
                transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
                OnHitTarget();
                Destroy(gameObject);
                return;
            }

            // Di chuyển đạn tiến về phía mục tiêu.
            Vector2 nextPos = currentPos + direction.normalized * step;
            transform.position = new Vector3(nextPos.x, nextPos.y, transform.position.z);
        }

        // Phương thức trừu tượng (Abstract Method):
        // Ép buộc mỗi loại đạn con phải tự định nghĩa hiệu ứng khi trúng đích (bắn đơn, nổ lan...).
        protected abstract void OnHitTarget();
    }
}