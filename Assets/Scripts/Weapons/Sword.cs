using System.Collections;
using UnityEngine;

public class Sword : Melee
{
    [SerializeField]
    private string holsterName = "Holster_Sword";
    [SerializeField]
    private string handName = "Hand_Sword";

    private Transform holsterTransform;
    private Transform handTransform;

    private Rigidbody rigid; 
    private GroundedComponent ground;

    private bool bAirComboFinish = false;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Sword;
    }

    protected override void Start()
    {
        base.Start();

        holsterTransform = rootObject.transform.FindChildByName(holsterName);
        Debug.Assert(holsterTransform != null, $"{rootObject.name} is error");

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(holsterTransform, false);
        rigid = rootObject.GetComponent<Rigidbody>();   
        ground = rootObject.GetComponent<GroundedComponent>();
        ground.OnCharacterGround += OnCharacterGround;
        //Debug.Assert(slashTransform != null);

        this.OnHitTarget += OnHitTarget_AirFinish;
    }

    //public override void PerpareWeapon()
    //{
    //    base.PerpareWeapon();

    //}

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(handTransform, false);
    }

    public override void Unequip()
    {
        base.Unequip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(holsterTransform, false);
    }

    public override void DoAction(int comboIndex, bool bNext = false)
    {
        if(isSubAction)
        {

            if (listener != null)
            {
                listener.m_ReactionSettings.m_SecondaryNoise = null;
            }

            comboIndex %= (doActionDatas.Length);
            bExist = bNext;
            if (isAnimating)
            {
                return;
            }

            index = comboIndex;
            isAnimating = true;

            animator.Play("Unarmed.Sword.Sub.Sword_Air_Combo"+ (index+1));
            return; 
        }

        base.DoAction(comboIndex, bNext);
    }

    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        if (isSubAction)
        {
            //TODO: 좀 복잡한데..
            rigid.drag = 0;
            if (rigid != null)
            {
                // 잠깐 공중일 경우 멈춤
                rigid.isKinematic = true;
            }
        }

    }

    public override void End_DoAction()
    {
        base.End_DoAction();

        if (isSubAction)
        {
            //TODO: 좀 복잡한데..
            if (rigid != null)
            {
                // 잠깐 공중일 경우 멈춤
                rigid.isKinematic = false;
                rigid.velocity = new Vector3(0, -1.5f, 0);
            }
        }
    }

    public override void DoSubAction()
    {
        base.DoSubAction();
        isSubAction = true;
        animator.SetBool("SubActionMode", true);
    }

    public override void Begin_SubAction()
    {
        base.Begin_SubAction();

        StartCoroutine(Play_QuickMoveToTarget());
    }

    IEnumerator Play_QuickMoveToTarget()
    {

        Collider[] colliders = Physics.OverlapSphere(this.transform.position,
            5, (1 << LayerMask.NameToLayer("Enemy")));

        GameObject candidate = null;
        float angle = -2.0f;
        foreach (Collider collider in colliders)
        {
            Vector3 direction = collider.transform.position - rootObject.transform.position;
            direction.Normalize();

            Vector3 forward = rootObject.transform.forward;
            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.5f || dot < angle)
                continue;

            angle = dot;
            candidate = collider.gameObject;
        }

        if (candidate == null)
        {
            End_SubAction();
            yield break;
        }

        float elapsedTime = 0.0f;
        const float durtaion = 0.25f;

        Vector3 startPosition = rootObject.transform.position;
        Vector3 targetPosition = candidate.transform.position;
        // 목표지점에서 어느 정도 떨어진 곳에 위치 
        Vector3 stopPosition = targetPosition + (startPosition - targetPosition).normalized * 1.5f;
        while (elapsedTime < durtaion)
        {
            Vector3 targetPos = Vector3.Lerp(startPosition, stopPosition,
                (elapsedTime / durtaion));
            rootObject.transform.position = targetPos;

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        yield return null;

        rootObject.transform.position = stopPosition;

        StartCoroutine(Play_ArroundBreak());
    }

    // 적 주변 타격 후 오르기 
    IEnumerator Play_ArroundBreak()
    {
        animator.Play("Unarmed.Sword.Sub.Sword_Upper");

        Collider[] colliders = Physics.OverlapSphere(this.transform.position,
            3, (1 << LayerMask.NameToLayer("Enemy")));

        foreach (Collider collider in colliders)
        {
            // 데미지 주기
            IDamagable damge = collider.GetComponent<IDamagable>();
            if (damge != null)
            {
                damge.OnDamage(rootObject, this, Vector3.zero, subActionDatas[0]);
            }
        }

        // 띄우기 
        {
          
            ground.OnAirial();

            float elapsedTime = 0.0f;
            float duration = 0.25f;
            float targetPosY = 5.0f;

            Vector3 startPos = rootObject.transform.position;
            Vector3 targetPos = rootObject.transform.position + Vector3.up * targetPosY; 
            while(elapsedTime < duration)
            {

                Vector3 resultPos = Vector3.Lerp(startPos, targetPos,
             (elapsedTime / duration));

                rootObject.transform.position = resultPos;

                elapsedTime += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }

            rootObject.transform.position = targetPos;
        }

        yield return null;
    }

    private void OnCharacterGround()
    {
        End_SubAction();
    }


    // 낙하 공격
    public void DoPlayDownFall()
    {
        bAirComboFinish = true;
        StartCoroutine(Play_DownFall(rigid));
    }

    private void OnHitTarget_AirFinish(GameObject target)
    {
        if (target == null)
            return;

        if (bAirComboFinish == false)
            return;

        bAirComboFinish = false; 

        if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            //rb.velocity = new Vector3(0, -10, 0);
            StartCoroutine(Play_DownFall(rb));
        }
    }

    private IEnumerator Play_DownFall(Rigidbody rigid)
    {
        rigid.isKinematic = false;
        float elapsedTime = 0.0f;
        float duration = 0.25f;
        float targetPosY = 3f;

        while (ground.IsGround == false)
        {
            rigid.velocity = new Vector3(0, -20, 0);
            yield return new WaitForFixedUpdate();
        }

        yield return null;
        
        //rigid.AddForce(Vector3.down * 100, ForceMode.Impulse);
    }


}
