using System;

namespace Imas.CharaCommuPoseSet.Internal
{
    [Serializable]
    struct DataSets
    {
        public string[] means;

        public DataSet[] sets;

        public DataSets(string[] means, int set_count)
        {
            this.means = means;
            sets = new DataSet[set_count];
        }

        public readonly bool IsValid() => means != null && means.Length != 0;
    }
}
