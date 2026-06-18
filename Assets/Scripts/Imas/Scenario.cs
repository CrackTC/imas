using System;

namespace Imas
{
    [Serializable]
    class Scenario
    {
        public ScenarioHeader header;
        public ScenarioDatas datas;
    }

    [Serializable]
    class ScenarioHeader
    {
        public string title;
    }

    [Serializable]
    class ScenarioDatas
    {
        public Sequence[] CutRecord;
        public Sequence[] Scenario;
    }

    [Serializable]
    class Sequence
    {
        /*0x10*/public int seq_id;

        /*0x18*/public string command;

        /*0x20*/public string arg1;

        /*0x28*/public string arg2;

        /*0x30*/public string arg3;

        /*0x38*/public string arg4;

        /*0x40*/public string arg5;

        /*0x48*/public string arg6;

        /*0x50*/public string arg7;

        /*0x58*/public string arg8;

        /*0x60*/public string arg9;

        /*0x68*/public string arg10;

        /*0x70*/public string arg11;

        /*0x78*/public string arg12;

        /*0x80*/public string arg13;

        /*0x88*/public string arg14;

        /*0x90*/public string arg15;

        /*0x98*/public string arg16;
    }
}
