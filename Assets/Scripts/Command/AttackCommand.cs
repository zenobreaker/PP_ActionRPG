using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCommand : ICommand
{
    private WeaponComponent weapon; 

    public AttackCommand(WeaponComponent weapon)
    {
        this.weapon = weapon;
    }
    public void Execute()
    {
        weapon.DoAction();    
    }    
}
