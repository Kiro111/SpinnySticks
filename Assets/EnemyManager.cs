using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int health = 3; // �������� �����
    public bool isHit = false; // ����, �����������, ��� ���� ��� �������

    // ����� ��� ��������� ������������ � ��������
    public void TakeDamage()
    {
        health--;

        // ���� �������� ����� ������ ��� ����� ����, ���������� ���
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
