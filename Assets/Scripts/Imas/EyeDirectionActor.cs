using System.Linq;
using UnityEngine;

namespace Imas
{
    class EyeDirectionActor : Actor
    {
        string mGameObjectName;
        int hVal;
        int vVal;

        Character mCharacter;
        float hvec;
        float vvec;
        float endhrate;
        float endvrate;

        public EyeDirectionActor(Sequence seq)
        {
            cutId = seq.arg1;
            from_cut = int.Parse(seq.arg2);
            to = int.Parse(seq.arg3);
            seqId = seq.seq_id;
            mGameObjectName = seq.arg4;
            hVal = int.Parse(seq.arg5);
            vVal = int.Parse(seq.arg6);
        }

        public override void Exec()
        {
            mCharacter = CharacterManager.GetCharacters(mGameObjectName).Single();
            var eyeDirectionRate = mCharacter.EyeTracking.GetEyeDirectionRate();
            float totFrame = to - from_cut;
            endhrate = hVal / 100f;
            endvrate = vVal / 100f;
            var diff = new Vector2(endhrate, endvrate) - eyeDirectionRate;
            hvec = diff.x / totFrame;
            vvec = diff.y / totFrame;
        }

        int _LastFrame = 0;

        public override void OnUpdate(int frame)
        {
            if (mCharacter == null || _LastFrame >= to - from_cut)
                return;
            var deltaFrame = Mathf.Clamp(frame, frame, to - from_cut) - _LastFrame;
            var eyeDirectionRate = mCharacter.EyeTracking.GetEyeDirectionRate();
            mCharacter.EyeTracking.SetEyeDirectionByRate(
                eyeDirectionRate.x + hvec * deltaFrame,
                eyeDirectionRate.y + vvec * deltaFrame
            );
            _LastFrame = frame;
        }
    }
}
