using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f; // �������� ����������� ��������
    private Vector2 inputPosition; // ������� ����� (������� ��� ������� ����)
    private bool isMoving = false; // ����, ����������� �� ��, ��� ������� ��������

    private Animator animator; // ������ �� ��������� Animator

    void Start()
    {
        // �������� ��������� Animator �� �������� �������
        animator = GetComponent<Animator>();
    }

    void Update()
    {
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
        animator.SetBool("Walking", isMoving);

        // ������� ������� �������� � ������� ��������
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
}
