using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class a3_ClipController
{
    public string name;

    // index of clip in pool and keyframe in clip
    public int clipIndex;
    public int keyframeIndex;

    // clip time, keyframe time and speed in steps
    public int clipTime_step;
    public int keyframeTime_step;
    public int playback_step;

    // clip time, keyframe time and speed in seconds
    public double clipTime_sec;
    public double keyframeTime_sec;
    public double playback_sec;
    public double playback_stepPerSec;
    public double playback_secPerStep;

    // clip and keyframe interpolation parameters
    public double clipParam;
    public double keyframeParam;

    // clip pool and pointers
    public a3_ClipPool clipPool;
    public a3_Clip clip;
    public a3_Keyframe keyframe;

    public a3_ClipController()
    {
        name = "unnamed clip ctrl";
    }
}

public static class KeyframeAnimationController
{
    public static int a3clipControllerSetPlayback(a3_ClipController clipCtrl, int playback_step, double playback_stepPerSec)
    {
        if (clipCtrl != null && clipCtrl.clipPool != null && playback_stepPerSec > 0.0)
        {
            clipCtrl.playback_step = playback_step;
            clipCtrl.playback_stepPerSec = playback_stepPerSec;
            clipCtrl.playback_secPerStep = playback_stepPerSec != 0 ? (playback_stepPerSec > 0 ? +1 : -1) : 0;
            clipCtrl.playback_sec = (double)playback_step * clipCtrl.playback_secPerStep;
            return 1;
        }
        return -1;
    }

    public static int a3clipControllerRefresh(a3_ClipController ctrl, a3_ClipPool clipPool)
    {
        if (ctrl != null && clipPool != null)
        {
            ctrl.clipPool = clipPool;
            ctrl.clip = clipPool.clip[ctrl.clipIndex];
            ctrl.keyframe = clipPool.keyframe[ctrl.keyframeIndex];
            return 1;
        }
        return -1;
    }

    public static int a3clipControllerSetClip(a3_ClipController clipCtrl, a3_ClipPool clipPool, int clipIndex_pool, int playback_step, double playback_stepPerSec)
    {
        if (clipCtrl != null && clipPool != null && clipPool.clip != null && clipIndex_pool < clipPool.clipCount && playback_stepPerSec > 0.0)
        {
            clipCtrl.clipPool = clipPool;
            clipCtrl.clipIndex = clipIndex_pool;
            clipCtrl.clip = clipPool.clip[clipIndex_pool];

            // default testing behavior: set to first keyframe, discard time
            clipCtrl.keyframeIndex = clipCtrl.clip.keyframeIndex_first;
            clipCtrl.keyframe = clipPool.keyframe[clipCtrl.keyframeIndex];
            clipCtrl.clipTime_step = 0;
            clipCtrl.keyframeTime_step = 0;
            clipCtrl.clipTime_sec = 0.0;
            clipCtrl.keyframeTime_sec = 0.0;
            clipCtrl.clipParam = 0.0;
            clipCtrl.keyframeParam = 0.0;

            // set playback state
            a3clipControllerSetPlayback(clipCtrl, playback_step, playback_stepPerSec);

            // done
            return clipIndex_pool;
        }
        return -1;
    }

    /// <summary>
    /// a3clipControllerInit
    /// Initialize clip controller
    /// </summary>
    public static int a3clipControllerInit(a3_ClipController clipCtrl_out, string ctrlName, a3_ClipPool clipPool, int clipIndex_pool, int playback_step, double playback_stepPerSec)
    {
        int ret = a3clipControllerSetClip(clipCtrl_out, clipPool, clipIndex_pool, playback_step, playback_stepPerSec);
        if (ret >= 0)
        {
            string searchName = (!string.IsNullOrEmpty(ctrlName)) ? ctrlName : "unnamed clip ctrl";
            clipCtrl_out.name = searchName;
            return ret;
        }
        return -1;
    }

    public static int a3clipControllerUpdate(a3_ClipController clipCtrl, double dt)
    {
        if (clipCtrl != null && clipCtrl.clipPool != null)
        {
            // variables
            double overstep;

            // time step
            dt *= clipCtrl.playback_sec;
            clipCtrl.clipTime_sec += dt;
            clipCtrl.keyframeTime_sec += dt;

            // resolve forward
            while ((overstep = clipCtrl.keyframeTime_sec - clipCtrl.keyframe.duration_sec) >= 0.0)
            {
                // are we passing the forward terminus of the clip
                if (clipCtrl.keyframeIndex == clipCtrl.clip.keyframeIndex_final)
                {
                    // handle forward transition

                    // default testing behavior: loop with overstep
                    clipCtrl.keyframeIndex = clipCtrl.clip.keyframeIndex_first;
                    clipCtrl.keyframe = clipCtrl.clipPool.keyframe[clipCtrl.keyframeIndex];
                    clipCtrl.keyframeTime_sec = overstep;
                }
                // are we simply moving to the next keyframe
                else
                {
                    // set keyframe indices
                    clipCtrl.keyframeIndex += clipCtrl.clip.keyframeDirection;

                    // set keyframe pointers
                    clipCtrl.keyframe = clipCtrl.clipPool.keyframe[clipCtrl.keyframeIndex];

                    // new time is just the overstep
                    clipCtrl.keyframeTime_sec = overstep;
                }
            }

            // resolve reverse
            while ((overstep = clipCtrl.keyframeTime_sec) < 0.0)
            {
                // are we passing the reverse terminus of the clip
                if (clipCtrl.keyframeIndex == clipCtrl.clip.keyframeIndex_first)
                {
                    // handle reverse transition

                    // default testing behavior: loop with overstep
                    clipCtrl.keyframeIndex = clipCtrl.clip.keyframeIndex_final;
                    clipCtrl.keyframe = clipCtrl.clipPool.keyframe[clipCtrl.keyframeIndex];
                    clipCtrl.keyframeTime_sec = overstep + clipCtrl.keyframe.duration_sec;
                }
                // are we simply moving to the previous keyframe
                else
                {
                    clipCtrl.keyframeIndex -= clipCtrl.clip.keyframeDirection;

                    // set keyframe pointers
                    clipCtrl.keyframe = clipCtrl.clipPool.keyframe[clipCtrl.keyframeIndex];

                    // new time is overstep (negative) from new duration
                    clipCtrl.keyframeTime_sec = overstep + clipCtrl.keyframe.duration_sec;
                }
            }

            // normalize
            clipCtrl.keyframeParam = clipCtrl.keyframeTime_sec * clipCtrl.keyframe.durationInv;
            clipCtrl.clipParam = clipCtrl.clipTime_sec * clipCtrl.clip.durationInv;

            // done
            return 1;
        }
        return -1;
    }
}
