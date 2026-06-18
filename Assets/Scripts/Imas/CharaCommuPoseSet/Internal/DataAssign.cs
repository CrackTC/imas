using System;

namespace Imas.CharaCommuPoseSet.Internal
{
    [Serializable]
    struct DataAssign
    {
        public string[] vals;

        public string[] means;

        public readonly bool IsValid() => vals.Length != 0;

        public enum Param
        {
            None = -1,
            CharacterId = 0,
            SetName = 1,
            SetNameSit = 2,
            Num = 3,
        }
    }
}
