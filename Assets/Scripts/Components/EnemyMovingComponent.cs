using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  Enemy���� �ٿ��� Enemy���� �̵��� ���
public class EnemyMovingComponent : MonoBehaviour
{
    [SerializeField]
    private float speed = 5.0f; // �̵��ӵ� 

    [SerializeField]
    private float minArriveDis = 1.5f; // Ư�� ���� ���� �ּ� �Ÿ�

    [SerializeField]
    private float rotateSpeed = 1.0f;
    private float deltaRotateSpeed;

    [SerializeField]
    private StateComponent state;


    [SerializeField]
    private Animator animator;

    private bool bCanMove = true;
    public bool CanMove { get => bCanMove; set => bCanMove = value; }

    private bool bArrived = false;
    public bool Arrived { get => bArrived; }

    private GameObject target;
    public GameObject Target { get => target; set => target = value; }


    public void Move()
    {
        bCanMove = true;
    }

    public void Stop()
    {
        bCanMove = false;
    }

    private void Awake()
    {
        state = GetComponent<StateComponent>(); 

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (state.IdleMode == false)
            return; 

        Update_CheckArrive();
        Update_RotateToTarget();
        Update_MoveToTarget();
    }

    public void Update_MoveToTarget()
    {
        if (target == null)
            bCanMove = false; 

        if (bCanMove == false)
        {
            animator.SetFloat("SpeedY", 0);
            return;
        }

        Vector3 direction = target.transform.position - this.transform.position;
        direction = direction.normalized * speed;
        //controller.Move(direction * Time.deltaTime);
        transform.Translate(direction * Time.deltaTime);
        animator.SetFloat("SpeedY",speed);
    }

    public void Update_RotateToTarget()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.transform.position;
        Vector3 direction = targetPosition - transform.position;

        direction = direction.normalized;
        // ������ �������� ������.
        //Quaternion targetRatation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion from = transform.localRotation;
        Quaternion to = Quaternion.LookRotation(direction, Vector3.up);

        // ȸ������ �����̻� ���� �� ������ ó�� 
        if (Quaternion.Angle(from, to) < 2.0f)
        {
            deltaRotateSpeed = 0.0f;
            transform.localRotation = to;
            return;
        }

        deltaRotateSpeed += rotateSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.RotateTowards(from, to, deltaRotateSpeed);
    }

    public void Update_CheckArrive()
    {
        bArrived = false;
        bCanMove = true;

        if (target == null)
            return;

        if (Vector3.Distance(this.transform.position, target.transform.position) <= minArriveDis)
        {
            bCanMove = false;
            bArrived = true;
        }
    }

}
