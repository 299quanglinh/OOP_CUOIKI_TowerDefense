using TMPro;
using UnityEngine;
using TowerDefense.Domain.Defense;

namespace TowerDefense.Presentation
{
    // Lớp hiển thị giao diện số dư vàng ở Tầng 1 (Presentation).
    // Đóng vai trò ConcreteObserver trong Observer Pattern: chỉ lắng nghe sự kiện OnGoldChanged
    // từ tầng Domain (Wallet) để cập nhật văn bản UI
    public class WalletUI : MonoBehaviour
    {
        // Tham chiếu tới Subject ở tầng Domain; ưu tiên gán trực tiếp qua Unity Inspector
        [SerializeField] private Wallet wallet;

        // Thành phần hiển thị văn bản TextMeshPro trên Canvas HUD.
        [SerializeField] private TMP_Text goldText;

        // Chạy 1 lần sau Awake() và OnEnable() của tất cả các đối tượng trong Scene.
        private void Start()
        {
            // Cơ chế dự phòng qua Singleton nếu người thiết kế quên kéo thả Wallet trên Inspector.
            if (wallet == null) wallet = Wallet.Instance;

            if (wallet != null)
            {
                // Đăng ký phương thức UpdateGoldDisplay lắng nghe sự kiện thay đổi số dư vàng.
                wallet.OnGoldChanged += UpdateGoldDisplay;

                // Chủ động đồng bộ giá trị hiển thị lần đầu ngay tại thời điểm đăng ký,
                // tránh tình trạng Wallet.Start() đã chạy trước WalletUI.Start().
                UpdateGoldDisplay(wallet.GetGold());
            }
        }

        // Chạy khi GameObject chứa WalletUI bị hủy (ví dụ khi tải lại Scene hoặc chuyển màn).
        private void OnDestroy()
        {
            // Hủy đăng ký sự kiện đối xứng với Start() để tránh giữ tham chiếu tới đối tượng đã hủy.
            if (wallet != null)
            {
                wallet.OnGoldChanged -= UpdateGoldDisplay;
            }
        }

        // Hàm xử lý sự kiện (Event Handler) khớp chữ ký Action<int> của Wallet.OnGoldChanged.
        private void UpdateGoldDisplay(int gold)
        {
            // Kiểm tra null an toàn trước khi ghi thuộc tính text để tránh NullReferenceException nếu thiếu tham chiếu UI.
            if (goldText != null) goldText.text = $"Vàng: {gold}G";
        }
    }
}