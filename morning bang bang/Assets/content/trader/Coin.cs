using System;
using banging_code.player_logic;
using MothDIed;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Rigidbody2D rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent(out PlayerRoot playerRoot))
        {
            Game.RunSystem.Data.Money++;
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (rigidbody.velocity != Vector2.zero)
        {
            rigidbody.velocity = Vector2.MoveTowards(rigidbody.velocity, Vector2.zero, 0.1f);
        }
    }
}
