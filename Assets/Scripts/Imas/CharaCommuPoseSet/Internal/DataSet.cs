using System;

namespace Imas.CharaCommuPoseSet.Internal
{
    [Serializable]
    struct DataSet
    {
        public string[] vals;

        public string[] means;

        public readonly bool IsValid() => vals.Length != 0;

        public enum Param
        {
            None = -1,
            SetName = 0,
            Category = 1,
            Num = 2,
        }
    }
}
