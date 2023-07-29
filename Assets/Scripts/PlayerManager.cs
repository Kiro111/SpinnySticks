using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f; // �������� ����������� ��������
    public GameObject bulletPrefab; // ������ ��� ������
    public Transform firePoint; // �����, �� ������� ����� ������� �����
    public float bulletForce = 20f; // ����, � ������� ����� ����� �������

    public float detectionRadius = 10f; // ������ ����������� ������
    public float shootCooldown = 0.5f; // ����� �������� ����� ����������

    public string enemyTag = "Enemy"; // ��� ��� ����������� ������

    public LineRenderer aimLineRenderer; // ������ �� ��������� LineRenderer ��� ����������� ���� ������������

    private Vector2 inputPosition; // ������� ����� (������� ��� ������� ����)
    private bool isMoving = false; // ����, ����������� �� ��, ��� ������� ��������
    private bool isAiming = false; // ����, ����������� �� ��, ��� ����� �������������
    private bool canShoot = false; // ����, ����������� �� ����������� ��������
    private float lastShootTime = 0f; // ����� ���������� ��������

    private Animator playerAnimator; // ������ �� ��������� Animator (���������� ��������)

    private int bulletCount = 400; // ���������� �������� � ������
    private bool isShooting = false; // ����, �����������, �������� �� ����� � ������ ������

    private bool canAimAndShoot = false;


    void Start()
    {
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
            }
            // ����� ������
            else if (touch.phase == TouchPhase.Ended)
            {
                isMoving = false;
            }
        }
        // ��������� ����������� ����
        else if (Input.GetMouseButton(0))
        {
            inputPosition = Input.mousePosition;
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // ������������� �������� "Walking" � Animator � ����������� �� ��������� ��������
        if (isMoving && isShooting)
        {
            playerAnimator.SetBool("Shoot", true);
        }
        else
        { 
            playerAnimator.SetBool("Walking", isMoving);
        }
        // ������� ������� �������� � ������� ��������
        if (isMoving)
        {
            Vector3 movementDirection = new Vector3(inputPosition.x - Screen.width / 2f, 0f, inputPosition.y - Screen.height / 2f).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

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
            directionToEnemy.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            // ������������� �� �����
            isAiming = true;

            // ���������, ������ �� �������� ����� ����������
            if (Time.time >= lastShootTime + shootCooldown)
            {
                // ��������� ��������, ������ ���� ������� ����������� � � ������ ���� �������
                canShoot = true;
            }
            else
            {
                // ��������� ��������, ���� ����� �������� ����� ���������� ��� �� ������
                canShoot = false;
            }
        }
        else
        {
            // ���� ������ � ������� ����������� ���, ���������� ������������ � ����������� ��������
            isAiming = false;
            canShoot = false;
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
        }

 
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
            playerAnimator.SetBool("Shoot", true);

        }
        else
        {
            // ���� � ������ ��� ��������, ���������� ��������
            isShooting = false;
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            // ��������� ������� ������� ������ ��� ������� ����
            Vector2 currentInputPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

            // ���������� ����������� ��������
            Vector2 direction = currentInputPosition - inputPosition;

            // ������������ ����������� ��������
            direction.Normalize();

            // ���������� ���� ��� ����������� ��������
            Vector3 movement = new Vector3(direction.x, 0f, direction.y) * moveSpeed * Time.fixedDeltaTime;
            transform.Translate(movement, Space.World);

            inputPosition = currentInputPosition;
        }
    }

    // ������������ ������ ����������� ��� ����������� � ��������� Unity
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // ������������ ������ ����������� ��� ����������� � ��������� Unity


    private void OnTriggerEnter(Collider other)
    {
        // �������� ��������� EnemyManager � �������������� ������� (���� �� ����)
        EnemyManager enemyManager = other.GetComponent<EnemyManager>();

        // ���� ������ ����� ��������� EnemyManager � ����� ������� �� �����
        if (enemyManager != null && !enemyManager.isHit)
        {
            // ������� ���� �����
            enemyManager.TakeDamage();
            enemyManager.isHit = true; // �������� �����, ����� ����� �� ������� ��� ��������� ����
        }
    }

    // ����� ��� ���������� ���������� �������� (��������, ��� ������� �������� � ����)
    public void IncreaseBulletCount(int amount)
    {
        bulletCount += amount;
        // ����� �������� ����� �������������� ��������, ����� ���������� �������� �� ��������� ������������ �������� � �. �.
    }

}
