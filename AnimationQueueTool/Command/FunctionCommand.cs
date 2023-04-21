using System;
using System.Collections.Generic;
using Script.Utility.AnimationQueueTool.Args;

namespace Script.Utility.AnimationQueueTool.Command
{

    public class FunctionCommand : AbstractCommand
    {
        public override CommandType Competition { get; } = AbstractCommand.CommandType.NonCompetition;

        private Action _action;

        public FunctionCommand(Action action)
        {
            _action = action;
            NeedBlock = false;
        }

        protected override void ExecSubClassCommand()
        {
            _action.Invoke();
            InvalidAndReleaseBlock(this);
        }

        public override IEnumerator<BaseCommandArgs> GetEnumerator()
        {
            return null;
        }
    }
}