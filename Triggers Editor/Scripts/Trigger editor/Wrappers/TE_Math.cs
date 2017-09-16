using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Math
    {
        public enum ScalarOperation
        {
            Add,
            Substract,
            Multiply,
            Divide
        }

        [NodeMethod("Math", "Real operation", NodeMethodType.Other)]
        static public float RealOp(ScalarOperation operation, float a, float b)
        {
            switch(operation)
            {
                case ScalarOperation.Add: return a + b;
                case ScalarOperation.Substract: return a - b;
                case ScalarOperation.Multiply: return a * b;
                case ScalarOperation.Divide: return a / b;
            }

            throw new System.Exception("Unknow math operation");
        }

        [NodeMethod("Math", "Integer operation", NodeMethodType.Other)]
        static public int IntegerOp(ScalarOperation operation, int a, int b)
        {
            switch (operation)
            {
                case ScalarOperation.Add: return a + b;
                case ScalarOperation.Substract: return a - b;
                case ScalarOperation.Multiply: return a * b;
                case ScalarOperation.Divide: return a / b;
            }

            throw new System.Exception("Unknow math operation");
        }

        public enum VV2VOperation3 // Vector°Vector to Vector 3
        {
            Add,
            Substract,
            CrossProduct
        }

        [NodeMethod("Math", "Vector3 operation", NodeMethodType.Other)]
        static public Vector3 VectorOp(VV2VOperation3 operation, Vector3 a, Vector3 b)
        {
            switch (operation)
            {
                case VV2VOperation3.Add: return a + b;
                case VV2VOperation3.Substract: return a - b;
                case VV2VOperation3.CrossProduct: return Vector3.Cross(a, b);
            }

            throw new System.Exception("Unknow math operation");
        }

        [NodeMethod("Convert", "Vector3 to XYZ", NodeMethodType.Other, "X")]
        static public float Vector3ToXYZ(Vector3 vector, out float Y, out float Z)
        {
            Y = vector.y;
            Z = vector.z;
            return vector.x;
        }

        [NodeMethod("Convert", "XYZ to Vector3", NodeMethodType.Other, "Vector3")]
        static public Vector3 XYZtoVector3(float X, float Y, float Z)
        {
            return new Vector3(X, Y, Z);
        }
    }
}
