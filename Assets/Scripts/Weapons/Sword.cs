using UnityEngine;

public class Sword : Melee
{
    [SerializeField]
    private string holsterName = "Holster_Sword";
    [SerializeField]
    private string handName = "Hand_Sword";

    private Transform holsterTransform;
    private Transform handTransform;
    
    

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Sword;
    }

    protected override void Start()
    {
        base.Start();

        holsterTransform = rootObject.transform.FindChildByName(holsterName);
        Debug.Assert(holsterTransform != null, $"{rootObject.name} is error");

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(holsterTransform, false);

        
        //Debug.Assert(slashTransform != null);
    }

    //public override void PerpareWeapon()
    //{
    //    base.PerpareWeapon();

    //}

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(handTransform, false);
    }

    public override void Unequip()
    {
        base.Unequip();

        transform.parent.DetachChildren();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(holsterTransform, false);
    }

  

   
}
