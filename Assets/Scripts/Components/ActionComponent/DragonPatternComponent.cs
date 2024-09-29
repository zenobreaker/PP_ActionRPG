using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    public DoActionData[] fireballActionDatas;

    private PerceptionComponent perception;
    private StateComponent state;
    private Animator animator;

    [SerializeField] private int currentPattern;
    [SerializeField] private string fireballPoint = "Fire Point FireBall";
    [SerializeField] private string breathPoint = "Fire Point Breath";
    //[SerializeField] private string triggerBite = "Attack Trigger Bite";
    //[SerializeField] private string triggerWingLeft = "TriggerWing_Left";
//    [SerializeField] private string triggerWingRight = "TriggerWing_Right";

    [SerializeField] private GameObject fireballObject;
    [SerializeField] private GameObject fireball2Object;
    [SerializeField] private GameObject fireball2_Second_Object;
    [SerializeField] private GameObject firebreathObject;
    private Transform fireballTransform; 
    private Transform breathTransform;
    private GameObject curFirebreahObject; 

    public GameObject biteTrigger;
    public GameObject wingTriggerL;
    public GameObject wingTriggerR;

    public event Action OnBeginDoAction;
    public event Action OnEndDoAction;

    private bool isAction;

    private static readonly int Mode = Animator.StringToHash("Mode");
    private static readonly int IDInt = Animator.StringToHash("IDInt");
    private static readonly int State = Animator.StringToHash("State");

    private void Awake()
    {
        perception = GetComponent<PerceptionComponent>();
        state = GetComponent<StateComponent>();
        animator = GetComponent<Animator>();

        //Transform  trans = this.gameObject.transform.FindChildByName(triggerBite);
        //biteTrigger = trans?.gameObject;
        //trans = this.gameObject.transform.FindChildByName(triggerWingLeft);
        //wingTriggerL = trans?.gameObject;
        //trans = this.gameObject.transform.FindChildByName(triggerWingRight);
        //wingTriggerR = trans?.gameObject;

        fireballTransform = this.gameObject.transform.FindChildByName(fireballPoint);
        breathTransform = this.gameObject.transform.FindChildByName(breathPoint);
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

        state.SetActionMode();
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

        StartCoroutine(FireBreathCoroutine(4));
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

        OnBeginDoAction?.Invoke();
    }

    public void End_DoAction()
    {
        if (animator != null)
        {
            animator.SetInteger(Mode, 0);
            animator.SetInteger(IDInt, -2);
        }

        isAction = false;
        state.SetIdleMode();

        OnEndDoAction?.Invoke();
    }

    public void Begin_Collision(AnimationEvent e)
    {

        if(e.intParameter == 0)
        {
            biteTrigger.SetActive(true);
        }
        else if(e.intParameter  == 1)
        {
            wingTriggerL.SetActive(true);
        }
        else if(e.intParameter == 2)
        {
            wingTriggerR.SetActive(true);
        }
    }

    public void End_Collision()
    {
        biteTrigger.SetActive(false);
        wingTriggerL.SetActive(false);
        wingTriggerR.SetActive(false);
    }


    private IEnumerator FirebreathCollisionEnableCoroutine()
    {
        if (firebreathObject == null)
            yield break; 


        curFirebreahObject = Instantiate<GameObject>(firebreathObject, breathTransform.position , gameObject.transform.rotation);
        DragonBreath dragonBreath = curFirebreahObject.GetComponent<DragonBreath>();  
        if (dragonBreath == null)
            yield break;
        SoundManager.Instance.PlaySFX("DragonBreath");
        dragonBreath.owner = this.gameObject;

        yield return null;

        yield return new WaitForSeconds(4);
        Destroy(curFirebreahObject);
    }



    private IEnumerator FireBreathCoroutine(float delay)
    {
        StartCoroutine(FirebreathCollisionEnableCoroutine());

        yield return new WaitForSeconds(delay);

        Debug.Log("Firebreath End");
        End_DoAction();
    }


    public void PlaySound(string soundName)
    {
        SoundManager.Instance.PlaySFX(soundName);
    }

    public void Begin_Particle()
    {
        if(currentPattern == 3)
        {
            SoundManager.Instance.PlaySFX(fireballActionDatas[0].effectSoundName);
            Instantiate<GameObject>(fireballActionDatas[0].Particle, fireballTransform.position, gameObject.transform.rotation);
            GameObject obj = Instantiate<GameObject>(fireballObject, fireballTransform.position, gameObject.transform.rotation);
            if(obj.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.OnProjectileHit += OnProjectileHit_Fireball;
            }
            obj.SetActive(true);
        }
        else if(currentPattern == 5)
        {
            SoundManager.Instance.PlaySFX(fireballActionDatas[1].effectSoundName);
            GameObject obj = Instantiate<GameObject>(fireball2Object, fireballTransform.position,
                fireballTransform.transform.rotation);
            if (obj.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.OnProjectileHit += OnProjectileHit_Fireball2;
            }
            obj.SetActive(true);
        }
    }

    private void OnProjectileHit_Fireball(Collider self, Collider other, Vector3 point)
    {
        IDamagable damage = other.GetComponent<IDamagable>();

        if (damage != null)
        {
            Vector3 hitPoint = self.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            damage?.OnDamage(this.gameObject, null, hitPoint, fireballActionDatas[0]);

            return;
        }

        Instantiate<GameObject>(fireballActionDatas[0].HitParticle, point, gameObject.transform.rotation);
        // hit Sound Play
        SoundManager.Instance.PlaySFX(fireballActionDatas[0].hitSoundName);
    }

    private void OnProjectileHit_Fireball2(Collider self, Collider other, Vector3 point)
    {
        IDamagable damage = other.GetComponent<IDamagable>();
        
        SoundManager.Instance.PlaySFX(fireballActionDatas[1].hitSoundName);
        GameObject obj = Instantiate<GameObject>(fireball2_Second_Object, point, gameObject.transform.rotation);
        if(obj.TryGetComponent<DragonMeleeWeapon>(out DragonMeleeWeapon weapon))
        {
            weapon.owner = this.gameObject;
        }

        if (damage != null)
        {
            Vector3 hitPoint = self.ClosestPoint(other.transform.position);
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            damage?.OnDamage(this.gameObject, null, hitPoint, fireballActionDatas[1]);

            return;
        }

        //Instantiate<GameObject>(fireballActionDatas[1].HitParticle, point, gameObject.transform.rotation);
        // hit Sound Play
    }

}
