using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f; // Скорость перемещения стикмена
    private Vector2 inputPosition; // Позиция ввода (касание или позиция мыши)
    private bool isMoving = false; // Флаг, указывающий на то, что стикмен движется

    private Animator animator; // Ссылка на компонент Animator

    void Start()
    {
        // Получаем компонент Animator из текущего объекта
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Обработка свайпов на экране или перемещения мыши
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Начало свайпа
            if (touch.phase == TouchPhase.Began)
            {
                inputPosition = touch.position;
                isMoving = true;
            }
            // Конец свайпа
            else if (touch.phase == TouchPhase.Ended)
            {
                isMoving = false;
            }
        }
        // Обработка перемещения мыши
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // Устанавливаем параметр "Walking" в Animator в зависимости от состояния движения
        animator.SetBool("Walking", isMoving);

        // Плавный поворот стикмена в сторону движения
        if (isMoving)
        {
            Vector3 movementDirection = new Vector3(inputPosition.x - Screen.width / 2f, 0f, inputPosition.y - Screen.height / 2f).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // Получение текущей позиции свайпа или курсора мыши
            Vector2 currentInputPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            // Вычисление направления движения
            Vector2 direction = currentInputPosition - inputPosition;

            // Нормализация направления движения
            direction.Normalize();

            // Применение силы для перемещения стикмена
            Vector3 movement = new Vector3(direction.x, 0f, direction.y) * moveSpeed * Time.fixedDeltaTime;
            transform.Translate(movement, Space.World);

            inputPosition = currentInputPosition;
        }
    }
}
