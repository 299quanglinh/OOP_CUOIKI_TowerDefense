using System;
using System.Collections;
using UnityEngine;
using TowerDefense.Contracts;

namespace TowerDefense.Domain.Battle
{
    public class EnemyMovement : MonoBehaviour, ISlowable
    {
        public static event Action<int> OnEnemyReachedBase;

        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int baseDamage = 1;

        private Transform[] waypoints; // Danh sách các waypoints
        private int waypointIndex;
        private float originalSpeed; // Lưu tốc độ trước khi bị làm chậm
        private Coroutine slowCoroutine;
        private SpriteRenderer spriteRenderer; // dùng đổi màu khi quái bị làm chậm

        private void Awake()
        {
            originalSpeed = moveSpeed;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Transform[] route)
        {
            waypoints = route;
            waypointIndex = 0;
        }

        // Trúng đạn băng mới sẽ làm mới thời gian làm chậm
        public void ApplySlow(float slowPercent, float duration)
        {
            float clampedPercent = Mathf.Clamp(slowPercent, 0f, 0.9f);
            if (slowCoroutine != null) StopCoroutine(slowCoroutine);
            slowCoroutine = StartCoroutine(SlowRoutine(clampedPercent, Mathf.Max(0.1f, duration)));
        }

        private IEnumerator SlowRoutine(float slowPercent, float duration)
        {
            moveSpeed = originalSpeed * (1f - slowPercent);
            if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

            yield return new WaitForSeconds(duration);

            moveSpeed = originalSpeed;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            slowCoroutine = null;
        }

        private void Update()
        {
            if (waypoints == null || waypointIndex >= waypoints.Length) return;

            Transform targetPoint = waypoints[waypointIndex];
            float step = Mathf.Max(0f, moveSpeed) * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, step);

            // Dùng Vector2.Distance tránh lỗi kẹt do lệch trục Z trong 2D
            if (Vector2.Distance(transform.position, targetPoint.position) <= 0.02f)
            {
                waypointIndex++;
                if (waypointIndex >= waypoints.Length)
                {
                    ReachBase(targetPoint);
                }
            }
        }

        // Quái chạm đích trừ máu căn cứ (qua IDamageable/Event) và tự hủy không thưởng vàng
        private void ReachBase(Transform lastWaypoint)
        {
            if (lastWaypoint != null && lastWaypoint.TryGetComponent<IDamageable>(out var baseCore))
            {
                baseCore.TakeDamage(baseDamage);
            }
            else
            {
                OnEnemyReachedBase?.Invoke(baseDamage);
            }
            Destroy(gameObject);
        }
    }
} 
