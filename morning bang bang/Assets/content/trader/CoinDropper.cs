using MothDIed;
using MothDIed.DI;
using UnityEngine;

namespace content.trader
{
    public class CoinDropper : MonoBehaviour
    {
        public int count;

        [Inject]
        private void Drop(Coin coinPrefab)
        {
            for (int i = 0; i < count; i++)
            {
                var instantiate = Game.CurrentScene.Fabric.Instantiate(coinPrefab, transform.position, null);
                instantiate.GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle * 100, ForceMode2D.Force);
            }
        }
    }
}