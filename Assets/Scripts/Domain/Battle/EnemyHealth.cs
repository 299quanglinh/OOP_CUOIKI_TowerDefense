using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain.Battle
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int goldReward = 25;

        private int currentHealth;

        public bool IsAlive { get; private set; } = true;
        public bool IsEnemy => true;
        public int GetCurrentHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public Vector3 GetPosition() => transform.position;

        private void Awake()
        {
            currentHealth = maxHealth;
            CombatRegistry.RegisterEnemy(this);
        }

        private void OnDestroy()
        {
            IsAlive = false;
            CombatRegistry.UnregisterEnemy(this);
        }

        // Cấu hình
        public void Configure(int hp, int reward)
        {
            maxHealth = Mathf.Max(1, hp);
            currentHealth = maxHealth;
            goldReward = Mathf.Max(0, reward);
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive || damage <= 0) return;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            if (currentHealth == 0) Die();
        }

        // Observer Pattern chuẩn: chỉ phát sự kiện qua Contracts/, không gọi trực tiếp phân hệ Kinh tế
        private void Die()
        {
            if (!IsAlive) return;
            IsAlive = false;
            CombatRegistry.NotifyEnemyKilled(this, goldReward);
            Destroy(gameObject);
        }
    }
}
