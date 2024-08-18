using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Trigger_Projectile : Skill_Trigger
{
    public enum Projectile_Diection
    {
        None = 0, Forward, Back, Right,Left, Down, Up,
    }

    [SerializeField] protected float lifeTime;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected bool bDestroyer;
    [SerializeField] protected Projectile_Diection prDirection = Projectile_Diection.None;

    protected float currentTime;
    protected float currHitDelayTime;
    protected Vector3 direction; 

    protected override void OnEnable()
    {
        base.OnEnable();

        switch(prDirection)
        {
            case Projectile_Diection.Forward:
            direction = Vector3.forward;
            break;
            case Projectile_Diection.Back:
            direction = Vector3.back;
            break;
            case Projectile_Diection.Right:
            direction = Vector3.right;
            break;
            case Projectile_Diection.Left:
            direction = Vector3.left;
            break;
            case Projectile_Diection.Down:
            direction = Vector3.down;
            break;
            case Projectile_Diection.Up:
            direction = Vector3.up;
            break;
        }
    }

    public override void SetSkillData(SkillData skillData)
    {
        base.SetSkillData(skillData);
        currentTime = lifeTime;
        if (skillData == null)
            return;

        currHitDelayTime = skillData.skillActions[0].HitDelayTime;
    }

    protected override void Update()
    {
        base.Update();

        transform.Translate(moveSpeed * Time.deltaTime * direction);
        currentTime -= Time.deltaTime;

        currHitDelayTime -= Time.deltaTime;

        Update_Apply_Skill();

        if (currentTime <= 0.0f)
            Destroy(gameObject);
    }

    private void Update_Apply_Skill()
    {
        if (currHitDelayTime > 0.0f)
        {
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);


        ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[0]);
        
        currHitDelayTime = skillData.skillActions[0].HitDelayTime;

        if (bDestroyer)
            Destroy(gameObject);
    }

    public override void ExecuteSkill()
    {
        Play_SkillMainSound();
        Play_SkillEffectParticle();
    }
}
