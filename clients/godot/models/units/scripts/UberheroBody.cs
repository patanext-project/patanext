using Godot;
using PataNext.models.scripts;

namespace PataNext.Presentations;

[Tool]
public partial class UberheroBody : Spline
{
    [Export] public NodePath NeckNodePath;

    private Node3D _neck;
    
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_neck == null)
        {
            _neck = GetNode<Node3D>(NeckNodePath);
            if (_neck == null)
                return;
        }

        if (GetActiveMaterial(0) is not ShaderMaterial shaderMaterial)
            return;
        
        var baseDir = _neck.GlobalRotation;

        shaderMaterial.SetShaderParameter("body_direction", Variant.CreateFrom(new Vector2(
            -baseDir.y / Mathf.Pi * 2, baseDir.x / Mathf.Pi * 1.1f
        )));
    }
}