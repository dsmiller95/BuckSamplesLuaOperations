using System;
using System.Collections.Generic;
using Lua;
using UnityEngine;
using static Buck.Condition;

namespace Buck.Samples
{
    public static class ConditionExtensionMethods
    {        
        /// <summary>
        /// Loops through a ICollection of Conditions and returns true if they all pass, false if any of them fail.
        /// </summary>
        public static bool PassConditions(this ICollection<LuaCondition> conditions)
        {
            if (conditions == null)
                return true;

            foreach(LuaCondition c in conditions)
                if (!c.PassCondition)
                    return false;

            return true;
        }

        /// <summary>
        /// Loops through a array of Conditions and returns true if they all pass, false if any of them fail.
        /// </summary>
        public static bool PassConditions(this LuaCondition[] conditions)
        {
            if (conditions == null)
                return true;
            
            foreach(LuaCondition c in conditions)
                if (!c.PassCondition)
                    return false;

            return true;
        }
    }
    
    /// <summary>
    /// A serializable class that can be used to define basic boolean conditional logic comparing two variables. 
    /// Supports Int, Bool, Float, and Vector3. Uses Buck Basic VariableReferences to support optionally referencing Buck Basics Scriptable Object Variables.
    /// See script for usage examples./// 
    /// </summary>
    [Serializable]
    public class LuaCondition
    {
        /// <summary>
        /// Constructor for Booleans
        /// </summary>
        public LuaCondition(BoolVariable boolA, string expression, BoolVariable boolB)
        {
            m_boolA = boolA;
            m_boolB = boolB;
        }
        
        /// <summary>
        /// Constructor for Numbers
        /// </summary>
        public LuaCondition(NumberVariable numberA, string expression, NumberVariable numberB)
        {
            m_numberA = numberA;
            m_numberB = numberB;
        }
        
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public LuaCondition()
        { }

        [Tooltip("The lua expression to execute.")]
        [SerializeField] string m_expression;
        public string Expression => m_expression;
        
        [Tooltip("The left side Bool in condition boolean logic.")]
        [SerializeField] BoolVariable m_boolA;
        public BoolVariable BoolA => m_boolA;
        
        [Tooltip("The right side Bool in condition boolean logic.")]
        [SerializeField] BoolVariable m_boolB;
        public BoolVariable BoolB => m_boolB;
        
        [Tooltip("The left side NumberReference in condition boolean logic. Supports a constant Float, IntVariables, FloatVariables, or DoubleVariables.")]
        [SerializeField] NumberVariable m_numberA;
        public NumberVariable NumberA => m_numberA;

        [Tooltip("The right side NumberReference in condition boolean logic. Supports a constant Float, IntVariables, FloatVariables, or DoubleVariables.")]
        [SerializeField] NumberVariable m_numberB;
        public NumberVariable NumberB => m_numberB;
    
        /// <value>Checks if the defined condition results in true or false.</value>
        public bool PassCondition
        {
            get
            {
                var state = LuaState.Create();
                state.Environment["nA"] = m_numberA?.ValueDouble ?? 0;
                state.Environment["nB"] = m_numberB?.ValueDouble ?? 0;
                state.Environment["bA"] = m_boolA?.Value ?? false;
                state.Environment["bB"] = m_boolB?.Value ?? false;
                
                // Execute the expression and return the result
                var result = state.DoStringAsync(@"return (" + m_expression + ")").Result[0];
                if(result.Type != LuaValueType.Boolean)
                {
                    Debug.LogWarning($"Lua expression '{m_expression}' did not return a number. Returned type: {result.Type}");
                }

                return result.ToBoolean();
            }
        }

        TypeCode HighestPrecision(TypeCode typeA, TypeCode typeB)
        {
            if (typeA == TypeCode.Double || typeB == TypeCode.Double)
                return TypeCode.Double;

            if (typeA == TypeCode.Single || typeB == TypeCode.Single)
                return TypeCode.Single;
                
            return TypeCode.Int32;
        }

        int HighestVectorPrecision(int lengthA, bool isAVectorIntA, int lengthB, bool isAVectorIntB, out bool isAVectorInt)
        {
            isAVectorInt = isAVectorIntA && isAVectorIntB; // Only use VectorInt if BOTH vectors are a vector int
            return Mathf.Max(lengthA, lengthB); // Return the longest length o the two vectors.
        }
        
#if UNITY_INCLUDE_TESTS
        public void SetValues(BoolVariable boolA, BoolVariable boolB, string expression)
        {
            m_boolA = boolA;
            m_boolB = boolB;
            m_expression = expression;
        }
        
        public void SetValues(NumberVariable numberA, NumberVariable numberB, string expression)
        {
            m_numberA = numberA;
            m_numberB = numberB;
            m_expression = expression;
        }
#endif
    }
}