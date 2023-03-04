using Godot;
using Godot.Collections;

namespace PataNext.models.scripts;

[Tool]
public partial class Spline : MeshInstance3D
{
    [Export] public Array<NodePath> Points;
    [Export] public bool ModifyFinalRotation;

    private readonly StringName smn_Points = "Points";

    private Node3D[] _points;
    private Vector3[] _previousPositions;

    public override void _Ready()
    {
        base._Ready();

        _points = new Node3D[Points.Count];
        _previousPositions = new Vector3[Points.Count];
        for (var i = 0; i < Points.Count; i++)
            _points[i] = GetNode<Node3D>(Points[i]);
    }
    
    private static Vector3 bzPos(float t, Vector3 a, Vector3 b, Vector3 c)
    {
        var mT = 1 - t;
        return (a * mT * mT +
                b * 2f * t * mT +
                c * t * t);
    }
    
    private static float angle(Vector3 a)
    {
        return MathF.Atan2(a.y, a.x);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (_points.Length != 3)
            return;

        if (GetActiveMaterial(0) is not ShaderMaterial shaderMaterial)
            return;
        
        var globalPosition = GlobalPosition;

        bool tryDirty(ref Vector3 old, Vector3 n)
        {
            if (old == n) return false;

            old = n;
            return true;
        }
        Vector3 pos(Node3D node) => node.GlobalPosition - globalPosition;

        // | not || (we don't want to short circuit)
        var dirty = tryDirty(ref _previousPositions[0], pos(_points[0]))
            | tryDirty(ref _previousPositions[1], pos(_points[1]))
            | tryDirty(ref _previousPositions[2], pos(_points[2]));

        if (!dirty)
            return;

        var correctedMiddle = (4f * _previousPositions[1] - _previousPositions[0] - _previousPositions[2]) / 2f;
        
        shaderMaterial.SetShaderParameter(smn_Points, Variant.CreateFrom(stackalloc Vector3[3]
        {
            _previousPositions[0],
            correctedMiddle,
            _previousPositions[2],
        }));

        if (ModifyFinalRotation)
        {
            var p0 = bzPos(0.9f, _previousPositions[0], correctedMiddle, _previousPositions[2]);
            var p1 = bzPos(1.0f, _previousPositions[0], correctedMiddle, _previousPositions[2]);
            
            _points[2].Rotation = new Vector3(0, 0, angle(p1 - p0));
        }
    }
}