using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Метод, вызываемый при столкновении с другим коллайдером
    private void OnTriggerEnter(Collider other)
    {
        // Получаем компонент EnemyManager у столкнувшегося объекта (если он есть)
        EnemyManager enemyManager = other.GetComponent<EnemyManager>();

        // Если объект имеет компонент EnemyManager, значит, это враг
        if (enemyManager != null)
        {
            // Наносим урон врагу и уничтожаем шарик
            enemyManager.TakeDamage();
            Destroy(gameObject);
        }
        else 
            Destroy(gameObject,3);
    }
}
