using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class Melee : Weapon
{
    protected bool bEnable;
    protected bool bExist;


    [SerializeField] protected bool isAnimating = false;
    [SerializeField] protected SO_Combo comboObjData;
    public SO_Combo ComboObjData { get => comboObjData; }


    [SerializeField] protected Weapon_Trail_Collision[] trail_Collisions;

    protected int index;

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
    #endregion

    [Header("Particle")]
    [SerializeField] protected GameObject[] particlePrefabs;
    [SerializeField] protected string[] particleTransformNames = { "StartPos", };
    protected Transform[] particleTransforms;
    protected GameObject[] trailParticles;
    protected Transform slashTransform;
    protected DoActionData currentActionData;

    protected BTAIController controller;

    public event Action<GameObject> OnHitTarget;

    protected override void Awake()
    {
        base.Awake();

        hittedList = new List<GameObject>();
        colliders = GetComponentsInChildren<Collider>();
        Debug.Assert(colliders != null);
        trail_Collisions = GetComponentsInChildren<Weapon_Trail_Collision>();
        if (trail_Collisions != null)
        {
            foreach (var collision in trail_Collisions)
            {
                collision.SetRootObject(rootObject);
                collision.OnDamage += OnDamage;
            }
        }

        impulse = GetComponent<CinemachineImpulseSource>();

        slashTransform = rootObject.transform.FindChildByName(slashTransformName);

        controller = rootObject.GetComponent<BTAIController>();


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
        foreach (Weapon_Trail_Collision collision in trail_Collisions)
            collision.OnInactivate();

        End_Collision();
    }
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

    public virtual void Begin_Collision(AnimationEvent e)
    {
        foreach (Collider collider in colliders)
            collider.enabled = true;
        
        hittedList.Clear();

        foreach (Weapon_Trail_Collision collision in trail_Collisions)
            collision.OnActivate();

    }



    private Coroutine rotateCoroutine;
    public virtual void End_Collision()
    {
        foreach (Collider collider in colliders)
            collider.enabled = false;

        foreach (Weapon_Trail_Collision collision in trail_Collisions)
            collision.OnInactivate();

        float angle = -2.0f;
        GameObject candidate = null;

        foreach (GameObject hit in hittedList)
        {
            if (hit == null)
                continue;

            Vector3 direction = hit.transform.position - rootObject.transform.position;
            direction.Normalize();

            Vector3 forward = rootObject.transform.forward;

            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.75f || dot < angle)
                continue;

            angle = dot;
            candidate = hit;
        }

        if (candidate != null)
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
            rootObject.transform.rotation = Quaternion.Slerp(rootObject.transform.rotation, rotation, (elapasedTime * rotationSpeed) / duration);
            float differ = Quaternion.Angle(rootObject.transform.rotation, rotation);
            if (differ < 2.0f)
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

        for (int i = 0; i < particleTransforms.Length; i++)
        {
            if (particleTransforms[i] != null)
            {
                for (int j = 0; j < particleTransforms[i].childCount; j++)
                {
                    Destroy(particleTransforms[i].GetChild(j).gameObject);
                }
            }
        }

        //for (int i = 0; i < trailParticles.Length; i++)
        //{
        //    Destroy(trailParticles[i]);
        //}
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
        currentActionData = doActionDatas[index];
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
        //Debug.Log($"{this.rootObject.name} Combo : " + index);
        animator.Play(comboObjData.comboDatas[index].ComboName);
        currentActionData = comboObjData.comboDatas[index].doActionData;

        if (state.IdleMode == false)
            return;

        base.DoAction(bNext);
    }


    public override void DoAction(int index = 0)
    {
        if (listener != null)
        {
            listener.m_ReactionSettings.m_SecondaryNoise = null;
        }

        ComboData comboData = comboObjData.GetComboDataByRewind(index);
        if (comboData == null)
            return;

        //if (isAnimating)
        //    return;

        //isAnimating = true;

        this.index = index %= (doActionDatas.Length);
        Debug.Log($"current index {this.index}");
        if (trail_Collisions != null)
        {
            foreach (var collision in trail_Collisions)
            {
                collision.bDebug = (this.index >= 3);
                //collision.bDebug = true;
            }
        }

        animator.Play(comboData.GetComboName);
        currentActionData = comboData.doActionData;

        if (state.IdleMode == false)
            return;

        base.DoAction(index);
    }


    public override void DoSubAction()
    {
        if (state.IdleMode == false)
            return;

        base.DoSubAction();
    }

    protected void CanMove()
    {
        if (currentActionData.bCanMove == true)
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

        if (currentActionData.impulseSettings == null)
            return;
        if (currentActionData.impulseDirection.magnitude <= 0.0f)
            return;
        if (listener == null)
            return;

        base.Play_Impulse();

        listener.m_ReactionSettings.m_SecondaryNoise = currentActionData.impulseSettings;

        impulse.GenerateImpulse(currentActionData.impulseDirection);

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

        if (currentActionData == null)
            return;

        if (trail_Collisions != null)
            return;

        hittedList.Add(other.gameObject);

        OnDamage(other);
    }


    protected void OnDamage(Collider other)
    {
        IDamagable damagable = other.GetComponent<IDamagable>();

        // hit Sound Play
        SoundManager.Instance.PlaySFX(currentActionData.hitSoundName);

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



        hitPoint = enabledCollider.ClosestPoint(other.transform.position);

        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        damagable.OnDamage(rootObject, this, hitPoint, currentActionData);
        OnHitTarget?.Invoke(other.gameObject);
        if (controller == null)
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


    protected virtual void CreateSlashParticle()
    {
        if (slashTransform == null)
            return;

        if (currentActionData.Particle == null)
            return;

        Debug.Assert(currentActionData.Particle != null);
        // ��ƼŬ ������Ʈ�� �ν��Ͻ�ȭ�ϰ� ȸ���� �����մϴ�.
        GameObject obj = Instantiate<GameObject>(currentActionData.Particle, slashTransform, false);
        Debug.Assert(obj != null);
        if (obj == null)
            return;

        //CreateSlashParticle_Test();

        obj.transform.localRotation = Quaternion.identity;

        Vector3 initPos = this.transform.position.normalized;
        Vector3 slashPos = slashTransform.position.normalized;

        float dotProduct = Vector3.Dot(initPos, slashPos);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg; // ������ ������ ��ȯ�մϴ�.
        Vector3 referVec = slashTransform.forward;
        Vector3 dirVec = transform.position - slashTransform.transform.position;
        Vector3 crossProdut = Vector3.Cross(referVec, dirVec);
        VFXController vfx = obj.GetComponent<VFXController>();
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
        SoundManager.Instance.PlaySFX(currentActionData.effectSoundName);

    }

    public override void Begin_EnemyAttack(AnimationEvent e)
    {
        base.Begin_EnemyAttack(e);
        index = e.intParameter;

    }
}
