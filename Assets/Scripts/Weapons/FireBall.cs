using System;
using UnityEngine;

public class FireBall : Weapon
{
    [SerializeField] private string staffTransformName = "Hand_FireBall_Staff";
    [SerializeField] private string flameTransformName = "Hand_FireBall_Flame";
    [SerializeField] private string muzzleTransformName = "Hand_FireBall_Muzzle";

    [SerializeField] private GameObject flameParticleOrigin;
    [SerializeField] private GameObject projectilePrefab;


    private Transform staffTransform;

    private Transform flameTransform;
    private GameObject flameParticle;

    private Transform muzzleTransform;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.FireBall;
    }

    protected override void Awake()
    {
        base.Awake();

        staffTransform = rootObject.transform.FindChildByName(staffTransformName);
        Debug.Assert(staffTransform != null);
        transform.SetParent(staffTransform, false);

        flameTransform = rootObject.transform.FindChildByName(flameTransformName);
        Debug.Assert(flameTransform != null);


        if (flameParticleOrigin != null)
        {
            flameParticle = Instantiate<GameObject>(flameParticleOrigin, flameTransform);
            flameParticle.SetActive(false);
        }

        muzzleTransform = rootObject.transform.FindChildByName(muzzleTransformName);
        Debug.Assert(muzzleTransform != null);

        gameObject.SetActive(false);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
        flameParticle?.SetActive(true);
    }

    public override void Unequip()
    {
        base.Unequip();

        gameObject.SetActive(false);
        flameParticle?.SetActive(false);
    }

    public override void Play_Particle(AnimationEvent e)
    {
        base.Play_Particle(e);
        
        if (doActionDatas[0].Particle == null)
            return;

        Vector3 position = muzzleTransform.position;
        Quaternion rotation = rootObject.transform.rotation;

        Instantiate<GameObject>(doActionDatas[0].Particle, position, rotation);


    }

    public override void Play_Sound()
    {
        base.Play_Sound();
        // Sound Play
        SoundManager.Instance.PlaySFX(doActionDatas[0].effectSoundName);
    }

    public override void Begin_DoAction()
    {
        base.Begin_DoAction();

        if (projectilePrefab == null)
            return;

        Vector3 muzzlePosition = muzzleTransform.position;
        muzzlePosition += rootObject.transform.forward * 1.0f;

        GameObject obj = Instantiate<GameObject>(projectilePrefab, muzzlePosition,
            rootObject.transform.rotation);
        Projectile projectile = obj.GetComponent<Projectile>();
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }

        obj.SetActive(true);
    }


    private void OnProjectileHit(Collider self, Collider other, Vector3 point)
    {
        IDamagable damage = other.GetComponent<IDamagable>();

        if (damage != null)
        {
            Vector3 hitPoint = self.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            damage?.OnDamage(rootObject, this, hitPoint, doActionDatas[0]);
         
            return;
        }

        Instantiate<GameObject>(doActionDatas[0].HitParticle, point, rootObject.transform.rotation);
        // hit Sound Play
        SoundManager.Instance.PlaySFX(doActionDatas[0].hitSoundName);
    }
}