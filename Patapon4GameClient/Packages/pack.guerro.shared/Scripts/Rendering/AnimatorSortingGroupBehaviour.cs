using UnityEngine;
using UnityEngine.Rendering;

namespace Packet.Guerro.Shared.Rendering
{
    public class AnimatorSortingGroupBehaviour : MonoBehaviour
    { 
        public float OrderInLayer;
        public SortingGroup Reference;

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