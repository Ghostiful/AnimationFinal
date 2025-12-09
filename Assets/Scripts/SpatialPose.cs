using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class SpatialPose
{
	public static int a3spatialPoseConvert(ref a3_SpatialPose spatialPose, a3_SpatialPoseChannel channel, a3_SpatialPoseEulerOrder order)
	{
		Vector4 temp = Vector4.Normalize(spatialPose.rotate);

        Matrix4x4 poseMatrix = Matrix4x4.TRS(spatialPose.translate, new Quaternion(temp.x, temp.y, temp.z, temp.w), spatialPose.scale);
        spatialPose.transformMat = poseMatrix;

        return 1;
		
	}

	public static int a3spatialPoseRestore(ref a3_SpatialPose spatialPose, a3_SpatialPoseChannel channel, a3_SpatialPoseEulerOrder order)
	{
        spatialPose.translate = spatialPose.transformMat.GetColumn(3);
        //Quaternion rotation = Quaternion.LookRotation(spatialPose.transformMat.GetColumn(2), spatialPose.transformMat.GetColumn(1));
		spatialPose.rotate = spatialPose.transformMat.rotation.eulerAngles;
        //spatialPose.rotate = rotation.eulerAngles;
        spatialPose.rotate.w = 1;
        //Debug.Log(spatialPose.rotate.x + ", " + spatialPose.rotate.y + ", " + spatialPose.rotate.z + ", " + spatialPose.rotate.w);
        spatialPose.scale = new Vector4(1, 1, 1, 1);
		return 1;
	}

    public static int a3spatialPoseCopy(ref a3_SpatialPose spatialPose_out, a3_SpatialPose spatialPose_in)
	{
		spatialPose_out.rotate = spatialPose_in.rotate;
		spatialPose_out.translate = spatialPose_in.translate;
		spatialPose_out.scale = spatialPose_in.scale;
		return 0;
	}

	public static int a3spatialPoseConcat(a3_SpatialPose spatialPose_out, a3_SpatialPose spatialPose_lhs, a3_SpatialPose spatialPose_rhs)
	{
		spatialPose_out.translate = spatialPose_lhs.translate + spatialPose_rhs.translate;
		spatialPose_out.rotate = spatialPose_lhs.rotate + spatialPose_rhs.rotate;
		spatialPose_out.scale = Vector4.Scale(spatialPose_lhs.scale, spatialPose_rhs.scale);
		return 1;
	}

    public static int a3spatialPoseDeconcat(a3_SpatialPose spatialPose_out, a3_SpatialPose spatialPose_lhs, a3_SpatialPose spatialPose_rhs)
    {
		spatialPose_out.translate = spatialPose_lhs.translate - spatialPose_rhs.translate;
		spatialPose_out.rotate = spatialPose_lhs.rotate - spatialPose_rhs.rotate;
        Vector4 inverseScale = new Vector4(1f / spatialPose_rhs.scale.x, 1f / spatialPose_rhs.scale.y, 1f / spatialPose_rhs.scale.z, 1f / spatialPose_rhs.scale.w);
        spatialPose_out.scale = Vector4.Scale(spatialPose_lhs.scale, inverseScale);

        return 1;
    }

    public static int a3spatialPoseLerp(a3_SpatialPose spatialPose_out, a3_SpatialPose spatialPose_0, a3_SpatialPose spatialPose_1, float u)
	{
		spatialPose_out.translate = Vector4.Lerp(spatialPose_0.translate, spatialPose_1.translate, u);
		spatialPose_out.rotate = Vector4.Lerp(spatialPose_0.rotate, spatialPose_1.rotate, u);
		spatialPose_out.scale = Vector4.Lerp(spatialPose_0.scale, spatialPose_1.scale, u);
		return 0;
	}

	//-----------------------------------------------------------------------------

// set rotation values for a single node pose
public static int a3spatialPoseSetRotation(a3_SpatialPose spatialPose, int rx_degrees, int ry_degrees, int rz_degrees)
{
	spatialPose.rotate.x = rx_degrees;
	spatialPose.rotate.y = ry_degrees;
	spatialPose.rotate.z = rz_degrees;
	return 1;
}

// scale
public static int a3spatialPoseSetScale(a3_SpatialPose spatialPose, float sx, float sy, float sz)
{
	spatialPose.scale.x = sx;
	spatialPose.scale.y = sy;
	spatialPose.scale.z = sz;
	return 1;
}

// translation
public static int a3spatialPoseSetTranslation(a3_SpatialPose spatialPose, float tx, float ty, float tz)
{
	spatialPose.translate.x = tx;
	spatialPose.translate.y = ty;
	spatialPose.translate.z = tz;
	return 1;
}

// reset single node pose
public static int a3spatialPoseReset(a3_SpatialPose spatialPose)
{
	spatialPose.transformMat = Matrix4x4.identity;
	spatialPose.transformDQ = Quaternion.identity;
	spatialPose.rotate = Vector4.zero;
	spatialPose.scale = Vector4.one;
	spatialPose.translate = new Vector4(0, 0, 0, 1);
	spatialPose.user = Vector4.zero;
	return 1;
}
}

public class a3_SpatialPose
{
    public Matrix4x4 transformMat;
    public Quaternion transformDQ;
    public Vector4 rotate;
    public Vector4 scale;
    public Vector4 translate;
    public Vector4 user;

    public a3_SpatialPose()
    {
        transformMat = Matrix4x4.identity;
        transformDQ = Quaternion.identity;
        rotate = new Vector4(0, 0, 0, 1);
        scale = Vector4.one;
        translate = new Vector4(0, 0, 0, 1);
        user = Vector4.zero;
    }
}

public enum a3_SpatialPoseChannel
{
    // identity (no channels)
    a3poseChannel_none,

	// rotation
	a3poseChannel_rotate_x = 0x0001,
	a3poseChannel_rotate_y = 0x0002,
	a3poseChannel_rotate_z = 0x0004,
	a3poseChannel_rotate_w = 0x0008,
	a3poseChannel_rotate_xy = a3poseChannel_rotate_x | a3poseChannel_rotate_y,
	a3poseChannel_rotate_yz = a3poseChannel_rotate_y | a3poseChannel_rotate_z,
	a3poseChannel_rotate_zx = a3poseChannel_rotate_z | a3poseChannel_rotate_x,
	a3poseChannel_rotate_xyz = a3poseChannel_rotate_xy | a3poseChannel_rotate_z,

	// scale
	a3poseChannel_scale_x = 0x0010,
	a3poseChannel_scale_y = 0x0020,
	a3poseChannel_scale_z = 0x0040,
	a3poseChannel_scale_w = 0x0080,
	a3poseChannel_scale_xy = a3poseChannel_scale_x | a3poseChannel_scale_y,
	a3poseChannel_scale_yz = a3poseChannel_scale_y | a3poseChannel_scale_z,
	a3poseChannel_scale_zx = a3poseChannel_scale_z | a3poseChannel_scale_x,
	a3poseChannel_scale_xyz = a3poseChannel_scale_xy | a3poseChannel_scale_z,

	// translation
	a3poseChannel_translate_x = 0x0100,
	a3poseChannel_translate_y = 0x0200,
	a3poseChannel_translate_z = 0x0400,
	a3poseChannel_translate_w = 0x0800,
	a3poseChannel_translate_xy = a3poseChannel_translate_x | a3poseChannel_translate_y,
	a3poseChannel_translate_yz = a3poseChannel_translate_y | a3poseChannel_translate_z,
	a3poseChannel_translate_zx = a3poseChannel_translate_z | a3poseChannel_translate_x,
	a3poseChannel_translate_xyz = a3poseChannel_translate_xy | a3poseChannel_translate_z,

	// user channels
	a3poseChannel_user_x = 0x1000,
	a3poseChannel_user_y = 0x2000,
	a3poseChannel_user_z = 0x4000,
	a3poseChannel_user_w = 0x8000,
	a3poseChannel_user_xy = a3poseChannel_user_x | a3poseChannel_user_y,
	a3poseChannel_user_yz = a3poseChannel_user_y | a3poseChannel_user_z,
	a3poseChannel_user_zx = a3poseChannel_user_z | a3poseChannel_user_x,
	a3poseChannel_user_xyz = a3poseChannel_user_xy | a3poseChannel_user_z
}

public enum a3_SpatialPoseEulerOrder
{
    a3poseEulerOrder_xyz,
    a3poseEulerOrder_yzx,
    a3poseEulerOrder_zxy,
    a3poseEulerOrder_yxz,
    a3poseEulerOrder_xzy,
    a3poseEulerOrder_zyx
}

public enum a3_BasisAxis
{
    basis_xp = 0x00,
    basis_yp = 0x01,
    basis_zp = 0x02,
    basis_xn = 0x10,
    basis_yn = 0x11,
    basis_zn = 0x12,
    basis_invalid = 0xFF
}

public struct a3_Basis
{
    public ushort value;

    // Invalid basis constant
    public static readonly a3_Basis Invalid = new a3_Basis { value = 0xFFFF };

    public a3_Basis(ushort val)
    {
        value = val;
    }

    // Implicit conversions
    public static implicit operator a3_Basis(ushort v) => new a3_Basis(v);
    public static implicit operator ushort(a3_Basis basis) => basis.value;

    public override string ToString() => $"Basis(0x{value:X4})";
}

public static class BasisUtil
{
    public static a3_Basis a3basisInit(a3_BasisAxis fwd, a3_BasisAxis up)
    {
        // Validate axes
        if (!a3basisAxisValid(fwd) || !a3basisAxisValid(up))
            return a3_Basis.Invalid;

        if (a3basisAxisIndex(fwd) == a3basisAxisIndex(up))
            return a3_Basis.Invalid;

        // Pack forward in lower 8 bits, up in upper 8 bits
        ushort result = (ushort)(((byte)fwd & 0xFF) | (((byte)up & 0xFF) << 8));
        return new a3_Basis(result);
    }

    public static bool a3basisAxisValid(a3_BasisAxis axis)
    {
        byte axisValue = (byte)axis;

        if (axisValue == 0x00 || axisValue == 0x01 || axisValue == 0x02)
            return true; // Positive X, Y, or Z

        if (axisValue == 0x10 || axisValue == 0x11 || axisValue == 0x12)
            return true; // Negative X, Y, or Z

        return false;
    }

    public static int a3basisAxisIndex(a3_BasisAxis axis)
    {
        return (byte)axis & 0x0F;
    }

    public static Matrix4x4 BasisToMatrix4x4(a3_Basis basis)
    {
        if (basis.value == a3_Basis.Invalid.value)
        {
            Debug.LogWarning("Cannot convert invalid basis to matrix");
            return Matrix4x4.identity;
        }

        // Get the three basis vectors
        Vector3 right = GetRightVector(basis);
        Vector3 up = GetUpVector(basis);
        Vector3 forward = GetForwardVector(basis);

        // Create matrix with basis vectors as columns
        Matrix4x4 matrix = new Matrix4x4();

        // Column 0: Right vector
        matrix.m00 = right.x;
        matrix.m10 = right.y;
        matrix.m20 = right.z;
        matrix.m30 = 0f;

        // Column 1: Up vector
        matrix.m01 = up.x;
        matrix.m11 = up.y;
        matrix.m21 = up.z;
        matrix.m31 = 0f;

        // Column 2: Forward vector
        matrix.m02 = forward.x;
        matrix.m12 = forward.y;
        matrix.m22 = forward.z;
        matrix.m32 = 0f;

        // Column 3: Translation (identity - no translation)
        matrix.m03 = 0f;
        matrix.m13 = 0f;
        matrix.m23 = 0f;
        matrix.m33 = 1f;

        return matrix;
    }

    public static bool a3basisAxisIsNegative(a3_BasisAxis axis)
    {
        return ((byte)axis & 0xF0) == 0x10;
    }

    public static bool a3basisAxisIsPositive(a3_BasisAxis axis)
    {
        return ((byte)axis & 0xF0) == 0x00;
    }

    public static a3_BasisAxis a3basisGetForward(a3_Basis basis)
    {
        return (a3_BasisAxis)(basis.value & 0xFF);
    }

    public static a3_BasisAxis a3basisGetUp(a3_Basis basis)
    {
        return (a3_BasisAxis)((basis.value >> 8) & 0xFF);
    }

    private static bool DetermineCrossProductSign(int fwdIdx, bool fwdNeg, int upIdx, bool upNeg)
    {
        // Calculate base sign from indices
        bool basePositive = false;

        if ((fwdIdx == 0 && upIdx == 1) || // X × Y = Z
            (fwdIdx == 1 && upIdx == 2) || // Y × Z = X
            (fwdIdx == 2 && upIdx == 0))   // Z × X = Y
        {
            basePositive = true;
        }
        else if ((fwdIdx == 1 && upIdx == 0) || // Y × X = -Z
                 (fwdIdx == 2 && upIdx == 1) || // Z × Y = -X
                 (fwdIdx == 0 && upIdx == 2))   // X × Z = -Y
        {
            basePositive = false;
        }

        // XOR with negations (negative axis flips the result)
        return basePositive ^ fwdNeg ^ upNeg;
    }

    public static a3_BasisAxis a3basisGetRight(a3_Basis basis)
    {
        a3_BasisAxis fwd = a3basisGetForward(basis);
        a3_BasisAxis up = a3basisGetUp(basis);

        int fwdIdx = a3basisAxisIndex(fwd);
        int upIdx = a3basisAxisIndex(up);

        // Calculate third axis index (the remaining axis)
        // X=0, Y=1, Z=2, so 3 - fwdIdx - upIdx gives the third
        int rightIdx = 3 - fwdIdx - upIdx;

        // Determine sign based on right-hand rule
        bool fwdNeg = a3basisAxisIsNegative(fwd);
        bool upNeg = a3basisAxisIsNegative(up);

        // Right = Forward × Up
        bool rightNeg = DetermineCrossProductSign(fwdIdx, fwdNeg, upIdx, upNeg);

        // Build the axis value
        byte rightAxis = (byte)rightIdx;
        if (rightNeg)
            rightAxis |= 0x10;  // Set negative flag

        return (a3_BasisAxis)rightAxis;
    }

    public static Vector3 AxisToVector(a3_BasisAxis axis)
    {
        int index = a3basisAxisIndex(axis);
        bool negative = a3basisAxisIsNegative(axis);
        float sign = negative ? -1f : 1f;

        switch (index)
        {
            case 0: return new Vector3(sign, 0, 0); // X
            case 1: return new Vector3(0, sign, 0); // Y
            case 2: return new Vector3(0, 0, sign); // Z
            default: return Vector3.zero;
        }
    }

    public static Vector3 GetForwardVector(a3_Basis basis)
    {
        return AxisToVector(a3basisGetForward(basis));
    }

    public static Vector3 GetUpVector(a3_Basis basis)
    {
        return AxisToVector(a3basisGetUp(basis));
    }

    public static Vector3 GetRightVector(a3_Basis basis)
    {
        return AxisToVector(a3basisGetRight(basis));
    }
}
