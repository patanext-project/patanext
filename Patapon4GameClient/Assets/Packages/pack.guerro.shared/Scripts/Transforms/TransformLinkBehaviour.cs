using UnityEngine;

namespace _GameAssets.packet.guerro.shared.Scripts.Transforms
{
    public class TransformLinkBehaviour : MonoBehaviour
    {
        public Transform ParentTransform;
        public bool      SynchronizePosition = true;
        public bool      SynchronizeRotation = true;
        public bool      SynchronizeScale    = false;

        private void Awake()
        {
            Camera.onPreRender += __OnPreRender;
        }
        
        private void __OnPreRender(Camera c)
        {
            if (SynchronizePosition) transform.position = ParentTransform.position;
            if (SynchronizeRotation) transform.rotation = ParentTransform.rotation;
            if (SynchronizeScale) transform.localScale  = ParentTransform.localScale;
        }

        private void OnDestroy()
        {
            Camera.onPreRender -= __OnPreRender;
        }
    }
}