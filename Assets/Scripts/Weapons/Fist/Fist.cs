using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class Fist : Melee
{
    #region SubAction
    [SerializeField] private Material subActionTrailMaterial;
    [SerializeField] private float subActionMeshRate = 0.25f;
    private GameObject subObject;
    #endregion

    private PlayableDirector subActionDirector;
    [SerializeField] private PlayableAsset subActionPlayableAsset;



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

        for (int i = 0; i < (int)PartType.Max; i++)
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

        subActionDirector = rootObject.GetComponent<PlayableDirector>();
        Debug.Assert(subActionDirector != null);
        subActionDirector.stopped += OnSubActionPlay;
    }


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

    public override void DoSubAction()
    {
        if (isSubAction)
            return; 

        base.DoSubAction();
        if (subActionDirector == null)
            return;

        isSubAction = true;
        subActionDirector.playableAsset = subActionPlayableAsset;
        subActionDirector.Play();
    }

    public override void Begin_SubAction()
    {
        base.Begin_SubAction();
       
        if(isSubAction)
        {
            Vector3 position = rootObject.transform.position + Vector3.up;
            subObject = Instantiate<GameObject>(subActionDatas[0].Particle, position,
                rootObject.transform.localRotation);
            

            return; 
        }
    }

    public override void End_SubAction()
    {
        Destroy(subObject, 0.5f);
        base.End_SubAction();
    }


    public override void Play_Particle(AnimationEvent e)
    {
        if (isSubAction)
        {
            if (subObject != null)
            {
                if (subObject.TryGetComponent<Fist_SubAction_Strike>(out Fist_SubAction_Strike result))
                {
                    result.OnSubActionHit += OnSubActionHit;
                    result.Apply_Effect(rootObject);
                }
            }

            return;
        }

        base.Play_Particle(e);
    }


    private void OnSubActionPlay(PlayableDirector playableDirector)
    {
        animator.Play("Sub_Fist_Combo");
        if (playableDirector == subActionDirector)
        {
            //TODO: 아.. 좀 복잡하긴 한데;
            if (rootObject.TryGetComponent<MeshTrail>(out var meshTrail))
            {
                AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
                float length = clipInfos[0].clip.length;
                meshTrail.StartActiveTrail(length, subActionMeshRate, subActionTrailMaterial, true);
            }
        }
    }

    private void OnSubActionHit(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (other.CompareTag(this.rootObject.tag))
            return;

        IDamagable damagable = other.GetComponent<IDamagable>();

        // hit Sound Play
        SoundManager.Instance.PlaySFX(subActionDatas[0].hitSoundName);

        if (damagable == null)
            return;

        Vector3 hitPoint = Vector3.zero;

        // 월드 상 좌표 
        hitPoint = other.transform.position;
        // 역행열 곱해서 월드 상 좌표를 소거해서 로컬좌표로 변환
        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        damagable.OnDamage(rootObject, this, hitPoint, subActionDatas[0]);
        if (aiController == null)
            Play_Impulse(subActionDatas[0]);

    }



    #region Skill_Action

    public void Start_ApproachToTarget(GameObject target)
    {
        StartCoroutine(ApproachToTarget(target));
    }

    Vector3 debugPos = Vector3.zero;
    private IEnumerator ApproachToTarget(GameObject target)
    {
        if (target == null && bSkillAction == false)
            yield break;

        Vector3 startPosition = rootObject.transform.position;
        Vector3 targetPosition = target.transform.position;

        Vector3 stopPosition = targetPosition + (startPosition - targetPosition).normalized * 1.5f;
        debugPos = stopPosition;

        float elapsedTime = 0.0f;
        const float snapDuration = 0.25f;
        while (elapsedTime < snapDuration)
        {
            Vector3 targetPos = Vector3.Lerp(startPosition, stopPosition, (elapsedTime / snapDuration));

            rootObject.transform.position = targetPos;
            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();

        }
        rootObject.transform.position = stopPosition;
    }

    public GameObject GetperceptFrontViewNearEnemy()
    {
        Vector3 position = rootObject.transform.position;

        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        Collider[] colliders = Physics.OverlapSphere(position, 20.0f, layerMask);

        GameObject candidate = null;
        float angle = -2.0f;
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == this.rootObject)
                continue;

            Vector3 forward = rootObject.transform.forward;
            Vector3 position2 = collider.gameObject.transform.position;

            Vector3 direction = position2 - position;
            direction.Normalize();

            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.5f || dot < angle)
                continue;

            angle = dot;
            candidate = collider.gameObject;
            Debug.Log("적 감지  " + candidate.name);
        }

        return candidate;
        //if (candidate != null)
        //{
        //    if (Physics.Linecast(position, candidate.transform.position, out RaycastHit hit))
        //    {
        //        if (hit.transform.gameObject == candidate)
        //        {
        //            Debug.Log("대상 간에 장애물 없음 날아감!");
        //            StartCoroutine(QuickApproachToTarget(hit.transform.gameObject));
        //        }
        //    }
        //}
    }

    public override void Begin_SkillAction()
    {
        base.Begin_SkillAction();

        if (currSkillData == null)
            return;

        switch (currSkillData.skillName)
        {
            case "PowerSpike":
            Debug.Log("스킬 씀?");
            // PerceptFrontViewNearEnemy();
            break;
        }

    }



    public override void End_SkillAciton()
    {
        base.End_SkillAciton();
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(debugPos, 0.5f);
    }

#endif
}
