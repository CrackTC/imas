using System;
using System.Linq;

namespace Imas
{
    class EyeBlinkActor : Actor
    {
        string mGameObjectName;
        string eyemode;

        public EyeBlinkActor(Sequence seq)
        {
            cutId = seq.arg1;
            from_cut = int.Parse(seq.arg2);
            to = int.Parse(seq.arg3);
            seqId = seq.seq_id;
            mGameObjectName = seq.arg4;
            eyemode = seq.arg5;
        }

        public override void Exec()
        {
            var character = CharacterManager.GetCharacters(mGameObjectName).Single();
            character.FaceCtrl.SetEyeMode(
                eyemode switch
                {
                    "blink" => CharacterEyeMode.Blink,
                    "open" => CharacterEyeMode.Open,
                    "close" => CharacterEyeMode.Close,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(eyemode),
                        $"Invalid eyemode: {eyemode}"
                    ),
                },
                true
            );
        }
    }
}
