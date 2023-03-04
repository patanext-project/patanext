using System.Runtime.InteropServices;
using Godot;

namespace PataNext.models.scripts;

public partial class AnimationFloatNode : AnimationNodeSync
{
    private record struct WeightedNode(StringName Name, int Index, float Weight, bool IsCreated);

    private List<WeightedNode> _nodes = new();
    private Dictionary<int, int> _mappedIndexToListIndex = new();

    public AnimationFloatNode(bool normalized)
    {
        GD.Print("created!");
        IsNormalized = normalized;
    }

    public int AddWeightedInput(float defaultWeight = 0)
    {
        var index = _nodes.Count;
        var name = (StringName) $"anim{index}";
        _nodes.Add(new WeightedNode(name, index, defaultWeight, true));
        _mappedIndexToListIndex[index] = index;
        
        AddInput(name);

        return index;
    }

    public void RemoveWeightedInput(int nodeIndex)
    {
        if (_mappedIndexToListIndex.TryGetValue(nodeIndex, out var listIndex))
        {
            ref var node = ref CollectionsMarshal.AsSpan(_nodes)[listIndex];
            node.IsCreated = false;
            node.Weight = 0;
        }
        else
            GD.PushWarning($"The node index {nodeIndex} was already removed!");

        _mappedIndexToListIndex.Remove(nodeIndex);
    }

    private ref WeightedNode GetWeightedNode(int nodeIndex) =>
        ref CollectionsMarshal.AsSpan(_nodes)[_mappedIndexToListIndex[nodeIndex]];

    public void SetWeight(int nodeIndex, float weight)
    {
        ref var node = ref GetWeightedNode(nodeIndex);
        node.Weight = weight;
    }

    public bool IsNormalized { get; set; }

    public override double _Process(double time, bool seek, bool seekRoot)
    {
        var span = CollectionsMarshal.AsSpan(_nodes);
        var sumFactor = 1.0f;
        
        // from https://stackoverflow.com/a/26785464
        if (IsNormalized)
        {
            var sum = 0.0f;
            foreach (var node in span)
            {
                if (!node.IsCreated)
                    continue;
                
                sum += node.Weight;
            }

            sumFactor = 1 / sum;
        }
        
        GD.Print($"time: {time:F2} seek:{seek} seekRoot:{seekRoot}");

        var length = 0.0;
        for (var i = 0; i < span.Length; i++)
        {
            ref var node = ref span[i];
            if (!node.IsCreated || node.Weight < float.Epsilon)
                continue;

            length = Math.Max(length, BlendInput(i, time, seek, seekRoot, node.Weight * sumFactor));
            GD.Print($"{i}={node.Weight * sumFactor} ({node.Weight}; {sumFactor})");
        }

        return length;
    }

    public override Variant _GetParameterDefaultValue(StringName parameter)
    {
        return 0;
    }

    public override string _GetCaption()
    {
        return "BlendFloat";
    }
}