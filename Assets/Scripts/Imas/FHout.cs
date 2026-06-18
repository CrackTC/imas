using System;

namespace Imas
{
    [Serializable]
    class FHout
    {
        public int t;
        public float v;
    }

    [Serializable]
    class FHoutText
    {
        public string TextId;
        public FHout[] List;
    }

    [Serializable]
    class FHoutTexts
    {
        public string Header;
        public FHoutText[] Body;
    }
}
