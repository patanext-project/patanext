using Godot;

namespace PataNext.Presentations;

[Tool]
public partial class UberheroEye : MeshInstance3D
{
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (GetActiveMaterial(0) is not ShaderMaterial shaderMaterial)
            return;

        var baseDir = GetParentNode3d().GlobalRotation;
        var eyeDir = GlobalRotation;

        shaderMaterial.SetShaderParameter("body_direction", Variant.CreateFrom(new Vector2(
            -baseDir.y / Mathf.Pi * 2, -baseDir.x / Mathf.Pi * 2
        )));
        shaderMaterial.SetShaderParameter("eye_direction", Variant.CreateFrom(new Vector2(
            -eyeDir.y / Mathf.Pi * 2, -eyeDir.x / Mathf.Pi * 2
        )));
    }
}