using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class SpatialPose
{
	public static int a3spatialPoseConvert(ref a3_SpatialPose spatialPose, a3_SpatialPoseChannel channel, a3_SpatialPoseEulerOrder order)
	{
        //Matrix4x4 Rx, Ry, Rz, R;
        //Rx = new Matrix4x4();
        //Ry = new Matrix4x4();
        //Rz = new Matrix4x4();
        //R = new Matrix4x4();
        //Matrix4x4Extensions.SetRotateX(ref Rx, spatialPose.rotate.x);
        //Matrix4x4Extensions.SetRotateY(ref Ry, spatialPose.rotate.y);
        //Matrix4x4Extensions.SetRotateZ(ref Rz, spatialPose.rotate.z);
        //R = Rx * Ry;
        //spatialPose.transformMat = R * Rz;
        //spatialPose.transformMat.SetColumn(3, spatialPose.translate);
		Vector4 temp = Vector4.Normalize(spatialPose.rotate);

        Matrix4x4 poseMatrix = Matrix4x4.TRS(spatialPose.translate, new Quaternion(temp.x, temp.y, temp.z, temp.w), spatialPose.scale);
        spatialPose.transformMat = poseMatrix;

        return 1;
		
	}

	public static int a3spatialPoseRestore(ref a3_SpatialPose spatialPose, a3_SpatialPoseChannel channel, a3_SpatialPoseEulerOrder order)
	{
        spatialPose.translate = spatialPose.transformMat.GetColumn(3);
        Quaternion rotation = Quaternion.LookRotation(spatialPose.transformMat.GetColumn(2), spatialPose.transformMat.GetColumn(1));
		spatialPose.rotate = spatialPose.transformMat.rotation.eulerAngles;
        //spatialPose.rotate = rotation.eulerAngles;
        spatialPose.scale = new Vector4(1, 1, 1);
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
        rotate = Vector4.zero;
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


public static class Matrix4x4Extensions
{
    /// <summary>
    /// Set a Matrix4x4 to represent a rotation around the X axis
    /// Unity uses column-major matrices like OpenGL
    /// </summary>
    public static Matrix4x4 SetRotateX(float degrees)
    {
        Matrix4x4 m = new Matrix4x4();

        float c = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float s = Mathf.Sin(degrees * Mathf.Deg2Rad);

        // Set to identity first
        m.m00 = 1f;
        m.m11 = c;
        m.m22 = c;
        m.m33 = 1f;

        // Set zeros
        m.m01 = m.m02 = m.m03 = 0f;
        m.m10 = m.m13 = 0f;
        m.m20 = m.m23 = 0f;
        m.m30 = m.m31 = m.m32 = 0f;

        // Unity uses column-major (like OpenGL, not row-major)
        // Column-major layout:
        m.m12 = -s;  // [row 1, col 2]
        m.m21 = s;   // [row 2, col 1]

        return m;
    }

    /// <summary>
    /// Set an existing Matrix4x4 to represent a rotation around the X axis
    /// </summary>
    public static void SetRotateX(ref Matrix4x4 m_out, float degrees)
    {
        float c = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float s = Mathf.Sin(degrees * Mathf.Deg2Rad);

        // Set diagonal
        m_out.m00 = 1f;
        m_out.m11 = c;
        m_out.m22 = c;
        m_out.m33 = 1f;

        // Set zeros
        m_out.m01 = m_out.m02 = m_out.m03 = 0f;
        m_out.m10 = m_out.m13 = 0f;
        m_out.m20 = m_out.m23 = 0f;
        m_out.m30 = m_out.m31 = m_out.m32 = 0f;

        // Set rotation components (column-major)
        m_out.m12 = -s;
        m_out.m21 = s;
    }

    /// <summary>
    /// Set a Matrix4x4 to represent a rotation around the Y axis
    /// </summary>
    public static Matrix4x4 SetRotateY(ref Matrix4x4 m, float degrees)
    {

        float c = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float s = Mathf.Sin(degrees * Mathf.Deg2Rad);

        m.m11 = 1f;
        m.m00 = c;
        m.m22 = c;
        m.m33 = 1f;

        m.m01 = m.m03 = 0f;
        m.m10 = m.m12 = m.m13 = 0f;
        m.m21 = m.m23 = 0f;
        m.m30 = m.m31 = m.m32 = 0f;

        // Column-major
        m.m02 = s;
        m.m20 = -s;

        return m;
    }

    /// <summary>
    /// Set a Matrix4x4 to represent a rotation around the Z axis
    /// </summary>
    public static Matrix4x4 SetRotateZ(ref Matrix4x4 m, float degrees)
    {

        float c = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float s = Mathf.Sin(degrees * Mathf.Deg2Rad);

        m.m22 = 1f;
        m.m00 = c;
        m.m11 = c;
        m.m33 = 1f;

        m.m02 = m.m03 = 0f;
        m.m12 = m.m13 = 0f;
        m.m20 = m.m21 = m.m23 = 0f;
        m.m30 = m.m31 = m.m32 = 0f;

        // Column-major
        m.m01 = -s;
        m.m10 = s;

        return m;
    }
}
