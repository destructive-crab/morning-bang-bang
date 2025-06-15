using System;
using banging_code.bullets;
using banging_code.common;
using MothDIed;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.items
{
    public class GunInstance : InHandsItemInstance
    {
        public Bullet bulletPrefab;
        public Transform firePoint;
        private Animator gunAnimator;

        private void Awake()
        {
            gunAnimator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            var gun = (Game.RunSystem.Data.Inventory.Main as Gun);

            if (gun != null)
            {
                bulletPrefab = gun.BulletPrefab;
            }

            InputService.OnUseMainItem += SpawnBullet;
        }

        private void OnDisable()
        {
            InputService.OnUseMainItem -= SpawnBullet;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
//                gunAnimator.Play("GunAnimation");
            }
        }

        private void SpawnBullet()
        {
            var bullet = Game.SceneSwitcher.CurrentScene.Fabric.Instantiate(bulletPrefab, firePoint.position);
            bullet.AddForce(UTLS.DirectionToVector(Game.RunSystem.Data.Level.PlayerInstance.Direction), 2);
        }
    }
}