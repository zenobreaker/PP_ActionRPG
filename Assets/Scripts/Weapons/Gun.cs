using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Melee
{
    private enum PartType
    {
        Hand_Gun_Left, Hand_Gun_Right, Hand_Gun_Rifle, TwoGuns, Max,
    }

    [SerializeField] private string leftMuzzleTransformName = "Hand_Gun_Left_Muzzle";
    [SerializeField] private string rightMuzzleTransformName = "Hand_Gun_Right_Muzzle";
    [SerializeField] private string rifleHolsterTransformName = "Holster_Rifle";
    [SerializeField] private string rifleHandTransformName = "Hand_Gun_Rifle";
    [SerializeField] private string rifleMuzzleTransformName = "Rifle_Muzzle";
    [SerializeField] private string gunShootSound = "Gun_Fire";
    [SerializeField] private float perceptionRange = 5.0f;
    // 총기 오브젝트
    [SerializeField] private GameObject[] gunObjects;

    private Transform leftMuzleTrasnform;
    private Transform rightMuzleTrasnform;
    private Transform rifleHolsterTransform;
    private Transform rifleHandTransform;

    private Coroutine rotateCoroutine;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Gun;
    }


    protected override void Awake()
    {
        base.Awake();

        leftMuzleTrasnform = rootObject.transform.FindChildByName(leftMuzzleTransformName);
        rightMuzleTrasnform = rootObject.transform.FindChildByName(rightMuzzleTransformName);

        rifleHandTransform = rootObject.transform.FindChildByName(rifleHandTransformName);

        for (int i = 0; i < 2; i++)
        {
            Transform gun = gunObjects[i].transform;
            gun.localPosition = Vector3.zero;
            gun.localRotation = Quaternion.identity;

            // 이벤트 
            if (gun.TryGetComponent<Gun_Trigger>(out var trigger))
            {
                trigger.RootObject = rootObject;
            }
            // 플레이어에 붙이기 
            string partName = ((PartType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            gun.SetParent(parent, false);
            gun.gameObject.SetActive(false);
        }

        // 라이플 장착 
        if (gunObjects.Length == 3)
        {

            Transform rifle = gunObjects[2].transform;
            rifle.localPosition = Vector3.zero;
            rifle.localRotation = Quaternion.identity;

            // 이벤트 
            if (rifle.TryGetComponent<Gun_Trigger>(out var trigger))
            {
                trigger.RootObject = rootObject;
            }

            rifleHolsterTransform = rootObject.transform.FindChildByName(rifleHolsterTransformName);
            Debug.Assert(rifleHolsterTransform != null);

            rifle.SetParent(rifleHolsterTransform, false);
            rifle.gameObject.SetActive(true);
        }
    }

    public override void DoAction(int comboIndex = 0, bool bNext = false)
    {
        if(isSubAction)
        {
            // 라이플 사격 
            Shoot_Rifle();
            
            return; 
        }

        base.DoAction(comboIndex, bNext);

        if (rotateCoroutine != null)
            StopCoroutine(rotateCoroutine);
        GameObject target = CheckTarget();
        rotateCoroutine = StartCoroutine(RotateToTarget(target));
    }

    public override void DoSubAction()
    {
        base.DoSubAction();
        isSubAction = !isSubAction;

        animator.SetBool("SubActionMode", isSubAction);
        if (isSubAction == false)
        {
            Equip_HandGun();
            Equip_Rifle();
        }
    }

    public override void Begin_SubAction()
    {
        base.Begin_SubAction();

    }


    public override void Begin_Equip()
    {
        base.Begin_Equip();

        Equip_HandGun();
        Equip_Rifle();
    }

    public override void Unequip()
    {
        base.Unequip();

        Debug.Assert(gunObjects != null);
        foreach (GameObject go in gunObjects)
            go.SetActive(false);

        if (gunObjects.Length == 3)
        {
            rifleHolsterTransform.DetachChildren();
            gunObjects[2].transform.localPosition = Vector3.zero;
            gunObjects[2].transform.localRotation = Quaternion.identity;

            gunObjects[2].transform.SetParent(rifleHolsterTransform, false);
        }
    }

    public override void Play_Particle(AnimationEvent e)
    {
        Transform transform = leftMuzleTrasnform;
        if (e.intParameter == 1)
            transform = rightMuzleTrasnform;

        Instantiate<GameObject>(particlePrefabs[0], transform.position, rootObject.transform.rotation);
        //base.Play_Particle(e);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);

        Transform transform = leftMuzleTrasnform;
        if (e.intParameter == 1)
            transform = rightMuzleTrasnform;

        SoundManager.Instance.PlaySFX(gunShootSound);
        GameObject obj = Instantiate<GameObject>(doActionDatas[index].Particle, transform.position, rootObject.transform.rotation);
        if (obj.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }
    }

    private void OnProjectileHit(Collider self, Collider other, Vector3 point)
    {
        Debug.Log($"self : {self} other : {other}");

        // hit Sound Play
        SoundManager.Instance.PlaySFX(doActionDatas[index].hitSoundName);

        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage != null)
        {
            Vector3 hitPoint = self.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            damage?.OnDamage(rootObject, this, hitPoint, doActionDatas[index]);

            return;
        }

        Instantiate<GameObject>(doActionDatas[index].HitParticle, point, rootObject.transform.rotation);
    }


    #region AutoTarget
    GameObject CheckTarget()
    {
        GameObject candidate = null;

        float angle = -2.0f;
        Collider[] targets = Physics.OverlapSphere(rootObject.transform.position, perceptionRange, (1 << LayerMask.NameToLayer("Enemy")));

        foreach (Collider target in targets)
        {

            Vector3 direction = target.gameObject.transform.position - rootObject.transform.position;
            direction.Normalize();

            Vector3 forward = rootObject.transform.forward;
            float signedAngle = Vector3.SignedAngle(forward, direction, Vector3.up);
            if (signedAngle > -1.0f)
                continue;

            float dot = Vector3.Dot(direction, forward);
            if (dot < 0.7f || dot < angle)
                continue;

            angle = dot;
            candidate = target.gameObject;
        }

        return candidate;
    }

    IEnumerator RotateToTarget(GameObject target)
    {
        if (target == null)
            yield break;

        Vector3 direction = target.transform.position - rootObject.transform.position;
        direction.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float rotationSpeed = 5.0f;

        // 목표 각도에 도달할 때까지 루프를 실행
        while (Quaternion.Angle(rootObject.transform.rotation, targetRotation) > 0.1f)
        {
            // 현재 회전을 목표 회전을 향해 보간합니다.
            rootObject.transform.rotation = Quaternion.Slerp(rootObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 다음 프레임을 기다립니다.
            yield return null;
        }
        rootObject.transform.rotation = targetRotation;
    }

    #endregion

    private void Equip_HandGun()
    {
        if (isSubAction == false)
        {
            Debug.Assert(gunObjects != null);
            foreach (GameObject go in gunObjects)
                go.gameObject.SetActive(true);
        }
        else
        {
            for (int i = 0; i < 2; i++)
                gunObjects[i].SetActive(false);
        }
    }

    private void Equip_Rifle()
    {
        if (isSubAction)
        {
            if (gunObjects.Length == 3)
            {
                rifleHolsterTransform.DetachChildren();
                gunObjects[2].transform.localPosition = Vector3.zero;
                gunObjects[2].transform.localRotation = Quaternion.identity;

                gunObjects[2].transform.SetParent(rifleHandTransform, false);
            }

            return;
        }
        
        if (gunObjects.Length == 3)
        {
            rifleHandTransform.DetachChildren();
            gunObjects[2].transform.localPosition = Vector3.zero;
            gunObjects[2].transform.localRotation = Quaternion.identity;

            gunObjects[2].transform.SetParent(rifleHolsterTransform, false);
        }
    }

    private void Shoot_Rifle()
    {
        animator.SetBool("IsAction", true);
    }

    private void CreateRifleBullet()
    {

    }
}