using Godot;
using revecs.Core;

namespace PataNext.Presentations.Animations;

public partial class AnimationComponent : Node3D
{
    public struct Animation
    {
        public bool IsActive;
    }
    
    public Node3D Node { get; private set; }

    [Export] public AnimationTree AnimationTree;

    public AnimationNodeBlendTree BlendTree => (AnimationNodeBlendTree) AnimationTree.TreeRoot;
    public AnimationPlayer Player => GetNode<AnimationPlayer>(AnimationTree.AnimPlayer);

    public Animation Current;
    
    public override void _Ready()
    {
        base._Ready();

        Node = GetParentNode3d();
    }

    public bool HasAnimationSystem(ComponentType type)
    {
        return false;
    }

    public void AddAnimationSystem<T>(ComponentType<T> type, in T data)
    {
        
    }

    public void SetAnimation(ComponentType type, bool canBeOverriden)
    {
        
    }
}