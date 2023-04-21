using System;
using System.Collections;
using System.Collections.Generic;
using Script.Utility.AnimationQueueTool.Args;
using Script.Utility.AnimationQueueTool.Queue;
using UnityEngine;
using UniversalModule.DelaySystem;

namespace Script.Utility.AnimationQueueTool.Command
{
    public class WaitCommand : AbstractCommand
    {
        public override CommandType Competition => CommandType.NonCompetition;

        private readonly float _during = 0.0f;

        public WaitCommand(float during)
        {
            if (during < float.Epsilon)
            {
                throw new Exception("Invalid parameter for wait command.");
            }


            _during = during;
        }


        protected override void ExecSubClassCommand()
        {
            DelayCallback.Delay(this,_during, () => { InvalidAndReleaseBlock(this); });
        }

        public override IEnumerator<BaseCommandArgs> GetEnumerator()
        {
            return null;
        }
    }
}