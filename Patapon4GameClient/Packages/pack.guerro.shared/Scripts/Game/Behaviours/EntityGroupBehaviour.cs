using Guerro.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Packet.Guerro.Shared.Game.Behaviours
{
    /// <summary>
    /// A class that link the group of this entity with the ECS-World from Unity-World. (Not a wrapper)
    /// </summary>
    [RequireComponent(typeof(GameObjectEntity))]
    public class EntityGroupBehaviour : MonoBehaviour
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Properties
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public int SceneGroupId = -1;
        public int GroupId;
        public int Version;

        private GameObjectEntity m_GameObjectEntity;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Unity Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        private void Awake()
        {
            m_GameObjectEntity = GetComponent<GameObjectEntity>();

            var manager = World.Active.GetOrCreateManager<CGameEntityGroupManager>();
            var group   = manager.AttachBehaviour(this);
            manager.AttachTo(m_GameObjectEntity.Entity, group);
        }

#if UNITY_EDITOR
        private void Update()
        {
            var entity        = m_GameObjectEntity.Entity;
            var entityManager = m_GameObjectEntity.EntityManager;

            var data = GetEntityGroupData();
            GroupId = data.ReferenceId;
            Version = data.Version;
        }
#endif

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public EntityGroup GetEntityGroupData()
        {
            var entity        = m_GameObjectEntity.Entity;
            var entityManager = m_GameObjectEntity.EntityManager;

            if (entityManager.HasComponent<EntityGroup>(entity))
                return entityManager.GetComponentData<EntityGroup>(entity);

            return default(EntityGroup);
        }
    }
}