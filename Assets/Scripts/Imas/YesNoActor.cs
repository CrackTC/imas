using System.Linq;

namespace Imas
{
    class YesNoActor : Actor
    {
        string mGameObjectName;
        string yes_or_no;

        public YesNoActor(Sequence seq)
        {
            cutId = seq.arg1;
            from_cut = int.Parse(seq.arg2);
            to = int.Parse(seq.arg3);
            seqId = seq.seq_id;
            mGameObjectName = seq.arg4;
            yes_or_no = seq.arg5;
        }

        public override void Exec()
        {
            var character = CharacterManager.GetCharacters(mGameObjectName).Single();
            if (yes_or_no == "yes")
            {
                character.CharacterActorCommu.PlayYes();
            }
            else if (yes_or_no == "no")
            {
                character.CharacterActorCommu.PlayNo();
            }
        }
    }
}
