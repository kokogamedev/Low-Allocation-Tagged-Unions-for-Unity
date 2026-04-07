using UnityEngine;
using PsigenVision.TaggedUnion;

[CreateAssetMenu(fileName = "ExampleTaggedUnionSO", menuName = "Scriptable Objects/ExampleTaggedUnionSO")]
public class ExampleTaggedUnionSO : ScriptableObject
{
	[SerializeField] public AnyPrimitive anyPrimitive;
	public AnyPrimitiveLiteral anyPrimLit;
    public AnyValue anyValue;
	public AnyLiteral anyLiteral;
	public AnyAnimatorParamValue anyAnimatorParamValue;
	public AnyAnimatorParamLiteral anyAnimatorParamLiteral;
	public AnyRange anyRange;
}
