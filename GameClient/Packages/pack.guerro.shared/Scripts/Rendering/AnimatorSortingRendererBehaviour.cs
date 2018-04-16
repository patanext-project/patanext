using UnityEngine;
using UnityEngine.Rendering;

namespace Packet.Guerro.Shared.Rendering
{
    public class AnimatorSortingRendererBehaviour : MonoBehaviour
    { 
        public float OrderInLayer;
        public Renderer Reference;

        private void Update()
        {
            Reference.sortingOrder = Mathf.FloorToInt(OrderInLayer);
        }

        private void OnDrawGizmos()
        {
            Reference.sortingOrder = Mathf.FloorToInt(OrderInLayer);
        }
    }
}