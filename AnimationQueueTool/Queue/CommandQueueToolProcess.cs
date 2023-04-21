using System;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Queue
{
    public class CommandQueueToolProcess : CommandQueueTool
    {
        public override void Start()
        {
            if (!IsValid)
            {
                throw new Exception("Commit failed,invalid CommandQueue");
            }

            if (IsStop)
            {
                base.Start();
                Process();
            }
        }

        public override void Stop()
        {
            CommandQueue.Clear();
            IsValid = false;
        }

        public override void Destroy()
        {
            Stop();
        }


        public override void TryReleaseBlock(GameObject targetGameObject)
        {
            base.TryReleaseBlock(targetGameObject);

            CheckShouldStop();
            if (IsStop)
            {
                NotifyAllCommandExecComplete();
                Reset();
                return;
            }

            if (!IsBlocking && !IsStop && IsValid)
            {
                Process();
            }
        }

        void Process()
        {
            ProcessAbstractCommand();
        }
    }
}