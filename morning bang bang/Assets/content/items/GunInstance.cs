using banging_code.bullets;
using banging_code.common;
using MothDIed;
using UnityEngine;

namespace banging_code.items
{
    public class GunInstance : InHandsItemInstance
    {
        public Bullet bulletPrefab;
        public Transform firePoint;
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("BANG BANG");
                SpawnBullet();
            }
        }

        private void SpawnBullet()
        {
            var bullet = Game.CurrentScene.Fabric.Instantiate(bulletPrefab, firePoint.position);
            bullet.AddForce(UTLS.DirectionToVector(Game.RunSystem.Data.Level.PlayerInstance.Direction), 2);
        }
    }
}