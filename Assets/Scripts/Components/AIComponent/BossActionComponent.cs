using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossPatternData
{
    public string name;
    public GameObject patternObject;
    public DoActionData data;
}


public class BossActionComponent : MonoBehaviour
{

    [SerializeField] private BossPatternData[] originPrfabs;

    [SerializeField]  private string muzzleTransformName = "Hand_FireBall_Muzzle";


    private Animator animator;
    private WeaponComponent weapon;

    private Transform muzzleTransform;

    private Dictionary<string, BossPatternData> bossPatternTable;

    private void Awake()
    {
        animator = GetComponent<Animator>();    
        Debug.Assert(animator != null); 
        weapon = GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);

        muzzleTransform = transform.FindChildByName(muzzleTransformName);
        Debug.Assert(muzzleTransform != null);

        bossPatternTable = new Dictionary<string, BossPatternData>();

        foreach(BossPatternData data in originPrfabs)
            bossPatternTable.Add(data.name, data);

    }

    public void DoPattern(int pattern )
    {
        animator.SetInteger("Pattern", pattern);
        animator.SetBool("IsAction", true);
    }

    public void Begin_ShootFire()
    {
        if (bossPatternTable == null)
            return;

        Vector3 muzzlePosition = muzzleTransform.position;
        muzzlePosition += transform.forward * 1.0f;

        var data = bossPatternTable["FireBall"];
        if (data == null)
            return; 

        GameObject obj = Instantiate<GameObject>(data.patternObject, muzzlePosition,
            transform.rotation);
        obj.name = data.name;
        Projectile projectile = obj.GetComponent<Projectile>();
        {
            projectile.OnProjectileHit += OnProjectileHit;
        }

        obj.SetActive(true);
    }

    private BossPatternData FindDataInTableWithObject(string name)
    {
        foreach(var data in bossPatternTable)
        {
            if (data.Value.name == name)
                return data.Value;
        }

        return null;
    }

    private void OnProjectileHit(Collider self, Collider other, Vector3 point)
    {
        IDamagable damage = other.GetComponent<IDamagable>();

        if (damage != null)
        {
            Vector3 hitPoint = self.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            BossPatternData patternData = FindDataInTableWithObject(self.gameObject.name);
            Debug.Log($"{weapon == null} / {patternData.data == null}");
            damage?.OnDamage(transform.gameObject, weapon.GetEquippedWeapon(), hitPoint, patternData.data);

            return;
        }

        //Instantiate<GameObject>(doActionDatas[0].HitParticle, point, rootObject.transform.rotation);

    }

}
