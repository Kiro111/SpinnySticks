using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Ссылка на трансформ персонажа, за которым камера должна следовать
    public Vector3 offset = new Vector3(0f, 5f, -5f); // Смещение камеры относительно персонажа
    public float smoothSpeed = 5f; // Скорость плавного следования камеры

    // Последовательность выполнения скриптов фиксирована, поэтому используем LateUpdate
    void LateUpdate()
    {
        if (target != null)
        {
            // Вычисляем целевую позицию камеры с учетом смещения относительно персонажа
            Vector3 desiredPosition = target.position + offset;

            // Плавно перемещаем камеру к новой позиции с помощью интерполяции
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Ориентируем камеру на персонаж
            transform.LookAt(target);
        }
    }
}
