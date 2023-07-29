using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f; // Скорость перемещения стикмена
    public GameObject bulletPrefab; // Префаб для шарика
    public Transform firePoint; // Точка, из которой будет выпущен шарик
    public float bulletForce = 20f; // Сила, с которой шарик будет выпущен

    public float detectionRadius = 10f; // Радиус обнаружения врагов
    public float shootCooldown = 0.5f; // Время задержки между выстрелами

    public string enemyTag = "Enemy"; // Тег для обнаружения врагов

    public LineRenderer aimLineRenderer; // Ссылка на компонент LineRenderer для отображения луча прицеливания

    private Vector2 inputPosition; // Позиция ввода (касание или позиция мыши)
    private bool isMoving = false; // Флаг, указывающий на то, что стикмен движется
    private bool isAiming = false; // Флаг, указывающий на то, что игрок прицеливается
    private bool canShoot = false; // Флаг, указывающий на возможность стрельбы
    private float lastShootTime = 0f; // Время последнего выстрела

    private Animator playerAnimator; // Ссылка на компонент Animator (необходимо добавить)

    private int bulletCount = 400; // Количество патронов у игрока
    private bool isShooting = false; // Флаг, указывающий, стреляет ли игрок в данный момент

    private bool canAimAndShoot = false;


    void Start()
    {
        // Получаем компонент Animator из текущего объекта
        playerAnimator = GetComponent<Animator>();

        // Инициализируем LineRenderer, если он есть
        if (aimLineRenderer != null)
        {
            aimLineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (isAiming && aimLineRenderer != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, detectionRadius, LayerMask.GetMask("Enemies")))
            {
                // Если лучевое трассирование попало на врага, разрешаем игроку стрелять
                canShoot = true;
            }
            else
            {
                // Если лучевое трассирование не попало на врага, запрещаем стрельбу
                canShoot = false;
            }
        }
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
        if (isMoving && isShooting)
        {
            playerAnimator.SetBool("Shoot", true);
        }
        else
        { 
            playerAnimator.SetBool("Walking", isMoving);
        }
        // Плавный поворот стикмена в сторону движения
        if (isMoving)
        {
            Vector3 movementDirection = new Vector3(inputPosition.x - Screen.width / 2f, 0f, inputPosition.y - Screen.height / 2f).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Проверяем, есть ли враги в радиусе обнаружения и стреляем на них
        Collider[] hitColliders = new Collider[10]; // Максимальное количество коллайдеров для обнаружения
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitColliders);
        Transform closestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].CompareTag(enemyTag))
            {
                // Находим ближайшего врага
                Transform enemyTransform = hitColliders[i].transform;
                float distanceSqr = (enemyTransform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestEnemy = enemyTransform;
                }
            }
        }

        // Если есть ближайший враг, стреляем на него
        if (closestEnemy != null)
        {
            // Поворачиваемся в сторону врага
            Vector3 directionToEnemy = closestEnemy.position - transform.position;
            directionToEnemy.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            // Прицеливаемся на врага
            isAiming = true;

            // Проверяем, прошла ли задержка между выстрелами
            if (Time.time >= lastShootTime + shootCooldown)
            {
                // Разрешаем стрельбу, только если успешно прицелились и у игрока есть патроны
                canShoot = true;
            }
            else
            {
                // Запрещаем стрельбу, если время задержки между выстрелами еще не прошло
                canShoot = false;
            }
        }
        else
        {
            // Если врагов в радиусе обнаружения нет, сбрасываем прицеливание и возможность стрельбы
            isAiming = false;
            canShoot = false;
            playerAnimator.SetBool("Shoot", false);
        }

        // Проверяем, может ли игрок прицелиться и стрелять
        canAimAndShoot = isAiming && canShoot;

        // Если игрок прицеливается и может стрелять, то стреляем
        if (canAimAndShoot)
        {
            ShootBullet();
            isShooting = true;
        }

        // Отображаем луч прицеливания, если есть LineRenderer
        if (aimLineRenderer != null)
        {
            if (isAiming)
            {
                aimLineRenderer.enabled = true;
                aimLineRenderer.SetPosition(0, firePoint.position);
                aimLineRenderer.SetPosition(1, firePoint.position + firePoint.forward * detectionRadius);
            }
            else
            {
                aimLineRenderer.enabled = false;
            }
        }

 
    }

    void ShootBullet()
    {
        // Если у игрока есть патроны, то выполняем стрельбу
        if (bulletCount > 0)
        {
            // Создаем шарик и задаем ему силу
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

            // Уменьшаем количество патронов
            bulletCount--;

            // Обновляем время последнего выстрела
            lastShootTime = Time.time;

            // Указываем, что игрок стреляет (используется для анимации)
            isShooting = true;

            // Запрещаем стрельбу после выстрела, чтобы игрок мог стрелять только после прицеливания
            canShoot = false;

            // Активируем параметр "Shoot" в аниматоре, чтобы начать анимацию стрельбы
            playerAnimator.SetBool("Shoot", true);

        }
        else
        {
            // Если у игрока нет патронов, прекращаем стрельбу
            isShooting = false;
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

    // Отрисовываем радиус обнаружения для наглядности в редакторе Unity
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // Отрисовываем радиус обнаружения для наглядности в редакторе Unity


    private void OnTriggerEnter(Collider other)
    {
        // Получаем компонент EnemyManager у столкнувшегося объекта (если он есть)
        EnemyManager enemyManager = other.GetComponent<EnemyManager>();

        // Если объект имеет компонент EnemyManager и игрок стрелял по врагу
        if (enemyManager != null && !enemyManager.isHit)
        {
            // Наносим урон врагу
            enemyManager.TakeDamage();
            enemyManager.isHit = true; // Помечаем врага, чтобы игрок не наносил ему повторный урон
        }
    }

    // Метод для увеличения количества патронов (например, при подборе патронов в игре)
    public void IncreaseBulletCount(int amount)
    {
        bulletCount += amount;
        // Можно добавить здесь дополнительные проверки, чтобы количество патронов не превышало максимальное значение и т. д.
    }

}
