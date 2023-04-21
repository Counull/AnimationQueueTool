using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.Utility.AnimationQueueTool.Args;
using Script.Utility.AnimationQueueTool.Queue;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Command
{
    public abstract class AbstractCommand : IEnumerable<BaseCommandArgs>
    {
        public enum CommandStatus
        {
            Prepare, //准备中
            Playing, //被执行
            Invalid //播放完毕或是被取消
        }

        public enum CommandType
        {
            Competition,
            NonCompetition
        }


        public abstract CommandType Competition { get; }
        // public abstract GameObject[] GameObjects { get; }

        private CommandQueueTool _execQueue;
        public bool NeedBlock { get; set; } = true; //表示此动画是否会阻塞后续动画
        public bool HasSubCommand { get; set; } = false;

        private bool _subCommandBlocking = false;


        public CommandStatus CommandState { get; protected set; } = AnimationCommand.CommandStatus.Prepare;

        public bool SubCommandBlocking
        {
            get => _subCommandBlocking;
            set
            {
                _subCommandBlocking = value;
                NeedBlock = value;
            }
        }

        public Action<AbstractCommand> CommandInvalid;
        public Action<AbstractCommand> BeforeCommandExec;


        protected void InvalidAndReleaseBlock(GameObject targetGameObject)
        {
            TryInvalid();
            _execQueue.TryReleaseBlock(targetGameObject);
        }


        protected void InvalidAndReleaseBlock(AbstractCommand command)
        {
            _execQueue.TryReleaseNoneCompetitionBlock(command);
            TryInvalid();
        }

        /// <summary>
        /// 给事件队列调用的公开的播放函数，
        /// 由于必须保证在动画被实际播放前将命令状态修改成playing所以实际回调动画的函数是PlayAnimation();
        /// </summary>
        public void Exec([NotNull] CommandQueueTool queue)
        {
            NotifyBeforeCommandExec();

            _execQueue = queue;
            CommandState = CommandStatus.Playing;

            ExecSubClassCommand();
        }

        /// <summary>
        /// 子类必须定义的接口用以回调播放动画
        /// </summary>
        protected abstract void ExecSubClassCommand();


        /// <summary>
        /// 子类可定义接口，默认实现为直接将command置为失效状态
        /// </summary>
        internal virtual void TryInvalid()
        {
            if (CommandState == CommandStatus.Playing)
            {
                CommandState = CommandStatus.Invalid;
                NotifyAnimationCommandInvalid();
            }
        }

        protected void NotifyAnimationCommandInvalid()
        {
            CommandInvalid?.Invoke(this);
        }

        protected void NotifyBeforeCommandExec()
        {
            BeforeCommandExec?.Invoke(this);
        }

        public virtual void Reset()
        {
            if (CommandState != CommandStatus.Invalid)
                return;

            CommandState = CommandStatus.Prepare;
            _execQueue = null;
        }

        public abstract IEnumerator<BaseCommandArgs> GetEnumerator();


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}