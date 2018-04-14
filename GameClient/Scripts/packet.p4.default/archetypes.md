<html>
    <p align="center">
    <img src="https://pre00.deviantart.net/4960/th/pre/i/2017/334/1/6/_patapon_4_tlb__p4_logo_variant_2_by_guerro323-dbvceq0.png" alt="Super Logo!" width="64" height="64" />
    </p>
    <h2 align="center">
    Patapon 4 - Default Package - Entity archetypes
    </h2>
</html>

___
List:
------
- [Entity for Movement](#archetype-entity-for-movement)

## Archetype "Entity for movement"

```c#
//! NOT A CLASS FOR COMPILATION

/* Default Folder location: P4Main>Scripts>p4.default
 * Components naming:
 * - Entity side            :> P4Default_D*Name      *Data
 * - Entity Wrapper side    :> P4Default_D*Name      *Wrapper
 * - Entity Component       :> P4Default_D*Name      *Component
 *
 * - Behaviours             :> P4Default_*Name      *Behaviour
 * - Systems                :> P4Default_*Name      *System
 * - Coordinators           :> P4Default_*Function  *Coordinators
 */
 /* Namespaces used:
 * P4Default:
 * - P4.Default
 * - P4.Default.Movements
 * P4Core:
 * - P4.Core.ECS
 * Unity:
 * - UnityEngine
 * - Unity.Entities
 */

/* This class indicate the default archetype of an entity for basic movements (Free and Rythmics) */
struct P4Default_EntityForMovementArchetype
{
    // ------ ------ ------ ------ ------ ------ ------ /.
    // Needed
    // ------ ------ ------ ------ ------ ------ ------ /.
    public P4Default_DMovementDetailData detail;    // Needed for the coordinator and systems. (RW for the Coordinator and R for the Systems)
    public P4Default_DEntityInputData    input;     // Needed for the coordinator and systems. (RW for the Coordinator and Input system, and R for the other Systems)
    public DWorldPositionData           position;   //< Rotation and Position can be combined to TWorldTransform (RW)
    public DWorldRotationData           rotation;   //^
    public Rigidbody                    rigidbody;  // Actually, I need to think if we reaaaally need a rigidbody
    public DCharacterData               character;  //< A data struct from stormium. I don't know if I keep it.
    public DCharacterInformationData    charInfo;   //^
    public DCharacterColliderShared     charColl;   //^

    // ------ ------ ------ ------ ------ ------ ------ /.
    // Optional
    // ------ ------ ------ ------ ------ ------ ------ /.
    public P4Default_FreeMovementData   freeMovementComponent;  // Read-only
    public P4Default_RythmMovementData  rythmMovementComponent; // Read-only
}
```