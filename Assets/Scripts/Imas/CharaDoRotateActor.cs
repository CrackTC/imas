using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Imas
{
    class CharaDoRotateActor : Actor
    {
        string mGameObjectName;
        string EasingType;
        string moveWay;
        Vector3 rotateVector;

        public CharaDoRotateActor(Sequence seq)
        {
            mGameObjectName = seq.arg4;
            cutId = seq.arg1;
            from_cut = int.Parse(seq.arg2);
            to = int.Parse(seq.arg3);
            seqId = seq.seq_id;
            moveWay = seq.arg9;
            EasingType = seq.arg8;
            rotateVector = new Vector3(
                float.Parse(seq.arg5),
                float.Parse(seq.arg6),
                float.Parse(seq.arg7)
            );
        }

        public override void Exec()
        {
            var seconds = (to - from_cut) * ScenarioManager.SCR_FRAME_TIME;
            var character = CharacterManager.GetCharacters(mGameObjectName).Single();

            var targetEuler = moveWay switch
            {
                "relative" => character.transform.eulerAngles + rotateVector,
                "abs" => rotateVector,
                _ => character.transform.eulerAngles,
            };

            var ease = GetEase(EasingType);
            character.transform.DOLocalRotate(targetEuler, seconds, RotateMode.Fast).SetEase(ease);
        }

        Ease GetEase(string val) =>
            val switch
            {
                "linear" => Ease.Linear,
                "easeInSine" => Ease.InSine,
                "easeOutSine" => Ease.OutSine,
                "easeInOutSine" => Ease.InOutSine,
                "easeInQuad" => Ease.InQuad,
                "easeOutQuad" => Ease.OutQuad,
                "easeInOutQuad" => Ease.InOutQuad,
                "easeInCubic" => Ease.InCubic,
                "easeOutCubic" => Ease.OutCubic,
                "easeInOutCubic" => Ease.InOutCubic,
                "easeInQuart" => Ease.InQuart,
                "easeOutQuart" => Ease.OutQuart,
                "easeInOutQuart" => Ease.InOutQuart,
                "easeInQuint" => Ease.InQuint,
                "easeOutQuint" => Ease.OutQuint,
                "easeInOutQuint" => Ease.InOutQuint,
                "easeInExpo" => Ease.InExpo,
                "easeOutExpo" => Ease.OutExpo,
                "easeInOutExpo" => Ease.InOutExpo,
                "easeInCirc" => Ease.InCirc,
                "easeOutCirc" => Ease.OutCirc,
                "easeInOutCirc" => Ease.InOutCirc,
                "easeInElastic" => Ease.InElastic,
                "easeOutElastic" => Ease.OutElastic,
                "easeInOutElastic" => Ease.InOutElastic,
                "easeInBack" => Ease.InBack,
                "easeOutBack" => Ease.OutBack,
                "easeInOutBack" => Ease.InOutBack,
                "easeInBounce" => Ease.InBounce,
                "easeOutBounce" => Ease.OutBounce,
                "easeInOutBounce" => Ease.InOutBounce,
                _ => Ease.Linear,
            };
    }
}
