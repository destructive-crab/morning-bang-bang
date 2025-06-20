using System;
using banging_code.player_logic;
using MothDIed;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Rigidbody2D coinRigidbody;

    private void Awake()
    {
        
        coinRigidbody = GetComponent<Rigidbody2D>();
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
        if (coinRigidbody.velocity != Vector2.zero)
        {
            coinRigidbody.velocity = Vector2.MoveTowards(coinRigidbody.velocity, Vector2.zero, 0.1f);
        }
    }
}
