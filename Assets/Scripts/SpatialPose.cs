using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class SpatialPose
{
	public static int a3spatialPoseConvert(a3_SpatialPose spatialPose, a3_SpatialPoseChannel channel, a3_SpatialPoseEulerOrder order)
	{
        Matrix4x4 Rx, Ry, Rz, R;
        Rx.m00 = Mathf.Sin(Mathf.Deg2Rad * spatialPose.rotate.x);
        Ry.m00 = Mathf.Sin(Mathf.Deg2Rad * spatialPose.rotate.y);
        Rz.m00 = Mathf.Sin(Mathf.Deg2Rad * spatialPose.rotate.z);
        R.m00 = Rx.m00 * Ry.m00;
        spatialPose.transformMat.m00 = R.m00 * Rz.m00;
        Matrix4x4 poseMatrix = Matrix4x4.TRS(spatialPose.translate, new Quaternion(spatialPose.rotate.x, spatialPose.rotate.y, spatialPose.rotate.z, spatialPose.rotate.w), spatialPose.scale);
        spatialPose.transformMat = poseMatrix;

        return 1;
		
	}

	public static int a3spatialPoseRestore(a3_SpatialPose spatialPose, a3_SpatialPoseChannel channel, a3_SpatialPoseEulerOrder order)
	{
        spatialPose.translate = spatialPose.transformMat.GetColumn(3);
		spatialPose.rotate = spatialPose.transformMat.rotation.eulerAngles;
		spatialPose.scale = spatialPose.transformMat.lossyScale;
		return 1;
	}

    public static int a3spatialPoseCopy(a3_SpatialPose spatialPose_out, a3_SpatialPose spatialPose_in)
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

}

public struct a3_SpatialPose
{
    public Matrix4x4 transformMat;
    public Quaternion transformDQ;
    public Vector4 rotate;
    public Vector4 scale;
    public Vector4 translate;
    public Vector4 user;
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
