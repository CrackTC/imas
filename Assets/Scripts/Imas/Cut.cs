using System;
using System.Collections.Generic;

namespace Imas
{
    class Cut
    {
        public string id;
        public int frameCount;
        public CutType CutType;
        public List<Actor> Actors = new();

        public Cut(Sequence sequence)
        {
            id = sequence.arg1;
            frameCount = int.Parse(sequence.arg5);
            CutType = sequence.arg6 switch
            {
                "l" => CutType.LOAD,
                "n" => CutType.NORMAL,
                _ => throw new NotImplementedException($"Unknown cut type: {sequence.arg6}"),
            };
        }
    }

    enum CutType
    {
        AUTO = 0,
        NORMAL = 1,
        LOAD = 2,
        SHORT = 3,
        NOTDEFINED = 4,
    }
}
