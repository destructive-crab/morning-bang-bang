using System;
using banging_code.camera_logic;
using banging_code.health;
using MothDIed;
using MothDIed.DI;
using Unity.Mathematics;
using UnityEngine;

#pragma warning disable CS4014

namespace banging_code.bullets
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        public int damageAmount;
        public GameObject hitEffect;
        
        [Inject] private HitsHandler hitsHandler;
        [Inject] private CCamera cCamera;
        
        public virtual BulletHitData GetHitData() => new BulletHitData(damageAmount);

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent(out HitableBody hitableBody))
            {
                hitsHandler.Hit(GetHitData(), hitableBody);
            }

            GameObject.Instantiate(hitEffect, transform.position, quaternion.identity);
            Game.SceneSwitcher.CurrentScene.Fabric.Destroy(gameObject);
        }

        public void AddForce(Vector2 direction, int multiplier)
        {
            transform.Rotate(0, 0, MathF.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            GetComponent<Rigidbody2D>().AddForce(direction * (multiplier * 10), ForceMode2D.Impulse);
            cCamera.Shake(1, 0.1f);
        }
    }
}