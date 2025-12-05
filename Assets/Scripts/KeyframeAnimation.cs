using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.PackageManager.UI;
using UnityEngine;

[System.Serializable]
public struct a3_Sample
{
    public int index;
    public int time_step;
    public double time_sec;
}

[System.Serializable]
public struct a3_Keyframe
{
    // index in keyframe pool
    public int index;

    // sample indices in pool
    public int sampleIndex0;
    public int sampleIndex1;

    // duration in steps
    public int duration_step;

    // duration in seconds and reciprocal
    public double duration_sec;
    public double durationInv;
}

public enum a3_ClipTransitionFlag
{
    a3clip_stopFlag = 0x00,
    a3clip_playFlag = 0x01,
    a3clip_reverseFlag = 0x02,
    a3clip_skipFlag = 0x04,
    a3clip_overstepFlag = 0x08,
    a3clip_terminusFlag = 0x10,
    a3clip_offsetFlag = 0x20,
    a3clip_clipFlag = 0x40,
    a3clip_branchFlag = 0x80,
}

[System.Serializable]
public struct a3_ClipTransition
{
    public a3_ClipTransitionFlag flag;
    public int offset;
    public int clipIndex;
}

[System.Serializable]
public class a3_Clip
{
    // clip name
    public string name;

    // index in clip pool
    public int index;

    // keyframe indices, count and step direction
    public int keyframeIndex_first;
    public int keyframeIndex_final;
    public int keyframeCount;
    public int keyframeDirection;

    // duration in steps
    public int duration_step;

    // duration in seconds and reciprocal
    public double duration_sec;
    public double durationInv;

    // transitions
    public a3_ClipTransition transitionForward;
    public a3_ClipTransition transitionReverse;
}

[System.Serializable]
public class a3_ClipPool
{
    // array of clips
    public a3_Clip[] clip;

    // array of keyframes
    public a3_Keyframe[] keyframe;

    // array of samples
    public a3_Sample[] sample;

    // counts
    public int clipCount;
    public int keyframeCount;
    public int sampleCount;
}

public class a3_KeyframeAnimation : MonoBehaviour
{
    public static int a3sampleInit(ref a3_Sample sample_out, int time_step, double playback_stepPerSec)
    {
        if (sample_out.index >= 0 && playback_stepPerSec > 0.0)
        {
            sample_out.time_step = time_step;
            sample_out.time_sec = (double)time_step / playback_stepPerSec;
            return sample_out.index;
        }
        return -1;
    }

    public static int a3keyframeInit(ref a3_Keyframe keyframe_out, a3_Sample sample0, a3_Sample sample1, double playback_stepPerSec)
    {
        if (keyframe_out.index >= 0 && sample0.index >= 0 && sample1.index >= 0 && playback_stepPerSec > 0.0)
        {
            keyframe_out.sampleIndex0 = sample0.index;
            keyframe_out.sampleIndex1 = sample1.index;
            keyframe_out.duration_step = sample1.time_step - sample0.time_step;
            keyframe_out.duration_sec = (double)keyframe_out.duration_step / playback_stepPerSec;
            keyframe_out.durationInv = 1.0f / keyframe_out.duration_sec;
            return keyframe_out.index;
        }
        return -1;
    }

    public static int a3clipPoolCreate(a3_ClipPool clipPool_out, int clipCount, int keyframeCount, int sampleCount)
    {
        if (clipPool_out != null && clipPool_out.clip == null && clipCount > 0 && keyframeCount > 0 && sampleCount > 0)
        {
            int i;

            clipPool_out.clip = new a3_Clip[clipCount];
            clipPool_out.keyframe = new a3_Keyframe[keyframeCount];
            clipPool_out.sample = new a3_Sample[sampleCount];

            // Initialize clip indices
            for (i = 0, clipPool_out.clipCount = clipCount; i < clipCount; ++i)
            {
                clipPool_out.clip[i] = new a3_Clip();
                clipPool_out.clip[i].index = i;
            }

            // Initialize keyframe indices
            for (i = 0, clipPool_out.keyframeCount = keyframeCount; i < keyframeCount; ++i)
            {
                clipPool_out.keyframe[i].index = i;
            }

            // Initialize sample indices
            for (i = 0, clipPool_out.sampleCount = sampleCount; i < sampleCount; ++i)
            {
                clipPool_out.sample[i].index = i;
            }

            return clipCount;
        }
        return -1;
    }

    public static int a3clipPoolRelease(a3_ClipPool clipPool)
    {
        if (clipPool != null && clipPool.clip != null)
        {
            int clipCount = clipPool.clipCount;
            clipPool.clip = null;
            clipPool.keyframe = null;
            clipPool.sample = null;
            clipPool.clipCount = 0;
            clipPool.keyframeCount = 0;
            clipPool.sampleCount = 0;
            return clipCount;
        }
        return -1;
    }

    public static int a3clipTransitionInit(ref a3_ClipTransition transition, a3_ClipTransitionFlag transitionFlag, int offset, a3_Clip clip)
    {
        if (clip != null)
        {
            transition.flag = transitionFlag;
            transition.offset = offset;
            transition.clipIndex = clip.index;
            return (int)transitionFlag;
        }
        return -1;
    }

    public static int a3clipInit(a3_Clip clip_out, string clipName, a3_Keyframe keyframe_first, a3_Keyframe keyframe_final)
    {
        if (clip_out != null && clip_out.index >= 0 && keyframe_first.index >= 0 && keyframe_final.index >= 0)
        {
            string searchName = (!string.IsNullOrEmpty(clipName)) ? clipName : "unamed clip";
            clip_out.name = searchName;

            clip_out.keyframeIndex_first = keyframe_first.index;
            clip_out.keyframeIndex_final = keyframe_final.index;
            clip_out.keyframeCount = clip_out.keyframeIndex_final - clip_out.keyframeIndex_first;
            clip_out.keyframeDirection = clip_out.keyframeCount != 0 ? (clip_out.keyframeCount > 0 ? +1 : -1) : 0;
            clip_out.keyframeCount = 1 + clip_out.keyframeCount * clip_out.keyframeDirection;

            a3clipTransitionInit(ref clip_out.transitionForward, a3_ClipTransitionFlag.a3clip_stopFlag, 0, clip_out);
            a3clipTransitionInit(ref clip_out.transitionReverse, a3_ClipTransitionFlag.a3clip_stopFlag, 0, clip_out);

            return clip_out.index;
        }
        return -1;
    }

    public static int a3clipGetIndexInPool(a3_ClipPool clipPool, string clipName)
    {
        if (clipPool != null && clipPool.clip != null)
        {
            int i;
            for (i = 0; i < clipPool.clipCount; ++i)
            {
                if (clipPool.clip[i].name == clipName)
                    return i;
            }
        }
        return -1;
    }

    public static int a3clipCalculateDuration(a3_ClipPool clipPool, int clipIndex, double playback_stepPerSec)
    {
        if (clipPool != null && clipPool.clip != null && clipIndex < clipPool.clipCount && playback_stepPerSec > 0.0)
        {
            a3_Clip clip = clipPool.clip[clipIndex];
            int i, k;
            clip.duration_step = 0;
            clip.duration_sec = 0.0;
            clip.durationInv = 0.0;

            for (i = 0, k = clip.keyframeIndex_first; i < clip.keyframeCount; ++i, k += clip.keyframeDirection)
            {
                clip.duration_step += clipPool.keyframe[k].duration_step;
            }

            clip.duration_sec = (double)clip.duration_step / playback_stepPerSec;
            clip.durationInv = 1.0f / clip.duration_sec;
            return clip.index;
        }
        return -1;
    }


    public static int a3clipDistributeDuration(a3_ClipPool clipPool, int clipIndex, double playback_stepPerSec)
    {
        if (clipPool != null && clipPool.clip != null && clipIndex < clipPool.clipCount && playback_stepPerSec > 0.0)
        {
            a3_Clip clip = clipPool.clip[clipIndex];
            a3_Keyframe keyframe;
            a3_Sample sample0, sample1;
            int i, k;

            for (i = 0, k = clip.keyframeIndex_first; i < clip.keyframeCount; ++i, k += clip.keyframeDirection)
            {
                keyframe = clipPool.keyframe[k];
                sample0 = clipPool.sample[keyframe.sampleIndex0];
                sample1 = clipPool.sample[keyframe.sampleIndex1];

                a3sampleInit(ref sample0, ((sample0.index * clip.duration_step) / clip.keyframeCount), playback_stepPerSec);
                a3sampleInit(ref sample1, ((sample1.index * clip.duration_step) / clip.keyframeCount), playback_stepPerSec);

                clipPool.sample[keyframe.sampleIndex0] = sample0;
                clipPool.sample[keyframe.sampleIndex1] = sample1;

                a3keyframeInit(ref keyframe, sample0, sample1, playback_stepPerSec);
                clipPool.keyframe[k] = keyframe;
            }

            return clip.index;
        }
        return -1;
    }
}
