using UnityEngine;

public class Bullet : MonoBehaviour
{
    // �����, ���������� ��� ������������ � ������ �����������
    private void OnTriggerEnter(Collider other)
    {
        // �������� ��������� EnemyManager � �������������� ������� (���� �� ����)
        EnemyManager enemyManager = other.GetComponent<EnemyManager>();

        // ���� ������ ����� ��������� EnemyManager, ������, ��� ����
        if (enemyManager != null)
        {
            // ������� ���� ����� � ���������� �����
            enemyManager.TakeDamage();
            Destroy(gameObject);
        }
        else 
            Destroy(gameObject,3);
    }
}
