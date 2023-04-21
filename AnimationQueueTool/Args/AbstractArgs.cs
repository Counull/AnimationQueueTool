using UnityEngine;

namespace Script.Utility.AnimationQueueTool.Args
{
    public interface BaseCommandArgs
    {
    }


    public class CompetitionCommandArgs : BaseCommandArgs
    {
        public GameObject GameObject;
    }

    public class NoneCompetitionCommandArgs : BaseCommandArgs
    {
    }
}