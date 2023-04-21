using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Script.Utility.AnimationQueueTool.Args;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Command
{
    /// <summary>
    /// 有点走了邪路的异步动画集合指令，一个命令异步播放所有动画，当所有动画失效后此Command失效
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MultAsyncAnimationCommand : AnimationCommand // where T : IAniPrm
    {
        //这里传的是引用如果在commit之后再向参数类内传数据可能会影响后续正常运行 或许我应该写个单独的函数判断状态是否为准备时才可添加数据
        public MultAsyncAnimationCommand() : base()
        {
            TargetObject = new Dictionary<GameObject, BaseCommandArgs>();
        }

        /// <summary>
        /// 动画命令所对应的GameObject实体
        /// </summary>
        private Dictionary<GameObject, BaseCommandArgs> TargetObject { get; }

        private int _invalidCounter;


        public void AddAnimation(AnimationCommandArgs args)
        {
            if (CommandState == CommandStatus.Prepare)
            {
                TargetObject.Add(args.GameObject, args);
                return;
            }

            throw new Exception("Command already commit or invalid");
        }


        internal override void TryInvalid()
        {
            if (CommandState == CommandStatus.Playing)
            {
                _invalidCounter++;

                if (_invalidCounter >= TargetObject.Count)
                {
                    Debug.Log("MultAsyncAnimCommand invalid");
                    CommandState = CommandStatus.Invalid;
                    NotifyAnimationCommandInvalid();
                }
            }
        }

        public override IEnumerator<BaseCommandArgs> GetEnumerator()
        {
            return TargetObject.Values.ToList().GetEnumerator();
        }

        protected override void ExecSubClassCommand()
        {
            if (TargetObject.Count == 0)
            {
                throw new Exception("Animation command empty");
            }

            SetPlayEvent();


            //我 在 写 什 么
            foreach (var args in TargetObject)
            {
                if (args.Value is AnimationCommandArgs animationArgs)
                {
                    animationArgs.BeforeAnimationPlay?.Invoke(animationArgs);
                    var animator = animationArgs.GameObject.GetComponent<Animator>();
                    if (animator.isActiveAndEnabled)
                    {
                        animator.Rebind();
                        animator.Play(animationArgs.AnimationNameHash);
                        animationArgs.AnimationPlayCallback?.Invoke(animationArgs);
                    }
                }
            }

           
        }

        public override void EventStopAnimation(GameObject obj)
        {
            TryStopAnimation(TargetObject[obj] as AnimationCommandArgs);
        }
    }
}