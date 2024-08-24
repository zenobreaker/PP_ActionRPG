using System;
using System.Collections;
using UnityEngine;

public class Skill_AirSoar : Skill_Trigger_Melee
{
    /// <summary>
    /// 이 스킬은 사용 시 감지 후 감지 대상들을 일제히 띄워 같이 공중에서 공격한다.
    /// </summary>


    public override void ExecuteSkill()
    {
        base.ExecuteSkill();

        StartCoroutine(Play_QuickMoveToTarget());
    }

    // 1. 적 감지 후 대상에게 빠르게 이동
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
    }


    // 2. 띄우는 이펙트 후 공중에 자신을 포함 이동 시킴
    IEnumerator Play_SoarAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position,
           3, (1 << LayerMask.NameToLayer("Enemy")));

        //데미지 주기
        ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[0]);

        // 띄우기 
        {
            // 오를 때 캐릭터의 그라운드 컴포넌트한테 나 올랐으니까 곧 내려달라고 신호보내기
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

    // 3. 공중에서 추가적인 공격키를 누르면 공중 공격 개시 

    // 4. 공격모션을 다취하거나 다시 스킬키를 누르면 마무리 공격
}
