using System.Collections;
using UnityEngine;

namespace Circles3D.Runtime.Player
{
    public class PlayerRagdoll : MonoBehaviour
    {
        public float lifetime = 10f;
        public float fadeoutTime = 0.5f;

        private IEnumerator Start()
        {
            var renderer = GetComponentInChildren<Renderer>();
            yield return new WaitForSeconds(lifetime);

            var p = 0f;
            var propertyBlock = new MaterialPropertyBlock();
            while (p < 1f)
            {
                propertyBlock.SetFloat("_Alpha", 1f - p);
                renderer.SetPropertyBlock(propertyBlock);

                p += Time.deltaTime / fadeoutTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}