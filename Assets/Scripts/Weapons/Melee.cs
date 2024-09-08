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

    // ��ƼŬ ���� 
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
        // ȸ�� �ӵ�
        float rotationSpeed = 5.0f;
        float elapasedTime = 0.0f;
        float duration = 0.1f;
        // ��ǥ ������ ������ ������ ������ ����
        while (elapasedTime < duration)
        {
            elapasedTime += Time.deltaTime;
            // ���� ȸ���� ��ǥ ȸ���� ���� �����մϴ�.
            rootObject.transform.rotation = Quaternion.Slerp(rootObject.transform.rotation, rotation, (elapasedTime * rotationSpeed)/duration);
            float differ = Quaternion.Angle(rootObject.transform.rotation, rotation);
            if(differ < 2.0f )
            {
                rootObject.transform.rotation = rotation;
                yield break; 
            }

            // ���� �������� ��ٸ��ϴ�.
            yield return null;
        }
    }

    // ��ƼŬ ���� 
    protected void DeleteParticle()
    {
        if (trailParticles == null)
            return;

        for (int i = 0; i < trailParticles.Length; i++)
        {
            Destroy(trailParticles[i]);
        }
    }

    // �޺� �� ���� �Է� Ȯ�� 
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

        // 콤보 가능한 상태인지 
        if (bEnable)
        {
            bEnable = false;
            bExist = true;

            return;
        }

        if (state.IdleMode == false)
            return;

        base.DoAction();

    }

    public override void DoAction(bool bNext)
    {

        if (listener != null)
        {
            listener.m_ReactionSettings.m_SecondaryNoise = null;
        }

        bExist = bNext;
        if (isAnimating)
        {
            return;
        }

        isAnimating = true;
        Debug.Log($"{this.rootObject.name} Combo : " + index);
        animator.Play(comboObjData.comboDatas[index].ComboName);
        

        if (state.IdleMode == false)
            return;
        
        base.DoAction(bNext);
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
        index++;
        index %= (doActionDatas.Length);
      
        if (bExist == false)
            return;

        bExist = false;
        animator.SetTrigger("NextCombo");

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



    protected virtual void OnTriggerEnter(Collider other)
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


        // ���� �� ��ǥ 
        hitPoint = enabledCollider.ClosestPoint(other.transform.position);
        // ���࿭ ���ؼ� ���� �� ��ǥ�� �Ұ��ؼ� ������ǥ�� ��ȯ
        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        //var text = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //GameObject temp = Instantiate<GameObject>(text,
        //    other.transform, false);
        //temp.transform.localPosition = hitPoint;
        //temp.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        //Debug.Log($"{rootObject.name} : current data {doActionDatas[index].dataID}");


        //if (moving != null)
        //{
        //    // �и�Ÿ���� ���� �۾� ���濡 �¾Ѱ� ���� �����𿡰� ī�޶� ������ 
        //    Vector3 direction = other.gameObject.transform.position - rootObject.transform.position;
        //    Quaternion q = Quaternion.FromToRotation(rootObject.transform.forward, direction.normalized);

        //    // ��ġ�� 0���̶� ȸ���Ϸ��� �����̴� ���߿� �ȷο�Ÿ���� �����ͼ� ������ �ϰ� �����ؾ��� �� 
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

        // ��: ���� ��ǥ���� z��� �� ���� ���� ������ ���� ���
        Vector3 referenceDirection = Vector3.forward;
        float angle = Vector3.SignedAngle(referenceDirection, direction.normalized, Vector3.up);

        Debug.Log($"�� ���� ����: {direction}");
        Debug.Log($"z��� �� ���� ���� ������ ����: {angle}��");

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

        // RotateOverLifetime ����� ��������
        var rotateOverLifetime = particleSystem.rotationOverLifetime;
        rotateOverLifetime.enabled = true;
        rotateOverLifetime.separateAxes = true;

        // ���� Ŀ�긦 �����ϰ� ���� ������Ű��
        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 15),
            new Keyframe(1f, rotateOverLifetime.y.curve.keys[0].value)
        );

        // Ŀ���� ��� Ű������ ���� ������Ű��
        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe key = curve.keys[i];
            key.value = key.value;
            curve.MoveKey(i, key);
        }

        // Ŀ�긦 MinMaxCurve�� ��ȯ
        ParticleSystem.MinMaxCurve yCurve = new ParticleSystem.MinMaxCurve(1f, curve);
        Debug.Log($"{rotateOverLifetime.y.curve.keys[0].value}/ {rotateOverLifetime.y.curve.keys[1].value}");

        // Y�� ȸ�� ���� �ִϸ��̼� Ŀ��� ����
        rotateOverLifetime.y = yCurve;
    }


    protected virtual void CreateSlashParticle()
    {
        if (slashTransform == null)
            return;

        if (doActionDatas[index].Particle == null)
            return;

        Debug.Assert(doActionDatas[index].Particle != null);
        // ��ƼŬ ������Ʈ�� �ν��Ͻ�ȭ�ϰ� ȸ���� �����մϴ�.
        GameObject obj = Instantiate<GameObject>(doActionDatas[index].Particle, slashTransform, false);
        Debug.Assert(obj != null);
        if (obj == null)
            return;

        //CreateSlashParticle_Test();

        obj.transform.localRotation = Quaternion.identity;

        Vector3 initPos = this.transform.position.normalized;
        Vector3 slashPos = slashTransform.position.normalized;

        // ������ ������ �̿��Ͽ� ������ ���մϴ�.
        float dotProduct = Vector3.Dot(initPos, slashPos);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg; // ������ ������ ��ȯ�մϴ�.
        Vector3 referVec = slashTransform.forward;
        Vector3 dirVec = transform.position - slashTransform.transform.position;
        // ���� ������Ʈ�� ���� ���Ϳ� Ÿ�� ������Ʈ�� ���ϴ� ������ ũ�ν� ���δ�Ʈ ���
        Vector3 crossProdut = Vector3.Cross(referVec, dirVec);
        VFXController vfx = obj.GetComponent<VFXController>();
        // ũ�ν� ���δ�Ʈ�� y���� ������� ���������� ������ ����
        if (vfx != null)
            vfx.ControllParticleSystem(crossProdut.y > 0);

        var result = Extend_Vector3.GetAngle(slashTransform.position, this.transform.position);
        Vector3 localY = slashTransform.InverseTransformPoint(this.transform.position);

        float prevRotY = obj.transform.rotation.eulerAngles.y;
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.forward);
        // ���Ϸ� ��ȯ
        Vector3 originEulerAngles = new Vector3(rotation.eulerAngles.x, prevRotY, rotation.eulerAngles.z);


        //  �ٽ� ���ʹϾ� ��ȯ 
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
