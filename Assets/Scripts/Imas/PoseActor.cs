using System.Linq;

namespace Imas
{
    class PoseActor : Actor
    {
        string mGameObjectName;
        string poseId;
        int poseType;
        string state;
        float start_seconds;

        public PoseActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            mGameObjectName = sequence.arg4;
            poseId = sequence.arg5;
            poseType = int.Parse(sequence.arg6);
        }

        public override void Init()
        {
            CharacterManager.GetCharacters(mGameObjectName).Single().ScenarioMotions.Add(poseId);
        }

        public override void Exec()
        {
            CharacterManager
                .GetCharacters(mGameObjectName)
                .Single()
                .CharacterActorCommu.step_pose_execute(0.0f, poseId, poseType == 1);
        }
    }
}
