using System;
using System.Collections;
using System.Collections.Generic;
using Script.Utility.AnimationQueueTool.Args;
using Script.Utility.AnimationQueueTool.Command;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Queue
{
    /// <summary>
    /// 动画调度队列 向队列内添加阻塞或不阻塞播放的动画以实现动画同步或异步播放
    /// </summary>
    public abstract class CommandQueueTool
    {
        public event Action OnAllCommandExecComplete;

        protected bool IsStop { get; set; } = true;

        protected bool IsValid { get; set; } = true;

        /// <summary>
        /// 一个命令队列，用于存储未被执行的动画命令
        /// 当事件循环空闲时会从此队列前端取出命令以调用
        /// </summary>
        public Queue<AbstractCommand> CommandQueue { get; } = new Queue<AbstractCommand>();

        protected bool IsBlocking; //先让逻辑不那么复杂使用的阻塞标记 如果需要优化需要在AniCommand需要Block的时候结束掉轮询携程
        private AbstractCommand currentAniCommand = null;


        /// <summary>
        /// 存储已被调用play且未失效的动画命令
        /// 其一，实现异步动画的栅栏（当异步调用多个动画并阻塞时，每个动画失效后会从这个表中被移除，当此表大小为0时即可释放阻塞）
        /// 其二，方便后续添加代码撤销动画的执行
        /// </summary>
        protected readonly Dictionary<GameObject, AbstractCommand> CommandInProgress =
            new Dictionary<GameObject, AbstractCommand>();

        private readonly List<AbstractCommand> _nonCompetitionList = new List<AbstractCommand>();

        protected void Reset()
        {
            IsValid = true;
            IsStop = true;
            IsBlocking = false;
            CommandQueue.Clear();

            CommandInProgress.Clear();
        }


        /// <summary>
        /// 将命令添加入队列等待被事件循环调度
        /// </summary>
        /// <param name="command">继承于AbstractAniCommand的Command实例，由用户定义</param>
        /// <returns></returns>
        public CommandQueueTool AddCommand(AbstractCommand command)
        {
            if (IsValid)
            {
                if (command.CommandState != AbstractCommand.CommandStatus.Prepare)
                {
                    throw new Exception("Add command failed,invalid command");
                }

                CommandQueue.Enqueue(command);
                return this;
            }

            throw new Exception("Add command failed,invalid AnimationQueue");
        }

        /// <summary>
        /// 开启事件循环执行队列内的command
        /// </summary>
        public virtual void Start()
        {
            IsValid = true;
            IsStop = false;
        }


        public abstract void Stop();

        public abstract void Destroy();

        /// <summary>
        /// 强制将动画插入队列最前端播放，阻塞模式为：队列阻塞状态和新添加的动画阻塞状态任一为true则阻塞（或运算）
        /// </summary>
        /// <param name="command">继承于AbstractAniCommand的Command实例，由用户定义</param>
        public void ForcePlay(AbstractCommand command)
        {
            /*SetPlayEvent(command);
            command.Play();
            IsBlocking |= command.NeedBlock;
            Add2Fence(command);*/
        }


        public virtual void TryReleaseBlock(GameObject targetGameObject)
        {
            CommandInProgress.Remove(targetGameObject);


            IsBlocking = !IsFenceEmpty();
        }

        public void TryReleaseNoneCompetitionBlock(AbstractCommand command)
        {
            if (command.Competition != AbstractCommand.CommandType.NonCompetition)
            {
                throw new Exception("NonCompetition command");
            }

            _nonCompetitionList.Remove(command);

            IsBlocking = !IsFenceEmpty();
        }


        private bool IsFenceEmpty()
        {
            return CommandInProgress.Count == 0 && _nonCompetitionList.Count == 0;
        }

        protected void ProcessAbstractCommand()
        {
            while (CommandQueue.Count > 0)
            {
                if (currentAniCommand == null)
                {
                    currentAniCommand = CommandQueue.Dequeue();
                    Add2Fence(currentAniCommand);
                }

                do
                {
                    currentAniCommand.Exec(this);
                } while (!currentAniCommand.SubCommandBlocking && currentAniCommand.HasSubCommand);

                //这两天重构得跟喝了假酒似的
                IsBlocking = currentAniCommand.NeedBlock;
                if (!currentAniCommand.HasSubCommand)
                {
                    currentAniCommand = null;
                }


                if (IsBlocking)
                {
                    break;
                }
            }
        }


        protected void CheckShouldStop()
        {
            //Check queue is empty
            IsStop = CommandQueue.Count == 0 && IsFenceEmpty();

            //Check is Destroy()
            IsValid = !IsStop && IsValid;
        }


        /// <summary>
        /// 多动画异步播放栅栏实现，异步时动画时长不一，通过到达统一的栅栏释放动画的阻塞，
        /// 当PlayingAnimation内为空时说明异步播放的动画全部完成
        /// 需在play后调用
        /// </summary>
        /// <param name="currentAbstractCommand">当前被执行的动画</param>
        private void Add2Fence(AbstractCommand currentAbstractCommand)
        {
            if (currentAbstractCommand.Competition == AbstractCommand.CommandType.NonCompetition)
            {
                _nonCompetitionList.Add(currentAbstractCommand);
                return;
            }


            foreach (var args in currentAbstractCommand)
            {
                var targetGameObject = (args as CompetitionCommandArgs)?.GameObject;
                if (targetGameObject == null)
                {
                    throw new Exception("Competition command fence Object is null");
                }

                if (CommandInProgress.ContainsKey(targetGameObject))
                {
                    CommandInProgress[targetGameObject].TryInvalid();
                    CommandInProgress.Remove(targetGameObject);
                }

                CommandInProgress.Add(targetGameObject, currentAbstractCommand);
            }
        }

        protected void NotifyAllCommandExecComplete()
        {
            OnAllCommandExecComplete?.Invoke();
            OnAllCommandExecComplete = null;
        }
    }
}