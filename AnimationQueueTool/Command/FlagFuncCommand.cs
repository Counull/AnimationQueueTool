using System;
using System.Collections.Generic;
using Script.Utility.AnimationQueueTool.Args;
using UnityEditor;

namespace Script.Utility.AnimationQueueTool.Command
{
    public class FlagFuncCommand : AbstractCommand
    {
        public override CommandType Competition { get; } = CommandType.NonCompetition;

        protected override void ExecSubClassCommand()
        {
            InvalidAndReleaseBlock(this);
        }

        public override IEnumerator<BaseCommandArgs> GetEnumerator()
        {
            return null;
        }
    }
}