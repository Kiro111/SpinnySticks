using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public int health = 1; // �������� �����
    public bool isHit = false; // ����, �����������, ��� ���� ��� �������

    public GameObject player; // ������ �� GameObject ������

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

    void Update()
    {
        if (player != null)
        {
            // �������� ����������� � ������
            Vector3 directionToPlayer = player.transform.position - transform.position;

            // ����������� ������ ����������� (����� ���� �� ����������� � ��������� ���������)
            directionToPlayer.Normalize();

            // �������� ������ �� ��������� NavMeshAgent
            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            // ������������� ����������� �������� �����
            agent.SetDestination(player.transform.position);
        }
    }
}
