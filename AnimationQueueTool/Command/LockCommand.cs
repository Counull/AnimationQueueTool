using System;
using System.Collections.Generic;
using Script.Utility.AnimationQueueTool.Args;


namespace Script.Utility.AnimationQueueTool.Command
{
    public class LockCommand : AbstractCommand
    {
        public override CommandType Competition => CommandType.NonCompetition;
        public Action Action { get; set; }

        public LockCommand()
        {
        }

        public LockCommand(Action action)
        {
            Action = action;
        }

        protected override void ExecSubClassCommand()
        {
            Action?.Invoke();
          
        }

        public void Release()
        {
            InvalidAndReleaseBlock(this);
        }


        public override IEnumerator<BaseCommandArgs> GetEnumerator()
        {
            return null;
        }
    }
}