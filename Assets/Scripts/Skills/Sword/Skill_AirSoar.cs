using System;
using System.Collections;
using UnityEngine;

public class Skill_AirSoar : Skill_Trigger_Melee
{
    /// <summary>
    /// �� ��ų�� ��� �� ���� �� ���� ������ ������ ��� ���� ���߿��� �����Ѵ�.
    /// </summary>


    public override void ExecuteSkill()
    {
        base.ExecuteSkill();

        StartCoroutine(Play_QuickMoveToTarget());
    }

    // 1. �� ���� �� ��󿡰� ������ �̵�
    IEnumerator Play_QuickMoveToTarget()
    {

        Collider[] colliders = Physics.OverlapSphere(this.transform.position, 
            skillData.skillRange, (1<<LayerMask.NameToLayer("Enemy")));

        GameObject candidate = null;
        float angle = float.MaxValue;
        foreach(Collider collider in colliders)
        {
            Vector3 direction = collider.transform.position - rootObject.transform.position;
            direction.Normalize();

            Vector3 forward = rootObject.transform.forward;
            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.5 || dot < angle)
                continue;

            angle = dot;
            candidate = collider.gameObject;
        }

        if (candidate == null)
            yield break;

        float elapsedTime = 0.0f;
        const float durtaion = 0.25f;

        Vector3 startPosition = rootObject.transform.position;
        Vector3 targetPosition = candidate.transform.position;
       // ��ǥ�������� ��� ���� ������ ���� ��ġ 
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
    }


    // 2. ���� ����Ʈ �� ���߿� �ڽ��� ���� �̵� ��Ŵ
    IEnumerator Play_SoarAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position,
           3, (1 << LayerMask.NameToLayer("Enemy")));

        //������ �ֱ�
        ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[0]);

        // ���� 
        {
            // ���� �� ĳ������ �׶��� ������Ʈ���� �� �ö����ϱ� �� �����޶�� ��ȣ������
            GroundedComponent ground = rootObject.GetComponent<GroundedComponent>();

            float elapsedTime = 0.0f;
            float duration = 0.25f;
            float targetPosY = 3f;

            Vector3 startPos = rootObject.transform.position;
            Vector3 targetPos = rootObject.transform.position + Vector3.up * targetPosY;
            while (elapsedTime < duration)
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

    // 3. ���߿��� �߰����� ����Ű�� ������ ���� ���� ���� 

    // 4. ���ݸ���� �����ϰų� �ٽ� ��ųŰ�� ������ ������ ����
}
