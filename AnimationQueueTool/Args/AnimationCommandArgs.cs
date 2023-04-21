using System;
using Script.Utility.AnimationQueueTool.Args;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool
{
    public class LoopArgs
    {
        //动画持续时间 此参数和LoopCounter只需设置一个，会影响AnimationCommand的EndTime
        public readonly float During = 0;

        // 动画循环次数 ，此数值会根据动画持续时间通过计算获得并在动画播放过程中被减少至0，所以此参数和During只需设置一个
        public uint LoopCounter = 0;

        //当一次动画播放完毕，会回调此事件
        public Action<uint> AfterLoopCounterChange;

        public LoopArgs(float during)
        {
            During = during;
        }

        public LoopArgs(uint loopCounter)
        {
            LoopCounter = loopCounter;
        }
    }


    public class AnimationCommandArgs : CompetitionCommandArgs
    {
        public static AnimationCommandArgs Build(GameObject obj, int animationHash)
        {
            return new AnimationCommandArgs
            {
                GameObject = obj,
                AnimationNameHash = animationHash
                
            };
        }

        
        //通过Animator.StringToHash()得到的Hash值，也就是此命令想要播放的动画的Hash
        public int AnimationNameHash { get; set; }
        
        //是否需要在播放前Rebind
        public bool RebindAnimation { get; set; } = false;

        //默认为null表示不循环播放动画，如播放的动画需设置此参需在Inspector设置动画属性为可循环
        public LoopArgs Loop;

        //在动画播放之前的回调，通常用于设置动画的Active
        public Action<AnimationCommandArgs> BeforeAnimationPlay { get; set; }

        //在动画开始播放时的回调，预留的
        public Action<AnimationCommandArgs> AnimationPlayCallback { get; set; }
    }

    public class AnimationCommandArgs<T> : AnimationCommandArgs
    {
        public T CallbackArgs;
    }
}