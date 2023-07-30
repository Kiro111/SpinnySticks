using UnityEngine;
using System.Collections;
using Cinemachine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))] 

public class PlayerManager : MonoBehaviour

{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private DynamicJoystick joystick;
    private bool isJoystickActive = false;
   
    public float moveSpeed = 5f; // Скорость перемещения стикмена
    public GameObject bulletPrefab; // Префаб для шарика
    public Transform firePoint; // Точка, из которой будет выпущен шарик
    public float bulletForce = 20f; // Сила, с которой шарик будет выпущен

    public float detectionRadius = 10f; // Радиус обнаружения врагов
    public float shootCooldown = 0.5f; // Время задержки между выстрелами

    public string enemyTag = "Enemy"; // Тег для обнаружения врагов

    public LineRenderer aimLineRenderer; // Ссылка на компонент LineRenderer для отображения луча прицеливания

    private Vector2 inputPosition; // Позиция ввода (касание или позиция мыши)
    public bool isMoving = false; // Флаг, указывающий на то, что стикмен движется
    private bool isAiming = false; // Флаг, указывающий на то, что игрок прицеливается
    private bool canShoot = false; // Флаг, указывающий на возможность стрельбы
    private float lastShootTime = 0f; // Время последнего выстрела

    private Animator playerAnimator; // Ссылка на компонент Animator (необходимо добавить)

    private int bulletCount = 400; // Количество патронов у игрока
    private bool isShooting = false; // Флаг, указывающий, стреляет ли игрок в данный момент

    private bool canAimAndShoot = false;
    public CinemachineFreeLook virtualCamera;

    void Start()
    {
        virtualCamera = FindObjectOfType<CinemachineFreeLook>();
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

        if (virtualCamera != null)
        {
            // Устанавливаем позицию и ориентацию Cinemachine в соответствии с персонажем.
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }

        //HandleJoystick();


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
                FixedUpdate();

            }
            // Конец свайпа
            else if (touch.phase == TouchPhase.Ended)
            {
                isMoving = false;
                playerAnimator.SetBool("Walking", true);
            }
        }
        // Обработка перемещения мыши
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            isMoving = true;
            FixedUpdate();




        }
        //else
        //{
        //    isMoving = false;
        //    playerAnimator.SetBool("Walking", true);
        //}

       if (isMoving)
       {
         playerAnimator.SetBool("Walking", true);

       }
       else
       {
          playerAnimator.SetBool("Walking", false);
       }

        // Плавный поворот стикмена в сторону движения
  
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
            directionToEnemy.y = 0f; // Оставляем только горизонтальное направление
            Quaternion targetRotation1 = Quaternion.LookRotation(directionToEnemy);

            // Применяем поворот без плавной интерполяции
            transform.rotation = targetRotation1;

            // Прицеливаемся на врага
            isAiming = true;

           
            
                playerAnimator.SetBool("Shooting", true);
            

            // Проверяем, прошла ли задержка между выстрелами
            if (Time.time >= lastShootTime + shootCooldown)
            {
                // Разрешаем стрельбу, только если успешно прицелились и у игрока есть патроны
                canShoot = true;
            }
        }

        else
        {
                // Если врагов в радиусе обнаружения нет, сбрасываем прицеливание и возможность стрельбы
                isAiming = false;
                canShoot = false;
                playerAnimator.SetBool("Shooting", false);
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
                //}
            }
    }

    //private void StartMoving(int v)
    //{
    //    throw new NotImplementedException();
    //}


    //private void HandleJoystick()
    //{
    //    // Если на экране есть касания
    //    if (Input.touchCount > 0)
    //    {
    //        Touch touch = Input.GetTouch(0);

    //        // Если началось касание, активируем джойстик и устанавливаем его позицию
    //        if (touch.phase == TouchPhase.Began)
    //        {
    //            inputPosition = touch.position;
    //            isJoystickActive = true;

    //            // Переводим позицию касания в локальные координаты Canvas
    //            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, inputPosition, canvas.worldCamera, out Vector2 localPosition);

    //            // Устанавливаем позицию джойстика в Canvas
    //            joystick.transform.localPosition = localPosition;
    //        }
    //        // Если касание закончилось, деактивируем джойстик
    //        else if (touch.phase == TouchPhase.Ended)
    //        {
    //            isJoystickActive = false;
    //            playerAnimator.SetBool("Walking", true);
    //            joystick.gameObject.SetActive(false); // Отключаем джойстик после окончания касания
    //        }
    //    }
    //    // Если есть щелчок мыши, обрабатываем активацию джойстика для тестирования на ПК
    //    else if (Input.GetMouseButtonDown(0))
    //    {
    //        inputPosition = Input.mousePosition;
    //        isJoystickActive = true;

    //        // Переводим позицию касания в локальные координаты Canvas
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, inputPosition, canvas.worldCamera, out Vector2 localPosition);

    //        // Устанавливаем позицию джойстика в Canvas
    //        joystick.transform.localPosition = localPosition;
    //    }

    //    // Включаем или отключаем джойстик в соответствии с его состоянием
    //    joystick.gameObject.SetActive(isJoystickActive);

    //    // Если джойстик активен, двигаем игрока
    //    if (isJoystickActive)
    //    {
    //        FixedUpdate();
    //    }
    //}







    private Quaternion targetRotation; // Добавляем новую переменную для хранения целевого поворота

   

    void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(joystick.Horizontal * moveSpeed, _rigidbody.velocity.y, joystick.Vertical * moveSpeed);

        if (joystick.Horizontal != 0 || joystick.Vertical != 0)
        {
            Vector3 movementDirection = new Vector3(joystick.Horizontal, 0f, joystick.Vertical).normalized;
            if (movementDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(movementDirection);
            }
        }

        // Применяем целевой поворот без плавной интерполяции
        transform.rotation = targetRotation;
    }
    //    if (Input.touchCount > 0)
    //    {
    //        Vector2 currentInputPosition = Input.GetTouch(0).position;
    //        // Вычисление направления движения
    //        Vector2 direction = currentInputPosition - inputPosition;

    //        // Нормализация направления движения
    //        direction.Normalize();

    //        // Применение силы для перемещения стикмена
    //        Vector3 movement = new Vector3(direction.x, 0f, direction.y) * moveSpeed * Time.fixedDeltaTime;
    //        transform.Translate(movement, Space.World);

    //        inputPosition = currentInputPosition;
    //        lastDirection = direction;
    //    }
    //    else
    //    {
    //        // Если нет активных касаний, двигаем стикмена в последнем запомненном направлении
    //        Vector3 movement = new Vector3(lastDirection.x, 0f, lastDirection.y) * moveSpeed * Time.fixedDeltaTime;
    //        transform.Translate(movement, Space.World);

    //    }


    //Добавив инициализацию inputPosition, код должен правильно сохранять позицию касания и использовать ее для вычисления направления движения.Теперь ваш стикмен должен двигаться в последнем запомненном направлении, когда на экране нет активных касаний.







    // Отрисовываем радиус обнаружения для наглядности в редакторе Unity
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // Метод для увеличения количества патронов (например, при подборе патронов в игре)
    public void IncreaseBulletCount(int amount)
    {
        bulletCount += amount;
        // Можно добавить здесь дополнительные проверки, чтобы количество патронов не превышало максимальное значение и т. д.
    }

    // Вызывается, когда игрок явно хочет остановить движение (например, когда кнопка остановки нажата)
    public void StopMoving()
    {
        isMoving = false;
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
            playerAnimator.SetBool("Shooting", true);

        }
       
        else
        {
            // Если у игрока нет патронов, прекращаем стрельбу
            isShooting = false;
            playerAnimator.SetBool("Shooting", false);
            playerAnimator.SetBool("Shoot", false);
        }
    }
}
