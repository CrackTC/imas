using System;

namespace Imas.CharaCommuPoseSet.Internal
{
    class Utility
    {
        public static string get_string_value<Tenum>(
            string[] means,
            string[] values,
            Tenum param,
            string default_value
        )
        {
            var index = Array.FindIndex(means, x => x == param.ToString());
            if (index < 0)
                return default_value;
            return values[index];
        }
    }
}
