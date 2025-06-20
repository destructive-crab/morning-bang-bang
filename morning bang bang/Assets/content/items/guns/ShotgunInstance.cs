using System.Collections.Generic;
using banging_code.health;
using MothDIed.DI;
using UnityEngine;
using UnityEngine.Serialization;

public class ShotgunInstance : MonoBehaviour
{
    [SerializeField] private ParticleSystem shootParticle;
    [SerializeField] private Collider2D shotgunTrigger;
    [SerializeField] private Transform firepoint;

    [Inject] private HitsHandler hitsHandler;
    
    private void Update()
    {
         if(Input.GetMouseButtonDown(0))
         {
//                gunAnimator.Play("GunAnimation");
             Shoot();
         }       
    }

    private void Shoot()
    {
        shootParticle.Play();
        ContactFilter2D filter2D = new();
        List<Collider2D> results = new();

        Physics2D.OverlapCollider(shotgunTrigger, filter2D, results);
        
        foreach (Collider2D result in results)
        {
            if (result.gameObject.TryGetComponent(out HitableBody hitableBody))
            {
                Debug.Log((int)(10f * (1 / Vector3.Distance(firepoint.position, result.transform.position))));
                
                hitsHandler.Hit(
                    new BulletHitData(
                        (int)(10f * (1 / Vector3.Distance(firepoint.position, result.transform.position)))),
                    hitableBody);
            }
        }
    }
}