using System.Linq;
using UnityEngine;

namespace Imas
{
    class SetPositionActor : Actor
    {
        Vector3 localPosition;
        string idol6id;

        public SetPositionActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            idol6id = sequence.arg4;
            localPosition = new Vector3(
                float.Parse(sequence.arg5),
                float.Parse(sequence.arg6),
                float.Parse(sequence.arg7)
            );
        }

        public override void Exec()
        {
            CharacterManager.GetCharacters(idol6id).Single().SetPosition(localPosition);
        }
    }
}
