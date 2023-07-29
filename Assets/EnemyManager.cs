using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int health = 3; // «доровье врага
    public bool isHit = false; // ‘лаг, указывающий, что враг был поражен

    // ћетод дл€ обработки столкновений с шариками
    public void TakeDamage()
    {
        health--;

        // ≈сли здоровье врага меньше или равно нулю, уничтожаем его
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
