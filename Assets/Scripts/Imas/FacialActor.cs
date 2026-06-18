using System.Linq;

namespace Imas
{
    class FacialActor : Actor
    {
        string mGameObjectName;
        int facialId;

        public FacialActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            mGameObjectName = sequence.arg4;
            facialId = int.Parse(sequence.arg5);
        }

        public override void Exec() =>
            CharacterManager.GetCharacters(mGameObjectName).Single().FaceCtrl.SetFace(facialId - 1);
    }
}
