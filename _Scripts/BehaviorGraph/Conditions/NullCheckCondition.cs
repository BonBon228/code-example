using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "Null Check", 
    story: "[Variable] is null", 
    category: "Variable Conditions", 
    description: "Null check a reference type. Return false if a value type is provided.", 
    id: "99c5e527ecd765e15037d4e7bd15b35c")]
public partial class NullCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable Variable;

    public override bool IsTrue()
    {
        if (Variable.Type.IsValueType)
        {
            return false;
        }

        return Variable.ObjectValue is null || Variable.ObjectValue.Equals(null);
    }
}
