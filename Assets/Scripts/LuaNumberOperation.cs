// MIT License
using System;
using Buck;
using Lua;
using UnityEngine;

[Serializable]
public class LuaNumberOperation : BaseOperation
{
    /// <summary>
    /// Constructor without RightHandArithmetic
    /// </summary>
    public LuaNumberOperation(string expression, NumberVariable numberA, NumberReference numberB, NumberReference numberC, NumberOperation.RoundingType rounding, BoolReference raiseEvent)
    {
        m_expression = expression;
        m_numberA = numberA;
        m_numberB = numberB;
        m_numberC = numberC;
        m_raiseEvent = raiseEvent;
    }
        
    /// <summary>
    /// Parameterless constructor
    /// </summary>
    public LuaNumberOperation()
    { }

    [Tooltip("The lua expression to execute.")]
    [SerializeField] string m_expression;
    public string Expression => m_expression;

    [Tooltip("The NumberVariable that this operation acts on. Supports IntVariables, FloatVariables, or DoubleVariables")]
    [SerializeField] NumberVariable m_numberA;
    public NumberVariable NumberA => m_numberA;
        
    [Tooltip("First NumberReference used in the operation. Supports a constant Float, IntVariables, FloatVariables, or DoubleVariables.")]
    [SerializeField] NumberReference m_numberB;
    public NumberReference NumberB => m_numberB;

    [Tooltip("Second NumberReference possibly used in the operation. Supports a constant Float, IntVariables, FloatVariables, or DoubleVariables.")]
    [SerializeField] NumberReference m_numberC;
    public NumberReference NumberC => m_numberC;
        
    [Tooltip("These rounding operations are only available if NumberA is an IntVariable. They match Unity.Mathf operations of the same name. They are applied as the final step after the operation is calculated as floats.")]
    [SerializeField] NumberOperation.RoundingType m_rounding = NumberOperation.RoundingType.RoundToInt;
    public NumberOperation.RoundingType Rounding => m_rounding;

    [Tooltip("If true, when Execute() is called NumberA's event will be raised.")]
    [SerializeField] BoolReference m_raiseEvent;
    public BoolReference RaiseEvent => m_raiseEvent;

    [SerializeField, HideInInspector] bool m_serialized = false;

    public override bool Serialized
    {
        get => m_serialized;
        set => m_serialized = value;
    }

    public override void Execute()
    {
        switch (m_numberA.TypeCode)
        {
            case TypeCode.Int32:
                IntVariable intA = (IntVariable)m_numberA;
                intA.Value = GetIntResult();

                break;

                
            case TypeCode.Single:
                FloatVariable floatA = (FloatVariable)m_numberA;
                floatA.Value = GetFloatResult();

                break;

                
            case TypeCode.Double:
                DoubleVariable doubleA = (DoubleVariable)m_numberA;
                doubleA.Value = GetDoubleResult();
                break;
        }

        if (m_raiseEvent)
            m_numberA.Raise();
    }
        
    public int GetIntResult()
    {
        float floatResult = GetFloatResult();

        switch (m_rounding)
        {
            default:
            case NumberOperation.RoundingType.RoundToInt:
                return Mathf.RoundToInt(floatResult);

            case NumberOperation.RoundingType.FloorToInt:
                return Mathf.FloorToInt(floatResult);

            case NumberOperation.RoundingType.CeilToInt:
                return Mathf.CeilToInt(floatResult);
        }
    }
        
    public float GetFloatResult()
    {
        return (float)GetDoubleResult();
    }

    public double GetDoubleResult()
    {
        var state = LuaState.Create();
        state.Environment["nA"] = m_numberA.ValueDouble;
        state.Environment["nB"] = m_numberB.ValueDouble;
        state.Environment["nC"] = m_numberC.ValueDouble;

        var result = state.DoStringAsync(@"return (" + m_expression + ")").Result[0];
            
        if(result.Type != LuaValueType.Number)
        {
            throw new InvalidOperationException($"Lua expression '{m_expression}' did not return a number. Returned type: {result.Type}");
        }

        return result.Read<double>();
    }
        
    public override void OnBeforeSerialize()
    {

    }

    public override void OnAfterDeserialize()
    {
        if (!Serialized)
        {
            m_raiseEvent = new BoolReference(true);
            Serialized = true;
        }
    }
#if UNITY_INCLUDE_TESTS
    public void SetValues(NumberVariable numberA, NumberReference numberB, NumberReference numberC, string expression, NumberOperation.RoundingType rounding)
    {
        m_numberA = numberA;
        m_numberB = numberB;
        m_numberC = numberC;
        m_expression = expression;
        m_rounding = rounding;
        m_raiseEvent = new BoolReference(false);
    }
#endif
}