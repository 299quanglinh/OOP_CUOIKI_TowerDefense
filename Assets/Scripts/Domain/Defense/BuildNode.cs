using System;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain.Defense
{
    public class BuildNode : MonoBehaviour
    {
        // Sự kiện dùng chung: thông báo ô đất nào vừa được chọn.
        public static event Action<BuildNode> OnNodeSelected;

        [Header("Màu ô đất")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor =
            new Color(0.8f, 0.95f, 1f);

        // Component hiển thị hình ảnh của ô đất.
        private SpriteRenderer spriteRenderer;

        // Tháp đang nằm trên ô này; null nghĩa là chưa có tháp.
        private Tower currentTower;

        // Cho bên ngoài đọc trạng thái và tháp hiện tại.
        public bool IsOccupied => currentTower != null;
        public Tower CurrentTower => currentTower;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        private void OnMouseEnter()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = hoverColor;
            }
        }

        private void OnMouseExit()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        private void OnMouseDown()
        {
            if (CombatRegistry.IsInputLocked) return;

            // Chặn chọn ô đất khi con trỏ đang nằm trên UI.
            if (CombatRegistry.IsPointerOverUI != null &&
                CombatRegistry.IsPointerOverUI())
            {
                return;
            }

            // Gửi chính component BuildNode được chọn tới bên nghe.
            OnNodeSelected?.Invoke(this);
        }

        // BuildManager gọi sau khi tạo tháp thành công.
        public void SetTower(Tower tower)
        {
            currentTower = tower;
        }

        // Xóa GameObject tháp và đưa ô đất về trạng thái trống.
        public void ClearTower()
        {
            if (currentTower != null)
            {
                Destroy(currentTower.gameObject);
                currentTower = null;
            }
        }
    }
}