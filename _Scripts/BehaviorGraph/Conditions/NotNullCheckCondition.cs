using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(
    name: "Not Null Check", 
    story: "[Variable] is not null", 
    category: "Variable Conditions", 
    description: "Not null check a reference type. Return false if a value type is provided.", 
    id: "3b9f93e695c4b78fff341870c851813d")]
public partial class NotNullCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable Variable;

    public override bool IsTrue()
    {
        if (Variable.Type.IsValueType)
        {
            return false;
        }

        return Variable.ObjectValue is not null && !Variable.ObjectValue.Equals(null);
    }
}
