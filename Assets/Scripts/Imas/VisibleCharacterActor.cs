using System;
using System.Linq;

namespace Imas
{
    class VisibleCharacterActor : Actor
    {
        string idol6id;
        string Visible;

        public VisibleCharacterActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            idol6id = sequence.arg4;
            Visible = sequence.arg5;
        }

        public override void Exec()
        {
            switch (Visible)
            {
                case "on":
                    CharacterManager.GetCharacters(idol6id).Single().SetVisible(true);
                    break;
                case "off":
                    CharacterManager.GetCharacters(idol6id).Single().SetVisible(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(Visible),
                        $"Unexpected value: {Visible}"
                    );
            }
        }
    }
}
