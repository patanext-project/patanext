using Unity.Entities;

namespace P4.Default.Movements
{
    public class P4Default_FreeMovementSystem : ComponentSystem
    {
        struct Group
        {
            
        }

        [Inject] private Group m_Group;
        
        protected override void OnUpdate()
        {
            
        }
    }
}