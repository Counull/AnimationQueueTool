using Script.Utility.AnimationQueueTool.Command;
using UnityEngine;

namespace Script.Utility.AnimationQueueTool
{
    public class AnimationQueueCallback : MonoBehaviour
    {
        public AnimationCommand Command { get; set; }

        private void PlayEndCallBack(GameObject obj)
        {
            if (obj == this.gameObject)
                Command.EventStopAnimation(obj);
        }
    }
}