using System;
using JetBrains.Annotations;
using P4.Default.Inputs;
using P4.Default.Movements;
using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Network;
using Packet.Guerro.Shared.Transforms;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace P4.Default
{
    [UsedImplicitly]
    [UpdateAfter(typeof(P4Default_EntityInputFreeSystem))]
    public class P4Default_MovementCoordinators : ComponentSystem
    {
        public class RequireMinimalAttribute : RequireComponentTagAttribute
        {
            public RequireMinimalAttribute()
            {
                this.TagComponents = RequireMinimal;
            }
        }

        public static Type[] RequireMinimal =
        {
            typeof(P4Default_DMovementDetailData),
            typeof(P4Default_DEntityInputUnifiedData),
            typeof(DWorldPositionData),
            typeof(DWorldRotationData),
            typeof(Rigidbody2D),
            typeof(DCharacterData),
            typeof(DCharacterInformationData),
            typeof(DCharacterCollider2DComponent)
        };
        
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        struct Group
        {
            #region Necessary Types (archetype:P4Default_EntityForMovementArchetype)

            public            ComponentDataArray<P4Default_DMovementDetailData>     Details;
            public            ComponentDataArray<P4Default_DEntityInputUnifiedData> UnifiedInputs;
            public            ComponentDataArray<DWorldPositionData>                Positions;
            public            ComponentDataArray<DWorldRotationData>                Rotations;
            public            ComponentArray<Rigidbody2D>                           Rigidbodies;
            public            ComponentDataArray<DCharacterData>                    Characters;
            public            ComponentDataArray<DCharacterInformationData>         CharactersInformations;
            public            ComponentArray<DCharacterCollider2DComponent>         CharactersColliders;
            [ReadOnly] public SharedComponentDataArray<NetworkEntity>               NetworkEntities;
            [ReadOnly] public SharedComponentDataArray<ControllableEntity>          ControllableEntities;

            #endregion

            public int Length;
        }

        [Inject] private Group m_Group;
        
        // TODO: Make it working in multiplayer, for now, I've just done a fast draft and concept
        protected override void OnUpdate()
        {
        }
    }
}