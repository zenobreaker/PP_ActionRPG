using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Dragon 클래스를 위한 패턴의 정의된 컴포넌트 
/// </summary>

public class DragonPatternComponent
    : MonoBehaviour
    , IActionComponent
    , ICollisionHandler
    , IPatternHandler
{
    private PerceptionComponent perception;

    private Animator animator;

    [SerializeField] private int currentPattern;

    public event Action OnEndDoAction;

    private bool isAction;

    private static readonly int Mode = Animator.StringToHash("Mode");
    private static readonly int IDInt = Animator.StringToHash("IDInt");
    private static readonly int State = Animator.StringToHash("State");

    private void Awake()
    {
        perception = GetComponent<PerceptionComponent>();
        animator = GetComponent<Animator>();
    }

    public void SetPattern(int pattern)
    {
        currentPattern = pattern;
    }

    public int GetPattern()
    {
        return currentPattern;
    }

    public void DoAction()
    {
        if (isAction)
            return;

        isAction = true;
        Debug.Log($"Dragon Action ! {currentPattern}");
        
        animator.SetInteger(IDInt, 0);

        if (currentPattern == 1)
        {
            DoAction_Bite();
        }

        if (currentPattern == 2)
        {
            DoAction_WingAttack();
        }

        if (currentPattern == 3)
        {
            DoAction_ShootFireball();
        }

        if (currentPattern == 4)
        {
            DoAction_Firebreath();
        }

        if (currentPattern == 5)
        {
            DoAction_FlyAndFire();
        }
    }


    // 물기 
    private void DoAction_Bite()
    {
        if (animator == null)
            return;
        
        animator.SetInteger(Mode, 1004);
    }

    // 날개 치기 

    private void DoAction_WingAttack()
    {
        if (animator == null)
            return;

        if (perception == null)
            return;

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            return;

        Vector3 forward = gameObject.transform.forward;
        Vector3 dest = player.transform.position;
        Vector3 cross = Vector3.Cross(forward, dest);
        float dot = Vector3.Dot(cross, Vector3.up);

        if(dot < 0 )
            animator.SetInteger(Mode, 1009);
        else
            animator.SetInteger(Mode, 1010);
    }


    // 화염탄 발사
    private void DoAction_ShootFireball()
    {
        if (animator == null)
            return;

        animator.SetInteger(Mode, 2001);
     
        animator.SetInteger(State, 1);
    }

    // 화염 방사
    private void DoAction_Firebreath()
    {
        if (animator == null)
            return;

        animator.SetInteger(Mode, 2002);

        StartCoroutine(FireBreathCoroutine(2));
    }

    // 날아오르고 화염 
    private void DoAction_FlyAndFire()
    {
        if (animator == null)
            return;

        animator.SetInteger(Mode, 1013);
    }

    public void Begin_DoAction()
    {
        if (animator != null)
        {
            animator.SetInteger(Mode, 0);
        }
    }

    public void End_DoAction()
    {
        if (animator != null)
        {
            animator.SetInteger(Mode, 0);
            animator.SetInteger(IDInt, -2);
        }

        isAction = false;

        OnEndDoAction?.Invoke();
    }

    public void Begin_Collision(AnimationEvent e)
    {
        throw new System.NotImplementedException();
    }

    public void End_Collision()
    {
        throw new System.NotImplementedException();
    }


    private IEnumerator FireBreathCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Firebreath End");
        End_DoAction();
    }

    public void PlaySound(string soundName)
    {
        //
    }

}
