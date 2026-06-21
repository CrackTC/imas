using System.Linq;

namespace Imas
{
    class TextActor : Actor
    {
        string MessagerName;
        string comment;
        string cuesheet;
        string cuename;
        int frameCount;
        string characterId;
        string DisplayCharacterId;
        string LogCharacterId;
        string rowDisplayCharacterId;
        string rowMessageId;

        public TextActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            characterId = sequence.arg4;
            rowDisplayCharacterId = sequence.arg5;
            rowMessageId = sequence.arg6;
            cuesheet = sequence.arg7;
            cuename = sequence.arg8;
        }

        Character _Character;
        FHout[] _FHouts;

        public override void Exec()
        {
            var characters = CharacterManager.GetCharacters(characterId);
            if (characters.Count == 0)
                return;

            _Character = characters.Single();
            _FHouts = ScenarioManager.Instance._FHoutTexts[rowMessageId];
        }

        int _Index = 0;

        public override void OnUpdate(int frame)
        {
            if (_Character == null)
                return;

            while (_Index < _FHouts.Length)
            {
                var fhout = _FHouts[_Index];
                if (fhout.t > frame)
                    break;

                _Character.FaceCtrl.SetTestLip(fhout.v);
                _Character.FaceCtrl.SetTestLipEnable(true);

                _Index++;
            }
        }
    }
}
