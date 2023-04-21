using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Script.Utility.AnimationQueueTool.Queue
{
    public class CommandQueueToolAutoCoroutine : CommandQueueTool
    {
        private AnimationCoroutineSupport _coroutineSupport = null;
        private readonly GameObject _coroutineObj;


        public CommandQueueToolAutoCoroutine([NotNull] GameObject coroutineObj)
        {
            this._coroutineObj = coroutineObj;
         
        }


        public override void Start()
        {
            base.Start();
            _coroutineSupport = _coroutineObj.GetComponent<AnimationCoroutineSupport>();
            if (_coroutineSupport == null)
            {
                _coroutineSupport = _coroutineObj.AddComponent<AnimationCoroutineSupport>();
            }


            _coroutineSupport.StartCoroutine(AnimationLoop());
        }

        public override void Stop()
        {
            IsStop = true;
            CommandQueue.Clear();
        }

        public override void Destroy()
        {
            IsValid = false;
            if (IsStop)
            {
                Object.Destroy(_coroutineSupport);
                _coroutineSupport = null;
            }
        }


        /// <summary>
        /// 动画事件循环，也就是这个类的主要功能实现的地方，在无阻塞且队列内有command时调度播放动画
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimationLoop()
        {
            //Main loop
            while (!IsStop)
            {
                while (IsBlocking || CommandQueue.Count <= 0)
                {
                    //Block self loop
                    yield return null;
                    //When queue is empty or the program call stop(),BREAK the main loop
                    CheckShouldStop();

                    //if program call Destroy() and need destroy this tool
                    if (!IsValid)
                    {
                        Stop();
                        break;
                    }
                }


                //if not blocking and queue is valid
                if (IsValid)
                {
                    ProcessAbstractCommand();
                }
            }

            //When the main loop break（like queue is empty），clean the coroutine tool
            while (CommandInProgress.Count > 0)
            {
                //waiting for all command complete
                yield return null;
            }

            NotifyAllCommandExecComplete();
            if (!IsValid)
            {
                Object.Destroy(_coroutineSupport);
                _coroutineSupport = null;
            }

            Reset();
        }
    }
}