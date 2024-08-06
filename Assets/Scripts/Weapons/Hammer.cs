using UnityEngine;

public class Hammer : Melee
{
    [SerializeField] private string handName = "Hand_Hammer";

   
    private Transform handTransform;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Hammer;
    }

    protected override void Awake()
    {
        base.Awake();

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(handTransform, false);
        gameObject.SetActive(false);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
    }

    public override void Unequip()
    {
        base.Unequip();

        gameObject.SetActive(false);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);

     
    }

    public override void End_Collision()
    {
        base.End_Collision();

       
    }
}
