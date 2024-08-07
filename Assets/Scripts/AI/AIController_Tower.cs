using UnityEditor;
using UnityEngine;

public class AIController_Tower : AIController
{
    protected override void Awake()
    {
        base.Awake();
    }


  

    protected override void FixedUpdate()
    {
        if (CheckCoolTime())
            return;

        if (CheckMode())
            return;


        GameObject player = perception.GetPercievedPlayer();

        if (player == null)
        {
            if (weapon.UnarmedMode == false)
                weapon.SetUnarmedMode();
            
            return;
        }

        if (weapon.UnarmedMode == true)
        {
            SetEquipMode(enemy.weaponType);
            return;
        }

        // 공격 조건 
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange)
        {
            if (weapon.UnarmedMode == false)
            {
                SetActionMode();
                return;
            }
        }

    }

    protected override void LateUpdate()
    {
        bool bCheck = false;
        bCheck |= state.DeadMode;
        bCheck |= enemy.DownCondition;
        if (bCheck)
        {
            Debug.Log("is Dead");
            SetNavMeshStop(true);
            return;
        }
    }


#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        if (Selection.activeGameObject != gameObject)
            return;
    }

 

#endif
}
