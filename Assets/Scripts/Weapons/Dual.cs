using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dual : Melee
{
    private enum PartType
    {
        DualLeft, DualRight, Max, Dual = 2,
    };

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Dual;
    }

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < (int)PartType.Max; i++)
        {
            Transform t = colliders[i].transform;

            //t.DetachChildren();
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;

            Dual_Trigger trigger = t.GetComponent<Dual_Trigger>();
            trigger.OnTrigger += OnTriggerEnter;

            string partName = ((PartType)i).ToString();
            Transform parent = rootObject.transform.FindChildByName(partName);
            Debug.Assert(parent != null);

            t.SetParent(parent, false);
            t.gameObject.SetActive(false);
        }
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        Debug.Assert(colliders != null);
        foreach (Collider collider in colliders)
            collider.gameObject.SetActive(true);
    }

    public override void Unequip()
    {
        base.Unequip();

        foreach (Collider collider in colliders)
            collider.gameObject.SetActive(false);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        //base.Begin_Collision(e);

        // 자기 공격만 활성화 부모는 콜 안함
        if ((PartType)e.intParameter == PartType.Dual)
        {
            for (int i = 0; i < (int)PartType.Max; i++)
            {
                colliders[i].enabled = true;
            }
            return;
        }
        colliders[e.intParameter].enabled = true;

    }

    protected override void SetParticleObject(int index)
    {
        if (particleTransforms == null)
            return;

        if (particlePrefabs == null)
            return;

        if (particlePrefabs.Length == 0 ||
            (index < 0 && index >= particlePrefabs.Length))
            return;

        if((PartType)index == PartType.Dual)
        {
            for (int i = 0; i < (int)PartType.Max; i++)
            {
                trailParticles[i] = Instantiate<GameObject>(particlePrefabs[i], particleTransforms[i]);
                trailParticles[i].transform.localPosition = Vector3.zero;
            }
            return;
        }

        base.SetParticleObject(index);
    }
}
