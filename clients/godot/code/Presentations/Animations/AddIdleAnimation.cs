using Godot;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revghost;

namespace PataNext.Presentations.Animations;

public partial class AddIdleAnimation : AnimationSystemBase<AddIdleAnimation.AnimData>
{
    private AnimationLibrary _library = ResourceLoader.Load<AnimationLibrary>("res://models/units/uberhero/anims/idle.tres");
    
    public AddIdleAnimation(Scope scope) : base(scope)
    {
    }
    
    protected override void OnAnimationUpdate(UEntityHandle entity, AnimationComponent component)
    {
        if (!component.HasAnimationSystem(SystemType))
        {
            // First register our animation system
            component.AddAnimationSystem(SystemType, new AnimData());
            
            // Add the 'idle' animation to the player...
            component.Player.AddAnimationLibrary("idle", _library);
            
            // Add the animation node and connect it to the graph
            component.BlendTree.AddNode("idle", new AnimationNodeAnimation
            {
                Animation = "idle"
            });

            component.BlendTree.ConnectNode(
                "root",
                component.BlendTree.GetNode("root").GetInputCount(),
                "idle"
            );
        }
        
        if (!component.Current.IsActive)
            component.SetAnimation(SystemType, true);
    }
    
    public partial struct AnimData : ISparseComponent
    {
        
    }
}