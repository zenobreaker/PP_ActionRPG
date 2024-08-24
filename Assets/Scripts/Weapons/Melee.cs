using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class Melee : Weapon
{
    protected bool bEnable;
    protected bool bExist;


    [SerializeField] protected bool isAnimating = false;
    [SerializeField] protected SO_Combo comboObjData;
    public SO_Combo ComboObjData { get => comboObjData; }

  //  [SerializeField] private bool bDebugMode = false;

    [SerializeField] protected int index;

    protected int useSkillID = -1;

    protected Collider[] colliders;
    protected List<GameObject> hittedList;

    #region Cinenmachine
    protected CinemachineImpulseSource impulse;
    protected CinemachineImpulseListener listener;
    protected CinemachineBrain brain;
    protected CinemachineVirtualCamera virtualCamera;
    #endregion

    #region Particle Offset
    [SerializeField] protected string slashTransformName = "Slash_Transform";
    [SerializeField] GameObject startPosObj;
    [SerializeField] GameObject endPosObj;
    #endregion

    [Header("Particle")]
    [SerializeField] protected GameObject[] particlePrefabs;
    [SerializeField] protected string[] particleTransformNames = { "StartPos", };
    protected Transform[] particleTransforms;
    protected GameObject[] trailParticles;

    protected Transform slashTransform;
    protected AIController aiController;

    public event Action<GameObject> OnHitTarget;

    protected override void Awake()
    {
        base.Awake();

        hittedList = new List<GameObject>();
        colliders = GetComponentsInChildren<Collider>();
        Debug.Assert(colliders != null);
        impulse = GetComponent<CinemachineImpulseSource>();

        slashTransform = rootObject.transform.FindChildByName(slashTransformName);

        aiController = GetComponent<AIController>();

        particleTransforms = new Transform[particleTransformNames.Length];
        for (int i = 0; i < particleTransformNames.Length; i++)
        {
            particleTransforms[i] = transform.FindChildByName(particleTransformNames[i]);
        }

        trailParticles = new GameObject[particlePrefabs.Length];
    }

    protected override void Start()
    {
        base.Start();

        brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            virtualCamera = brain.ActiveVirtualCamera as CinemachineVirtualCamera;

            if (virtualCamera != null)
                listener = virtualCamera.GetComponent<CinemachineImpulseListener>();
        }

        //Debug.Log($"{rootObject.name} = {name}");
        End_Collision();
    }

    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true;


    }

    // 파티클 세팅 
    protected virtual void SetParticleObject(int index)
    {
        if (particleTransforms == null)
            return;

        if (particlePrefabs == null)
            return;

        if (particlePrefabs.Length == 0 ||  
            (index < 0 && index >= particlePrefabs.Length))
            return;
        
        trailParticles[index] = Instantiate<GameObject>(particlePrefabs[index], particleTransforms[index]);
        trailParticles[index].transform.localPosition = Vector3.zero;
        
    }

    private Coroutine rotateCoroutine;
    public virtual void End_Collision()
    {
        foreach (Collider collider in colliders)
            collider.enabled = false;

        float angle = -2.0f; 
        GameObject candidate = null;

        foreach (GameObject hit in hittedList)
        {
            Vector3 direction = hit.transform.position - rootObject.transform.position;
            direction.Normalize();

            Vector3 forward = rootObject.transform.forward;

            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.75f || dot < angle)
                continue;

            angle = dot; 
            candidate = hit;
        }

        if(candidate != null)
        {
            Vector3 direction = candidate.transform.position - rootObject.transform.position;
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation.x = rootObject.transform.rotation.x;
            targetRotation.z = rootObject.transform.rotation.z;
            if (rotateCoroutine != null)
                StopCoroutine(rotateCoroutine);
            rotateCoroutine = StartCoroutine(RotateToTarget(targetRotation));
        }


        hittedList.Clear();

        DeleteParticle();
    }

    IEnumerator RotateToTarget(Quaternion rotation)
    {
        // 회전 속도
        float rotationSpeed = 5.0f;
        float elapasedTime = 0.0f;
        float duration = 0.1f;
        // 목표 각도에 도달할 때까지 루프를 실행
        while (elapasedTime < duration)
        {
            elapasedTime += Time.deltaTime;
            // 현재 회전을 목표 회전을 향해 보간합니다.
            rootObject.transform.rotation = Quaternion.Slerp(rootObject.transform.rotation, rotation, (elapasedTime * rotationSpeed)/duration);
            float differ = Quaternion.Angle(rootObject.transform.rotation, rotation);
            if(differ < 2.0f )
            {
                rootObject.transform.rotation = rotation;
                yield break; 
            }

            // 다음 프레임을 기다립니다.
            yield return null;
        }
    }

    // 파티클 삭제 
    protected void DeleteParticle()
    {
        if (trailParticles == null)
            return;

        for (int i = 0; i < trailParticles.Length; i++)
        {
            Destroy(trailParticles[i]);
        }
    }

    // 콤보 중 공격 입력 확인 
    public void Begin_Combo()
    {
        bEnable = true;
    }

    public void End_Combo()
    {
        bEnable = false;
    }

    public override void DoAction()
    {
        if (listener != null)
        {
            listener.m_ReactionSettings.m_SecondaryNoise = null;
        }

        // 공격이 들어온 상황
        if (bEnable)
        {
            bEnable = false;
            bExist = true;

            return;
        }

        // 최초 공격
        if (state.IdleMode == false)
            return;

        // 부모에선 아이들모드가 안되므로 이 위치로
        base.DoAction();

    }

    public override void DoAction(int comboIndex , bool bNext = false)
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
        animator.Play(comboObjData.comboDatas[comboIndex].ComboName);

        // 최초 공격
        if (state.IdleMode == false)
            return;
        
        // 부모에선 아이들모드가 안되므로 이 위치로
        base.DoAction(comboIndex, bNext);
    }



    public override void DoSubAction()
    {
        if (state.IdleMode == false)
            return;

        base.DoSubAction();
    }

    protected void CanMove()
    {
        if (doActionDatas[index].bCanMove == true)
        {
            Move();
            return;
        }

        CheckStop(index);
    }

    public override void Begin_DoAction()
    {
        isAnimating = false;
      
        if (bExist == false)
            return;
        bExist = false;
        //index++;
        //animator.SetTrigger("NextCombo");

        CanMove();
        base.Begin_DoAction();
    }

    public override void End_DoAction()
    {
        base.End_DoAction();
        
        comboObjData?.OnChangeCombo(index);
        isAnimating = false; 
        index = 0;
        bEnable = false;

    }

    public override void Play_Impulse()
    {
        if (impulse == null)
            return;

        if (doActionDatas[index].impulseSettings == null)
            return;
        if (doActionDatas[index].impulseDirection.magnitude <= 0.0f)
            return;
        if (listener == null)
            return;

        base.Play_Impulse();

        listener.m_ReactionSettings.m_SecondaryNoise = doActionDatas[index].impulseSettings;

        impulse.GenerateImpulse(doActionDatas[index].impulseDirection);

    }

    public override void Play_Impulse(ActionData data)
    {
        if (impulse == null || data == null)
            return;

        if (data.impulseSettings == null)
            return;
        if (data.impulseDirection.magnitude <= 0.0f)
            return;
        if (listener == null)
            return;

        base.Play_Impulse();

        listener.m_ReactionSettings.m_SecondaryNoise = data.impulseSettings;

        impulse.GenerateImpulse(data.impulseDirection);

    }



    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        if (hittedList.Contains(other.gameObject) == true)
            return;

        if (other.CompareTag(this.rootObject.tag))
            return;

        hittedList.Add(other.gameObject);

        IDamagable damagable = other.GetComponent<IDamagable>();

        // hit Sound Play
        SoundManager.Instance.PlaySFX(doActionDatas[index].hitSoundName);

        if (damagable == null)
            return;


        Vector3 hitPoint = Vector3.zero;

        Collider enabledCollider = null;
        foreach (Collider collider in colliders)
        {
            if (collider.enabled)
            {
                enabledCollider = collider;
                break;
            }
        }


        // 월드 상 좌표 
        hitPoint = enabledCollider.ClosestPoint(other.transform.position);
        // 역행열 곱해서 월드 상 좌표를 소거해서 로컬좌표로 변환
        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        //var text = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //GameObject temp = Instantiate<GameObject>(text,
        //    other.transform, false);
        //temp.transform.localPosition = hitPoint;
        //temp.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        //Debug.Log($"{rootObject.name} : current data {doActionDatas[index].dataID}");


        //if (moving != null)
        //{
        //    // 밀리타격일 때만 작업 전방에 맞앗고 가장 가까운놈에게 카메라 돌리기 
        //    Vector3 direction = other.gameObject.transform.position - rootObject.transform.position;
        //    Quaternion q = Quaternion.FromToRotation(rootObject.transform.forward, direction.normalized);

        //    // 피치가 0값이라 회전완료후 정면이다 나중에 팔로우타겟을 가져와서 세팅을 하고 전달해야할 것 
        //    rootObject.transform.rotation *= Quaternion.Euler(0, q.eulerAngles.y, 0);
        //    moving.Rotation = rootObject.transform.rotation;
        //}

        damagable.OnDamage(rootObject, this, hitPoint, doActionDatas[index]);
        OnHitTarget?.Invoke(other.gameObject);
        if (aiController == null)
            Play_Impulse();
    }


    protected bool CheckPlayerToForward()
    {
        Vector3 forward = rootObject.transform.forward;
        Vector3 toRefer = (forward - rootObject.transform.up).normalized;

        float dot = Vector3.Dot(toRefer, forward);

        if (dot < 0)
            return false;
        return true;
    }

    protected virtual void CreateSlashParticle_Test()
    {
        if (slashTransform == null)
            return;

        if (doActionDatas[index].Particle == null)
            return;


        Vector3 direction = endPosObj.transform.position - startPosObj.transform.position;

        // 예: 월드 좌표계의 z축과 검 방향 벡터 사이의 각도 계산
        Vector3 referenceDirection = Vector3.forward;
        float angle = Vector3.SignedAngle(referenceDirection, direction.normalized, Vector3.up);

        Debug.Log($"검 방향 벡터: {direction}");
        Debug.Log($"z축과 검 방향 벡터 사이의 각도: {angle}도");

        //float dot = Vector3.Dot(rootObject.transform.position.normalized, direction.normalized);
        //float theta = Mathf.Acos(dot) * Mathf.Rad2Deg;
        //float angleRadian = Mathf.Atan2(direction.z, direction.x);
        //float angleDegree = angleRadian * Mathf.Rad2Deg;

        //Debug.DrawLine(startPosObj.transform.position, endPosObj.transform.position, Color.green, 4.0f);
        //Debug.Log($"dot : {theta} / angle : {angleRadian} / {angleDegree}");

        //Vector3 cross1 = Vector3.Cross(rootObject.transform.position, direction.normalized);
        //Vector3 cross2 = Vector3.Cross(direction.normalized, rootObject.transform.position);
        //Debug.Log($"cross : {cross1} / {cross2}");



        //GameObject obj = Instantiate<GameObject>(doActionDatas[index].Particle, slashTransform);
        //Debug.Assert(obj != null);
        //if (obj == null)
        //    return;
        //obj.transform.rotation = Quaternion.Euler(0, rootObject.transform.eulerAngles.y, 0);
        //if (cross1.y < 0)
        //    theta *= -1.0f;
        //obj.transform.rotation = Quaternion.Euler(0, 0, theta);



        //VFXController vfx = obj.GetComponent<VFXController>();
        //if (vfx != null)
        //    vfx.ControllParticleSystem(cross.y < 0);
        //RotateVFS(obj, cross.y < 0);
    }


    private void RotateVFS(GameObject obj, bool right)
    {
        if (obj == null)
            return;

        ParticleSystem particleSystem = obj.GetComponent<ParticleSystem>();
        if (particleSystem == null) return;

        // RotateOverLifetime 모듈을 가져오기
        var rotateOverLifetime = particleSystem.rotationOverLifetime;
        rotateOverLifetime.enabled = true;
        rotateOverLifetime.separateAxes = true;

        // 기존 커브를 복사하고 값을 반전시키기
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 15),
            new Keyframe(1f, rotateOverLifetime.y.curve.keys[0].value)
        );

        // 커브의 모든 키프레임 값을 반전시키기
        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe key = curve.keys[i];
            key.value = key.value;
            curve.MoveKey(i, key);
        }

        // 커브를 MinMaxCurve로 변환
        ParticleSystem.MinMaxCurve yCurve = new ParticleSystem.MinMaxCurve(1f, curve);
        Debug.Log($"{rotateOverLifetime.y.curve.keys[0].value}/ {rotateOverLifetime.y.curve.keys[1].value}");

        // Y축 회전 값을 애니메이션 커브로 설정
        rotateOverLifetime.y = yCurve;
    }


    protected virtual void CreateSlashParticle()
    {
        if (slashTransform == null)
            return;

        if (doActionDatas[index].Particle == null)
            return;

        Debug.Assert(doActionDatas[index].Particle != null);
        // 파티클 오브젝트를 인스턴스화하고 회전을 설정합니다.
        GameObject obj = Instantiate<GameObject>(doActionDatas[index].Particle, slashTransform, false);
        Debug.Assert(obj != null);
        if (obj == null)
            return;

        //CreateSlashParticle_Test();

        obj.transform.localRotation = Quaternion.identity;

        Vector3 initPos = this.transform.position.normalized;
        Vector3 slashPos = slashTransform.position.normalized;

        // 벡터의 내적을 이용하여 각도를 구합니다.
        float dotProduct = Vector3.Dot(initPos, slashPos);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg; // 라디안을 각도로 변환합니다.
        Vector3 referVec = slashTransform.forward;
        Vector3 dirVec = transform.position - slashTransform.transform.position;
        // 기준 오브젝트의 전방 벡터와 타겟 오브젝트로 향하는 벡터의 크로스 프로덕트 계산
        Vector3 crossProdut = Vector3.Cross(referVec, dirVec);
        VFXController vfx = obj.GetComponent<VFXController>();
        // 크로스 프로덕트의 y값이 양수인지 음수인지로 방향을 결정
        if (vfx != null)
            vfx.ControllParticleSystem(crossProdut.y > 0);

        var result = Extend_Vector3.GetAngle(slashTransform.position, this.transform.position);
        Vector3 localY = slashTransform.InverseTransformPoint(this.transform.position);

        float prevRotY = obj.transform.rotation.eulerAngles.y;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);
        // 오일러 변환
        Vector3 originEulerAngles = new Vector3(rotation.eulerAngles.x, prevRotY, rotation.eulerAngles.z);


        //  다시 쿼터니언 변환 
        Quaternion flippQuaternion = Quaternion.Euler(originEulerAngles);

        obj.transform.rotation = flippQuaternion;
    }

    public override void Play_Particle(AnimationEvent e)
    {
        base.Play_Particle(e);

        SetParticleObject(e.intParameter);
        //CreateSlashParticle();
        //CreateSlashParticle_Test();
    }

    public override void Play_Sound()
    {
        base.Play_Sound();
        // Sound Play
        SoundManager.Instance.PlaySFX(doActionDatas[index].effectSoundName);

    }

    public override void Begin_EnemyAttack(AnimationEvent e)
    {
        base.Begin_EnemyAttack(e);
        index = e.intParameter;

    }
}
