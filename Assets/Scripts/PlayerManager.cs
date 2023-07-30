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
   
    public float moveSpeed = 5f; // �������� ����������� ��������
    public GameObject bulletPrefab; // ������ ��� ������
    public Transform firePoint; // �����, �� ������� ����� ������� �����
    public float bulletForce = 20f; // ����, � ������� ����� ����� �������

    public float detectionRadius = 10f; // ������ ����������� ������
    public float shootCooldown = 0.5f; // ����� �������� ����� ����������

    public string enemyTag = "Enemy"; // ��� ��� ����������� ������

    public LineRenderer aimLineRenderer; // ������ �� ��������� LineRenderer ��� ����������� ���� ������������

    private Vector2 inputPosition; // ������� ����� (������� ��� ������� ����)
    public bool isMoving = false; // ����, ����������� �� ��, ��� ������� ��������
    private bool isAiming = false; // ����, ����������� �� ��, ��� ����� �������������
    private bool canShoot = false; // ����, ����������� �� ����������� ��������
    private float lastShootTime = 0f; // ����� ���������� ��������

    private Animator playerAnimator; // ������ �� ��������� Animator (���������� ��������)

    private int bulletCount = 400; // ���������� �������� � ������
    private bool isShooting = false; // ����, �����������, �������� �� ����� � ������ ������

    private bool canAimAndShoot = false;
    public CinemachineFreeLook virtualCamera;

    void Start()
    {
        virtualCamera = FindObjectOfType<CinemachineFreeLook>();
        // �������� ��������� Animator �� �������� �������
        playerAnimator = GetComponent<Animator>();

        // �������������� LineRenderer, ���� �� ����
        if (aimLineRenderer != null)
        {
            aimLineRenderer.enabled = false;
        }
    }

    void Update()
    {

        if (virtualCamera != null)
        {
            // ������������� ������� � ���������� Cinemachine � ������������ � ����������.
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }

        //HandleJoystick();


        if (isAiming && aimLineRenderer != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, detectionRadius, LayerMask.GetMask("Enemies")))
            {
                // ���� ������� ������������� ������ �� �����, ��������� ������ ��������
                canShoot = true;
            }
            else
            {
                // ���� ������� ������������� �� ������ �� �����, ��������� ��������
                canShoot = false;
            }
        }

        // ��������� ������� �� ������ ��� ����������� ����
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // ������ ������
            if (touch.phase == TouchPhase.Began)
            {
                inputPosition = touch.position;

                isMoving = true;
                FixedUpdate();

            }
            // ����� ������
            else if (touch.phase == TouchPhase.Ended)
            {
                isMoving = false;
                playerAnimator.SetBool("Walking", true);
            }
        }
        // ��������� ����������� ����
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

        // ������� ������� �������� � ������� ��������
  
        // ���������, ���� �� ����� � ������� ����������� � �������� �� ���
        Collider[] hitColliders = new Collider[10]; // ������������ ���������� ����������� ��� �����������
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitColliders);
            Transform closestEnemy = null;
            float closestDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < numColliders; i++)
            {
                if (hitColliders[i].CompareTag(enemyTag))
                {
                    // ������� ���������� �����
                    Transform enemyTransform = hitColliders[i].transform;
                    float distanceSqr = (enemyTransform.position - transform.position).sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestEnemy = enemyTransform;
                    }
                }
            }

        // ���� ���� ��������� ����, �������� �� ����
        if (closestEnemy != null)
        {
            // �������������� � ������� �����
            Vector3 directionToEnemy = closestEnemy.position - transform.position;
            directionToEnemy.y = 0f; // ��������� ������ �������������� �����������
            Quaternion targetRotation1 = Quaternion.LookRotation(directionToEnemy);

            // ��������� ������� ��� ������� ������������
            transform.rotation = targetRotation1;

            // ������������� �� �����
            isAiming = true;

           
            
                playerAnimator.SetBool("Shooting", true);
            

            // ���������, ������ �� �������� ����� ����������
            if (Time.time >= lastShootTime + shootCooldown)
            {
                // ��������� ��������, ������ ���� ������� ����������� � � ������ ���� �������
                canShoot = true;
            }
        }

        else
        {
                // ���� ������ � ������� ����������� ���, ���������� ������������ � ����������� ��������
                isAiming = false;
                canShoot = false;
                playerAnimator.SetBool("Shooting", false);
                playerAnimator.SetBool("Shoot", false);
        }

            // ���������, ����� �� ����� ����������� � ��������
            canAimAndShoot = isAiming && canShoot;

            // ���� ����� ������������� � ����� ��������, �� ��������
            if (canAimAndShoot)
            {
                ShootBullet();
                isShooting = true;
            }

            // ���������� ��� ������������, ���� ���� LineRenderer
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
    //    // ���� �� ������ ���� �������
    //    if (Input.touchCount > 0)
    //    {
    //        Touch touch = Input.GetTouch(0);

    //        // ���� �������� �������, ���������� �������� � ������������� ��� �������
    //        if (touch.phase == TouchPhase.Began)
    //        {
    //            inputPosition = touch.position;
    //            isJoystickActive = true;

    //            // ��������� ������� ������� � ��������� ���������� Canvas
    //            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, inputPosition, canvas.worldCamera, out Vector2 localPosition);

    //            // ������������� ������� ��������� � Canvas
    //            joystick.transform.localPosition = localPosition;
    //        }
    //        // ���� ������� �����������, ������������ ��������
    //        else if (touch.phase == TouchPhase.Ended)
    //        {
    //            isJoystickActive = false;
    //            playerAnimator.SetBool("Walking", true);
    //            joystick.gameObject.SetActive(false); // ��������� �������� ����� ��������� �������
    //        }
    //    }
    //    // ���� ���� ������ ����, ������������ ��������� ��������� ��� ������������ �� ��
    //    else if (Input.GetMouseButtonDown(0))
    //    {
    //        inputPosition = Input.mousePosition;
    //        isJoystickActive = true;

    //        // ��������� ������� ������� � ��������� ���������� Canvas
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, inputPosition, canvas.worldCamera, out Vector2 localPosition);

    //        // ������������� ������� ��������� � Canvas
    //        joystick.transform.localPosition = localPosition;
    //    }

    //    // �������� ��� ��������� �������� � ������������ � ��� ����������
    //    joystick.gameObject.SetActive(isJoystickActive);

    //    // ���� �������� �������, ������� ������
    //    if (isJoystickActive)
    //    {
    //        FixedUpdate();
    //    }
    //}







    private Quaternion targetRotation; // ��������� ����� ���������� ��� �������� �������� ��������

   

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

        // ��������� ������� ������� ��� ������� ������������
        transform.rotation = targetRotation;
    }
    //    if (Input.touchCount > 0)
    //    {
    //        Vector2 currentInputPosition = Input.GetTouch(0).position;
    //        // ���������� ����������� ��������
    //        Vector2 direction = currentInputPosition - inputPosition;

    //        // ������������ ����������� ��������
    //        direction.Normalize();

    //        // ���������� ���� ��� ����������� ��������
    //        Vector3 movement = new Vector3(direction.x, 0f, direction.y) * moveSpeed * Time.fixedDeltaTime;
    //        transform.Translate(movement, Space.World);

    //        inputPosition = currentInputPosition;
    //        lastDirection = direction;
    //    }
    //    else
    //    {
    //        // ���� ��� �������� �������, ������� �������� � ��������� ����������� �����������
    //        Vector3 movement = new Vector3(lastDirection.x, 0f, lastDirection.y) * moveSpeed * Time.fixedDeltaTime;
    //        transform.Translate(movement, Space.World);

    //    }


    //������� ������������� inputPosition, ��� ������ ��������� ��������� ������� ������� � ������������ �� ��� ���������� ����������� ��������.������ ��� ������� ������ ��������� � ��������� ����������� �����������, ����� �� ������ ��� �������� �������.







    // ������������ ������ ����������� ��� ����������� � ��������� Unity
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // ����� ��� ���������� ���������� �������� (��������, ��� ������� �������� � ����)
    public void IncreaseBulletCount(int amount)
    {
        bulletCount += amount;
        // ����� �������� ����� �������������� ��������, ����� ���������� �������� �� ��������� ������������ �������� � �. �.
    }

    // ����������, ����� ����� ���� ����� ���������� �������� (��������, ����� ������ ��������� ������)
    public void StopMoving()
    {
        isMoving = false;
    }
 

    void ShootBullet()
    {
        // ���� � ������ ���� �������, �� ��������� ��������
        if (bulletCount > 0)
        {
            // ������� ����� � ������ ��� ����
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);

            // ��������� ���������� ��������
            bulletCount--;

            // ��������� ����� ���������� ��������
            lastShootTime = Time.time;

            // ���������, ��� ����� �������� (������������ ��� ��������)
            isShooting = true;

            // ��������� �������� ����� ��������, ����� ����� ��� �������� ������ ����� ������������
            canShoot = false;

            // ���������� �������� "Shoot" � ���������, ����� ������ �������� ��������
            playerAnimator.SetBool("Shooting", true);

        }
       
        else
        {
            // ���� � ������ ��� ��������, ���������� ��������
            isShooting = false;
            playerAnimator.SetBool("Shooting", false);
            playerAnimator.SetBool("Shoot", false);
        }
    }
}
