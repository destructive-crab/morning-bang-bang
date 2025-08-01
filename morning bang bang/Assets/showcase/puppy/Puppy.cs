using System;
using banging_code.common;
using banging_code.health;
using UnityEngine;

public class Puppy : HitableBody 
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public override void TakeBulletHit(BulletHitData data)
    {
        animator.SetTrigger("HitBullet");
    }

    public override void TakeStabHit(StabHitData data)
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDumbHit(DumbHitData dumbHitData)
    {
        throw new System.NotImplementedException();
    }

    public override ID EntityID { get; }
}
