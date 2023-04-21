using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.Utility.AnimationQueueTool.Args;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Command
{
    /// <summary>
    /// 单步动画指令，当动画播放完毕或是被后续动画替代则此指令显现Invalid状态
    /// </summary>
    /// <typeparam name="T">动画指令所携带的参数类型，该参数会在play时传递给被调用函数</typeparam>
    public sealed class SingleAnimationCommand : AnimationCommand //where T : IAniPrm
    {
        //这里的设计应该让支持此组件的Mono支持一个动画接口以播放动画 这样就不需要在command的构造函数里填写动画的Hash
        //此时这个参数与播放的动画毫无关系，容易出问题
        public SingleAnimationCommand(AnimationCommandArgs commandArgs) : base()
        {
            CommandArgs = commandArgs;

            _enumerator = new SingleEnumerator(CommandArgs);
        }

        public static SingleAnimationCommand Build(GameObject obj, int animationHash)
        {
            return new SingleAnimationCommand(AnimationCommandArgs.Build(obj, animationHash));
        }


        private readonly SingleEnumerator _enumerator;

        public AnimationCommandArgs CommandArgs { get; }


        protected override void ExecSubClassCommand()
        {
            SetPlayEvent();

            CommandArgs.BeforeAnimationPlay?.Invoke(CommandArgs);

            var animator = CommandArgs.GameObject.GetComponent<Animator>();
            if (animator.isActiveAndEnabled)
            {
                animator.Rebind();
                animator.Play(CommandArgs.AnimationNameHash);
                CommandArgs.AnimationPlayCallback?.Invoke(CommandArgs);
            }
        }

        public override void EventStopAnimation(GameObject obj)
        {
            TryStopAnimation(CommandArgs);
        }

        public override IEnumerator<BaseCommandArgs> GetEnumerator()
        {
            return _enumerator; //这么写很魔怔 .net库里都是现new
        }

        private class SingleEnumerator : IEnumerator<BaseCommandArgs>
        {
            private BaseCommandArgs _baseCommandArgs;

            private bool _moved = false;

            public SingleEnumerator(BaseCommandArgs args)
            {
                _baseCommandArgs = args;
            }

            public bool MoveNext()
            {
                _moved = !_moved;
                return _moved;
            }

            public void Reset()
            {
                _moved = false;
            }

            public BaseCommandArgs Current => _moved ? _baseCommandArgs : null;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}