namespace Imas
{
    class LoadCharacterActor : Actor
    {
        string idol6id;
        string modelId;
        string gameObjectName;
        int SetId;
        string costumeName;
        int costumeNumber;
        string costumeVariation;

        public LoadCharacterActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            idol6id = sequence.arg4;
            SetId = int.Parse(sequence.arg5);
            modelId = sequence.arg6;
            costumeName = sequence.arg7;
            costumeNumber = int.Parse(sequence.arg8);
            costumeVariation = sequence.arg9;
        }

        public override void Init()
        {
            CharacterManager.CreateCharacter(
                idol6id,
                $"{costumeName}{costumeNumber:000}{costumeVariation}_{modelId}"
            );
        }
    }
}
