using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TowerDefense.Contracts;
using TowerDefense.Domain.Defense;

namespace TowerDefense.Presentation
{
    public class NodeUI : MonoBehaviour
    {
        [Header("Manager điều phối")]
        [SerializeField] private BuildManager buildManager;

        [Header("Các bảng giao diện")]
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private GameObject managePanel;

        [Header("Prefab tháp")]
        [SerializeField] private Tower arrowTowerPrefab;
        [SerializeField] private Tower cannonTowerPrefab;
        [SerializeField] private Tower iceTowerPrefab;

        [Header("Nút và nội dung hiển thị")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text upgradeButtonText;
        [SerializeField] private TMP_Text sellButtonText;
        [SerializeField] private TMP_Text feedbackText;

        // Ô đất mà giao diện đang thao tác.
        private BuildNode selectedNode;

        private void Start()
        {
            // Ưu tiên tham chiếu được gán trên Inspector.
            // Nếu chưa gán thì lấy Manager qua Instance.
            if (buildManager == null)
            {
                buildManager = BuildManager.Instance;
            }

            // Nghe sự kiện chọn ô và khóa thao tác.
            BuildNode.OnNodeSelected += Open;
            CombatRegistry.OnInputLockChanged += HandleInputLockChanged;

            // Cung cấp hàm kiểm tra con trỏ trên UI cho BuildNode.
            CombatRegistry.IsPointerOverUI = CheckPointerOverUI;

            // Nghe lý do giao dịch thất bại để hiển thị.
            if (buildManager != null)
            {
                buildManager.OnActionFailed += ShowFeedback;
            }
            else
            {
                Debug.LogError("[NodeUI] Chưa có BuildManager trong Scene.");
            }

            // Khi bắt đầu game, chưa có ô nào được chọn.
            HideAll();
        }

        private void OnDestroy()
        {
            // Hủy đăng ký đối xứng với Start().
            BuildNode.OnNodeSelected -= Open;
            CombatRegistry.OnInputLockChanged -= HandleInputLockChanged;

            // Chỉ xóa callback nếu nó vẫn thuộc NodeUI này.
            if (CombatRegistry.IsPointerOverUI ==
                (Func<bool>)CheckPointerOverUI)
            {
                CombatRegistry.IsPointerOverUI = null;
            }

            if (buildManager != null)
            {
                buildManager.OnActionFailed -= ShowFeedback;
            }
        }

        private bool CheckPointerOverUI()
        {
            return EventSystem.current != null &&
                   EventSystem.current.IsPointerOverGameObject();
        }

        private void HandleInputLockChanged(bool locked)
        {
            // Pause hoặc kết thúc trận thì đóng bảng thao tác.
            if (locked)
            {
                HideAll();
            }
        }

        public void Open(BuildNode node)
        {
            if (node == null || CombatRegistry.IsInputLocked)
            {
                HideAll();
                return;
            }

            // Ghi nhận ô vừa được chọn.
            selectedNode = node;
            ShowFeedback(string.Empty);

            bool occupied = selectedNode.IsOccupied;

            // Ô trống: hiện bảng xây.
            if (buildPanel != null)
            {
                buildPanel.SetActive(!occupied);
            }

            // Ô có tháp: hiện bảng quản lý.
            if (managePanel != null)
            {
                managePanel.SetActive(occupied);
            }

            if (occupied)
            {
                RefreshManagePanel();
            }
        }

        public void HideAll()
        {
            if (buildPanel != null)
            {
                buildPanel.SetActive(false);
            }

            if (managePanel != null)
            {
                managePanel.SetActive(false);
            }

            selectedNode = null;
            ShowFeedback(string.Empty);
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
            }
        }

        // Ba hàm này được gán vào On Click của ba nút xây.
        public void OnBuildArrowClicked() => TryBuild(arrowTowerPrefab);
        public void OnBuildCannonClicked() => TryBuild(cannonTowerPrefab);
        public void OnBuildIceClicked() => TryBuild(iceTowerPrefab);

        private void TryBuild(Tower prefab)
        {
            if (buildManager == null) return;

            bool success = buildManager.BuildTowerOn(selectedNode, prefab);

            // Xây thành công: mở lại giao diện theo trạng thái mới.
            if (success)
            {
                Open(selectedNode);
            }
        }

        public void OnUpgradeClicked()
        {
            if (buildManager == null) return;

            bool success = buildManager.UpgradeTowerOn(selectedNode);

            if (success)
            {
                ShowFeedback(string.Empty);
                RefreshManagePanel();
            }
        }

        public void OnSellClicked()
        {
            if (buildManager == null) return;

            bool success = buildManager.SellTowerOn(selectedNode);

            if (success)
            {
                HideAll();
            }
        }

        private void RefreshManagePanel()
        {
            if (selectedNode == null || !selectedNode.IsOccupied)
                return;

            Tower tower = selectedNode.CurrentTower;
            bool isMax = tower.IsMaxLevel();

            // Khóa nút nâng cấp khi tháp đã tối đa.
            if (upgradeButton != null)
            {
                upgradeButton.interactable = !isMax;
            }

            if (upgradeButtonText != null)
            {
                upgradeButtonText.text = isMax
                    ? "CẤP TỐI ĐA"
                    : $"Nâng cấp ({tower.GetUpgradeCost()}G)";
            }

            if (sellButtonText != null)
            {
                sellButtonText.text =
                    $"Bán (+{tower.GetSellPrice()}G)";
            }
        }
    }
}