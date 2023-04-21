using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.Utility.AnimationQueueTool.Queue;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Command
{
    /// <summary>
    /// 抽象的动画指令，如需实现自定义指令类应继承该类
    /// </summary>
    public abstract class AnimationCommand : AbstractCommand
    {
        public override CommandType Competition => CommandType.Competition;

        private const string CallBackFunName = "PlayEndCallBack";

        
        //动画持续时间
        public float EndTime = -1f;


        protected void SetPlayEvent()
        {
            Dictionary<AnimationClip, List<AnimationEvent>> eventsDic =
                new Dictionary<AnimationClip, List<AnimationEvent>>();
            foreach (var obj in this)
            {
                var animationArgs = obj as AnimationCommandArgs;
                var gameObject = animationArgs.GameObject;
                var callback = gameObject.GetComponent<AnimationQueueCallback>();
                if (gameObject.GetComponent<AnimationQueueCallback>() == null)
                {
                    callback = gameObject.AddComponent<AnimationQueueCallback>();
                }

                callback.Command = this;
                var animator = gameObject.GetComponent<Animator>();

                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                foreach (var clip in clips) //过于浪费性能 要不要直接传入clip而不是动画的hash
                {
                    if (Animator.StringToHash(clip.name) == animationArgs.AnimationNameHash)
                    {
                        if (!eventsDic.ContainsKey(clip))
                        {
                            eventsDic.Add(clip, new List<AnimationEvent>());
                        }


                        float eventTime;
                        if (animationArgs.Loop != null)
                        {
                            //Loop
                            var loopDuring = animationArgs.Loop.During;
                            if (animationArgs.Loop.LoopCounter <= 0)
                            {
                                eventTime = (loopDuring % clip.length);
                                animationArgs.Loop.LoopCounter = (uint)(loopDuring / clip.length + 1);
                            }
                            else
                            {
                                eventTime = EndTime < 0
                                    ? clip.length
                                    : (loopDuring % clip.length);
                            }
                        }
                        else
                        {
                            eventTime = EndTime < 0 || clip.length <= EndTime
                                ? clip.length
                                : EndTime;
                        }


                        AnimationEvent animationEvent = new AnimationEvent
                        {
                            functionName = CallBackFunName,
                            objectReferenceParameter = animationArgs.GameObject,

                            time = eventTime - float.Epsilon
                        };
                        eventsDic[clip].Add(animationEvent);
                        break;
                    }
                }
            }

            foreach (var kv in eventsDic)
            {
                kv.Key.events = kv.Value.ToArray();
            }
        }


        public abstract void EventStopAnimation(GameObject obj);

        public void ForceStopAnimation(bool rebind = false)
        {
            foreach (var args in this)
            {
                var targetGameObject = (args as AnimationCommandArgs)?.GameObject;

                InvalidAndReleaseBlock(targetGameObject);
                if (rebind)
                {
                    if (targetGameObject != null) targetGameObject.GetComponent<Animator>().Rebind();
                }
            }
        }

        /// <summary>
        /// 用户 必须 通过动画结束时的回调事件调用此函数以告知某动画已播放完毕可将相应的command释放同时符合条件时释放阻塞
        /// </summary>
        /// <param name="argsetGameObject"></param>
        protected void TryStopAnimation(AnimationCommandArgs args)
        {
            var loopArgs = args.Loop;

            if (loopArgs != null)
            {
                --loopArgs.LoopCounter;
                loopArgs.AfterLoopCounterChange?.Invoke(loopArgs.LoopCounter);
            }

            if (loopArgs == null || loopArgs.LoopCounter == 0)
            {
                InvalidAndReleaseBlock(args.GameObject);
                if (args.RebindAnimation)
                {
                    args.GameObject.GetComponent<Animator>().Rebind();
                }
            }
        }
    }
}