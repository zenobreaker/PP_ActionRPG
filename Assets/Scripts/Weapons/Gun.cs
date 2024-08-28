using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

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
        [SerializeField] private float perceptionRange = 5.0f;

    [Header("Sound")]
    [SerializeField] private string Equip_HandGun_SoundName = "Equip_HandGun";
    [SerializeField] private string Equip_Rifle_SoundName = "Equip_Rifle";
    [SerializeField] private string gunShootSound = "Gun_Fire";
    [SerializeField] private string rifleShootSound = "Gun_Rifle_Fire";

    // 총기 오브젝트
    [Header("Gun Objects")]
    [SerializeField] private GameObject[] gunObjects;
    [SerializeField] private ScopeUI scopeUI;

    [Header("Rifle Ammo")]
    [SerializeField] int rifleMaxAmmo = 3;
    private int curr_RifleAmmo;

    private RifleGun rifleGun;

    private Transform leftMuzleTrasnform;
    private Transform rightMuzleTrasnform;
    private Transform rifleHolsterTransform;
    private Transform rifleHandTransform;

    private Coroutine rotateCoroutine;

    private bool bTurn = false; 
    public bool Turn { get => bTurn; }

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Gun;
    }


    protected override void Awake()
    {
        base.Awake();

        scopeUI = FindObjectOfType<ScopeUI>();

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
            rifleGun = gunObjects[2].transform.GetComponent<RifleGun>();
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

        Awake_InitRifleAmmo();
    }

    private void Awake_InitRifleAmmo()
    {
        curr_RifleAmmo = rifleMaxAmmo;
    }

    protected override void Update()
    {
        base.Update();

        if (isSubAction)
        {
            GameObject cameraObj = rifleGun?.GetCameraObj();
            if (cameraObj == null)
                return;

            Ray ray = new Ray(cameraObj.transform.position, cameraObj.transform.forward * 3500.0f);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 3500.0f, 1 << LayerMask.NameToLayer("Enemy")))
            {
                scopeUI.LockOn_AdditiveScope();
            }
            else
            {
                scopeUI.LockOff_AdditiveScope();
            }

            return;
        }
    }

    public override void DoAction(int comboIndex = 0, bool bNext = false)
    {
        if (isSubAction)
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

        bTurn = false;
        if (isSubAction)
            RotateToCameraFoward();

        animator.SetBool("SubActionMode", isSubAction);
        Stop();

        if (isSubAction == false)
        {
          
            Equip_HandGun();
            Equip_Rifle();
            Move();
        }
    }

    private void RotateToCameraFoward()
    {
        GameObject camera = rifleGun.GetCameraObj();
        if (camera == null)
            return;
        Vector3 forward = camera.transform.forward;

        forward.y = 0; 

        rootObject.transform.rotation = Quaternion.LookRotation(forward);
     
    }

    public override void End_DoAction()
    {
        base.End_DoAction();

        if (isSubAction)
        {
            // 총알 없으면 스나이프 모드 해제 
            if (curr_RifleAmmo <= 0)
            {
                // 라이플 장착 해제 
                Unequip_Rifle();
                // 다시 총 장착 
                Equip_HandGun();
                return;
            }
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


    public override void End_Equip()
    {
        base.End_Equip();

        if (isSubAction)
        {
            SetSnipeMode();
            bTurn = true;
            return;
        }
    }

    public override void Unequip()
    {
        base.Unequip();

        Debug.Assert(gunObjects != null);
        //foreach (GameObject go in gunObjects)
        //    go.SetActive(false);

        for (int i = 0; i < 2; i++)
            gunObjects[i].SetActive(false);

        Unequip_Rifle();
    }

    public void Unequip_Rifle()
    {
        isSubAction = false;
        animator.SetBool("SubActionMode", isSubAction);
        Equip_Rifle();
    }

    public override void Play_Particle(AnimationEvent e)
    {
        if (isSubAction)
        {

            return;
        }

        Transform transform = leftMuzleTrasnform;
        if (e.intParameter == 1)
            transform = rightMuzleTrasnform;

        Instantiate<GameObject>(particlePrefabs[0], transform.position, rootObject.transform.rotation);
        //base.Play_Particle(e);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);

        if (isSubAction)
        {
            CreateRifleBullet();

            return;
        }

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

            SoundManager.Instance.PlaySFX(Equip_HandGun_SoundName);
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
            if (rifleGun != null)
            {
                rifleHolsterTransform.DetachChildren();
                rifleGun.transform.localPosition = Vector3.zero;
                rifleGun.transform.localRotation = Quaternion.identity;

                rifleGun.transform.SetParent(rifleHandTransform, false);

                SoundManager.Instance.PlaySFX(Equip_Rifle_SoundName);
            }

            return;
        }

        if (rifleGun == null)
            return;

        {
            rifleHandTransform.DetachChildren();
            rifleGun.transform.localPosition = Vector3.zero;
            rifleGun.transform.localRotation = Quaternion.identity;

            rifleGun.transform.SetParent(rifleHolsterTransform, false);

            EndSnimpeMode();
        }
    }

    private void Shoot_Rifle()
    {
        scopeUI.Shoot_Snipe();

        animator.SetBool("IsAction", true);
        Use_Ammo();

    }

    private void Use_Ammo(int ammount = 1)
    {
        if (curr_RifleAmmo > 0)
            curr_RifleAmmo -= ammount;
    }

    private void CreateRifleBullet()
    {
        if (subActionDatas.Length <= 0)
            return;

        GameObject muzzle = rifleGun.GetMuzzleObj();
        GameObject scope = rifleGun.GetCameraObj();

        SoundManager.Instance.PlaySFX(rifleShootSound);
        
        // 머즐 이펙트 
        if(muzzle != null && subActionDatas[0].Particle == null)
        {
            Instantiate<GameObject>(particlePrefabs[0], muzzle.transform.position, 
                muzzle.transform.rotation);
        }

        // 진동
        {
            Play_Impulse(subActionDatas[0]);
        }

        // 탄환 -
        if (scope != null && subActionDatas[0].Particle != null)
        {
            GameObject obj = Instantiate<GameObject>(subActionDatas[0].Particle, scope.transform.position,
                scope.transform.rotation);
            if (obj.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.OnProjectileHit += OnRifleProjectileHit;
            }
        }
    }

    private void OnRifleProjectileHit(Collider self, Collider other, Vector3 point)
    {
        Debug.Log($"self : {self} other : {other}");

        // hit Sound Play
        SoundManager.Instance.PlaySFX(subActionDatas[0].hitSoundName);

        IDamagable damage = other.GetComponent<IDamagable>();
        if (damage != null)
        {
            Vector3 hitPoint = self.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            damage?.OnDamage(rootObject, this, hitPoint, subActionDatas[0]);

            return;
        }

        Instantiate<GameObject>(subActionDatas[0].HitParticle, point, rootObject.transform.rotation);
    }

    // 스나이프 모드
    private void SetSnipeMode()
    {
        if (rifleGun == null)
            return;

        // 총알 장전
        curr_RifleAmmo = rifleMaxAmmo;
        rifleGun.SetSnipeMode();
        scopeUI?.SetDrawSnipeUI(rifleMaxAmmo);
    }

    private void EndSnimpeMode()
    {
        if (rifleGun == null)
            return;

        rifleGun.EndSnipeMode();
        scopeUI?.EndDrawSnipeUI();
    }
}