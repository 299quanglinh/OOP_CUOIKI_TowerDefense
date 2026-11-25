using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Domain.Battle;

namespace TowerDefense.Presentation
{
    public class BaseHealthUI : MonoBehaviour
    {
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Slider healthSlider;

        private void Start()
        {
            if (baseHealth != null)
            {
                baseHealth.OnHealthChanged += UpdateHealthBar;
                UpdateHealthBar(baseHealth.GetCurrentHealth(), baseHealth.GetMaxHealth());
            }
        }

        private void OnDestroy()
        {
            if (baseHealth != null) baseHealth.OnHealthChanged -= UpdateHealthBar;
        }

        private void UpdateHealthBar(int current, int max)
        {
            if (healthText != null) healthText.text = $"Căn cứ: {current} / {max}";
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
        }
    }
}
