using Packet.Guerro.Shared.Characters;
using Packet.Guerro.Shared.Game;
using Packet.Guerro.Shared.Network;
using Packet.Guerro.Shared.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace P4.Default.Movements
{
    [UpdateAfter(typeof(P4Default_MovementCoordinators))]
    public class P4Default_FreeMovementSystem : ComponentSystem
    {
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

            #region Optionnal

            public ComponentDataArray<P4Default_DFreeMovementData> DataFreeMovementComponents;

            #endregion

            public int Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private Group m_Group;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Base methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        protected override void OnUpdate()
        {
            for (int i = 0; m_Group.Length != i; ++i)
            {
                var input = m_Group.UnifiedInputs[i];
                var inputDirection = input.FreeInput.MoveDirection;

                var character = m_Group.Characters[i];
                var rigidbody = m_Group.Rigidbodies[i];
                
                var contactFilter = new ContactFilter2D();
                contactFilter.useNormalAngle = true;
                contactFilter.SetNormalAngle(character.MaximumStepAngle - 0.5f, character.MaximumStepAngle + 0.5f);

                var isGrounded = Physics2D.IsTouching(m_Group.CharactersColliders[i].MovementCollider, contactFilter);
                var velocity = rigidbody.velocity;

                if (isGrounded)
                {
                    velocity.x = math.lerp(velocity.x, inputDirection.x * 10, Time.deltaTime * 25f);
                }
                else
                {
                    velocity.x = math.lerp(velocity.x, inputDirection.x * 10, Time.deltaTime * 5f);
                }

                rigidbody.velocity = velocity;
            }
        }
    }
}