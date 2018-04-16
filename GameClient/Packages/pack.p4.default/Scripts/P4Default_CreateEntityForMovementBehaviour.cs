using P4.Default.Movements;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Transforms;
using Unity.Entities;
using UnityEngine;

namespace P4.Default
{
    [AddComponentMenu("Moddable/P4Default/EntityArchetype Creator/Entity For Movement")]
    [RequireComponent
    (
        typeof(Rigidbody),
        typeof(DCharacterWrapper),
        typeof(DCharacterCollider2DComponent)
    )]
    public class P4Default_CreateEntityForMovementBehaviour
        : CGameEntityCreatorBehaviour<P4Default_CreateEntityForMovementSystem>
    {
    }

    public class P4Default_CreateEntityForMovementSystem
        : CGameEntityCreatorSystem
    {
        public override void FillEntityData(GameObject gameObject, Entity entity)
        {
            AddComponents(gameObject,
                typeof(P4Default_DMovementDetailWrapper),
                typeof(P4Default_DEntityInputUnifiedWrapper),
                typeof(DWorldPositionWrapper),
                typeof(DWorldRotationWrapper),
                typeof(DCharacterInformationWrapper)
            );
        }

        protected override void OnUpdate()
        {

        }
    }
}