using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable 
{
    void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data);
}
