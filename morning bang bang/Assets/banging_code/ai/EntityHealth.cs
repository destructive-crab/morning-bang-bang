using System;
using System.Collections;
using banging_code.common;
using banging_code.health;
using UnityEngine;

namespace banging_code.ai
{
    [RequireComponent(typeof(Enemy))]
    public class EntityHealth : HitableBody
    {
        [SerializeField] private Color hitColor;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Coroutine colorBlinkCoroutine;
        
        [field: SerializeField] public int Health { get; private set; }
        public event Action<Enemy> OnDie;

        public void TakeHit(HitData data)
        {
            Health -= data.DamageAmount;
            
            if(Health <= 0) OnDie?.Invoke(GetComponent<Enemy>());
            else
            {
                if(colorBlinkCoroutine != null) StopCoroutine(colorBlinkCoroutine);
                colorBlinkCoroutine = StartCoroutine(HitColor());
            }
        }

        private IEnumerator HitColor()
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.color = Color.white;
            colorBlinkCoroutine = null;
        }
        
        public override void TakeBulletHit(BulletHitData data)
        {
            TakeHit(data);
        }

        public override void TakeStabHit(StabHitData data)
        {
            TakeHit(data);
        }

        public override void TakeDumbHit(DumbHitData data)
        {
            TakeHit(data);
        }

        public override ID EntityID { get; }
    }
}