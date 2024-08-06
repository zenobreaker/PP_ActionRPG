using System.Collections;
using UnityEngine;

public class Fist : Melee
{
    private enum PartType
    {
        LeftHand, RightHand, LeftFoot, RightFoot, Max,
    };


    protected bool bSkillAction = false;
  
    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Fist;
    }


    private void Test()
    {
        //Transform tf = childObjects[i].transform;
        //// tf.DetachChildren();
        ////tf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        //Transform bodyPartTF = rootObject.transform.FindChildByName(transformNames[i]);
        //Debug.Assert(bodyPartTF != null, "BodypartTF For Fireball Not Found");

        //tf.SetParent(bodyPartTF, false);
    }

    protected override void Awake()
    {
        base.Awake();
        
        for(int i = 0; i < (int)PartType.Max; i++)
        {
            Transform t = colliders[i].transform;
            
            //t.DetachChildren();
            //t.position = Vector3.zero;
            //t.rotation = Quaternion.identity;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            Fist_Trigger trigger = t.GetComponent<Fist_Trigger>();
            trigger.OnTrigger += OnTriggerEnter;

            string partName = ((PartType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            t.SetParent(parent, false);
        }
    }

    //public override void DoAction(int comboIndex)
    //{
    //    base.DoAction(comboIndex);
    //    Debug.Log("자식에서 팅길께?");
    //    Debug.Log($"Current Combo: {comboIndex}");
    //    animator.Play($"Fist_Combo{comboIndex}");
    //}

    public override void Begin_Collision(AnimationEvent e)
    {
        //base.Begin_Collision(e);

        // 자기 공격만 활성화 부모는 콜 안함
        string estring = e.stringParameter;
        if (estring != "")
        {
            string[] strings = estring.Split(',');
            foreach (string s in strings)
            {
                if (int.TryParse(s, out int result))
                    colliders[result].enabled = true;
            }
            return; 
        }

        colliders[e.intParameter].enabled = true;

    }

    protected override void SetParticleObject(int index)
    {
        base.SetParticleObject(index);
        //if (particleTransforms == null)
        //    return;

        //if (particlePrefabs == null)
        //    return;

        //if (particlePrefabs.Length == 0 ||
        //    (index >= 0 && index <= particlePrefabs.Length - 1))
        //    return;

    }

    private GameObject target = null; 
    public override void End_Collision()
    {
        target = null;

        target = ChaseAirboneEnemy();
        //2. 높이 뜬 적이 있다면 적에게 바로 이동 
        StartCoroutine(QuickApproachToTarget(target));

        //TODO: 임시로 여기에 둠 
        bSkillAction = false; 
        base.End_Collision();
    }

    #region Skill_Action

    public void Fist_AirCombo()
    {
        // 1. 하이킥
        bSkillAction = true;
        animator.SetInteger(SkillNumberHash, 1);
        animator.SetTrigger(SkillActionHash);
        useSkillID = 4;
        index = useSkillID;
       
        //3. 공격 입력 받음 

        //4. 입력 받는 것에 따라 추가 공격 3연타 
    }

    private GameObject ChaseAirboneEnemy()
    {
        if (hittedList.Count == 0)
            return null;

        float minDistance = float.MaxValue;
        GameObject target = null;
        foreach (GameObject obj in hittedList)
        {
            float distance = obj.transform.position.y - transform.position.y;
            if(distance <= 1.0f)
                continue;

            if (distance < minDistance)
            {
                minDistance = distance;
                target = obj;
            }
        }
                
        return target; 
    }


    private IEnumerator QuickApproachToTarget(GameObject target)
    {
        if (target == null && bSkillAction == false)
            yield break;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (target.transform.position - transform.position).normalized;
        float dist = Vector3.Distance(startPosition, targetPosition);

        float xzOffset = dist /*- distanceToStop*/;
        Vector3 stopPosition = startPosition +
            new Vector3(direction.x * xzOffset, direction.y * dist, direction.z * xzOffset);

        float elapsedTime = 0.0f;
        const float snapDuration = 0.1f;
        while (elapsedTime < snapDuration)
        {
            //transform.position = Vector3.Slerp(startPosition, stopPosition, elapsedTime / snapDuration);
            Vector3 targetPos = Vector3.Slerp(startPosition, stopPosition, elapsedTime / snapDuration);
            Vector3 moveDirection = targetPos - transform.position;

            transform.position = moveDirection;
            //Debug.Log($" final {stopPosition}");
            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();

        }
        transform.position = stopPosition;
    }


    public override void Begin_SkillAction()
    {
        base.Begin_SkillAction();

        Fist_AirCombo();



        CanMove();
    }

    public override void End_SkillAciton()
    {
        base.End_SkillAciton();
    }
    #endregion


}
