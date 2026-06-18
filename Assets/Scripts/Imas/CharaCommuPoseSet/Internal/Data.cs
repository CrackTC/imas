using System;

namespace Imas.CharaCommuPoseSet.Internal
{
    [Serializable]
    struct Data
    {
        public string date;

        public string kind;

        public DataSets sets;

        public DataAssigns assigns;

        public readonly bool IsValid() =>
            assigns.means != null && assigns.assigns != null && date != null && kind != null;
    }
}
