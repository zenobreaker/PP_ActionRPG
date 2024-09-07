using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PerceptionComponent : MonoBehaviour
{
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float angle = 45.0f;
    [SerializeField] private float lostTime = 2.0f;
    [SerializeField] private LayerMask layerMask;

    // ������ ������ ��� ���̺� 
    private Dictionary<GameObject, float> percievedTable;

    private void Reset()
    {
        layerMask = 1 << LayerMask.NameToLayer("Character");
    }

    private void Awake()
    {
        percievedTable = new Dictionary<GameObject, float>();
    }
    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, layerMask);

        Vector3 forward = transform.forward;
        List<Collider> candidateList = new List<Collider>();

        //1. ���� ���ǿ� �´� ����� ����
        foreach (Collider collider in colliders)
        {
            Vector3 direction = collider.transform.position - transform.position;
            float signedAngle = Vector3.SignedAngle(forward, direction.normalized, Vector3.up);

            if (Mathf.Abs(signedAngle) <= angle)
                candidateList.Add(collider);
        }

        //candidateList.ForEach(collider => print(collider.name));

        //2. ���� ����� ��� �� �ð� ������Ʈ
        foreach (Collider collider in candidateList)
        {
            if (percievedTable.ContainsKey(collider.gameObject) == false)
            {
                percievedTable.Add(collider.gameObject, Time.realtimeSinceStartup);

                continue;
            }

            percievedTable[collider.gameObject] = Time.realtimeSinceStartup;
        }

        //3. �ð� �ʰ� ����� ���� �� ���� 
        List<GameObject> removeList = new List<GameObject>();
        foreach (var item in percievedTable)
        {
            if (Time.realtimeSinceStartup - item.Value >= lostTime)
                removeList.Add(item.Key);
        }

        removeList.RemoveAll(remove => percievedTable.Remove(remove));
    }

    public GameObject GetPercievedPlayer()
    {
        foreach (var item in percievedTable)
        {
            if (item.Key.CompareTag("Player"))
                return item.Key;
        }

        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distance);

        Gizmos.color = Color.blue;

        Vector3 direction = Vector3.zero;
        Vector3 forward = transform.forward;
        // ���� ȸ�� ���� ��� 
        direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);

        direction = Quaternion.AngleAxis(-angle, Vector3.up) * forward;
        Gizmos.DrawLine(transform.position, transform.position + direction.normalized * distance);


        GameObject player = GetPercievedPlayer();
        if (player == null)
            return;

        Gizmos.color = Color.magenta;

        Vector3 position = transform.position;
        position.y += 1.0f;

        Vector3 playerPosition = player.transform.position;
        playerPosition.y += 1.0f;

        Gizmos.DrawLine(position, playerPosition);
        Gizmos.DrawWireSphere(playerPosition, 0.25f);
    }

#endif
}
