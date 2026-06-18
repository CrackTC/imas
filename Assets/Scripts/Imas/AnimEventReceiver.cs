using UnityEngine;

namespace Imas
{
    [RequireComponent(typeof(Animator))]
    class AnimEventReceiver : MonoBehaviour
    {
        public void _AnimEvent_EndFrame(AnimationClip clip)
        {
            if (clip.name.EndsWith("_lop"))
            {
                var animator = GetComponent<Animator>();

                if (
                    !animator.IsInTransition(0)
                    && animator.GetCurrentAnimatorClipInfo(0)[0].clip == clip
                )
                {
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    animator.Play(state.fullPathHash, 0, 0);
                }
            }
        }
    }
}
