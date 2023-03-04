using Godot;

namespace PataNext.Presentations;

[Tool]
public partial class UberheroShoulderSpline_LookAt : Node3D
{
    [Export] public NodePath Target;

    private Node3D _target;

    public override void _Ready()
    {
        base._Ready();

        _target = GetNode<Node3D>(Target);
    }
    
    private static float angle(Vector3 a)
    {
        return MathF.Atan2(a.y, a.x);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var p0 = _target.Position;
        var p1 = Position;
        Rotation = new Vector3(0, 0, angle(p1 - p0));
    }
}