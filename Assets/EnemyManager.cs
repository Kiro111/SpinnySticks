using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public int health = 1; // Здоровье врага
    public bool isHit = false; // Флаг, указывающий, что враг был поражен

    public GameObject player; // Ссылка на GameObject игрока

    // Метод для обработки столкновений с шариками
    public void TakeDamage()
    {
        health--;

        // Если здоровье врага меньше или равно нулю, уничтожаем его
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (player != null)
        {
            // Получаем направление к игроку
            Vector3 directionToPlayer = player.transform.position - transform.position;

            // Нормализуем вектор направления (чтобы враг не перемещался с различной скоростью)
            directionToPlayer.Normalize();

            // Получаем ссылку на компонент NavMeshAgent
            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            // Устанавливаем направление движения врага
            agent.SetDestination(player.transform.position);
        }
    }
}
