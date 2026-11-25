using System;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain.Battle
{
    public class BaseHealth : MonoBehaviour, IDamageable
    {
        public event Action<int, int> OnHealthChanged;
        public event Action OnBaseDestroyed;

        [SerializeField] private int maxHealth = 20;
        private int currentHealth;

        public bool IsAlive { get; private set; } = true;
        public bool IsEnemy => false;
        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public Vector3 GetPosition() => transform.position;

        private void Awake()
        {
            currentHealth = Mathf.Max(1, maxHealth);
            IsAlive = true;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive || damage <= 0) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth == 0)
            {
                IsAlive = false;
                OnBaseDestroyed?.Invoke();
            }
        }

#if UNITY_EDITOR
        // Chỉ dùng để test trong Editor: chuột phải component -> "Test: -1 HP"
        [ContextMenu("Test: -1 HP")]
        private void DebugHit() => TakeDamage(1);
#endif
    }
}
