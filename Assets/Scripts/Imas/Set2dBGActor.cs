using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Imas
{
    class Set2dBGActor : Actor
    {
        readonly string bgid;
        readonly bool IsBGM;
        readonly bool IsAmbient;
        readonly bool IsLight;

        Sprite _MainSprite;
        Sprite _SubSprite;

        public Set2dBGActor(Sequence sequence)
        {
            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            bgid = sequence.arg5;
            IsBGM = sequence.arg6 == "on";
            IsAmbient = sequence.arg7 == "on";
            IsLight = sequence.arg8 == "on";
        }

        public override void Init()
        {
            ScenarioManager.Instance.StartCoroutine(
                AssetBundleManager.LoadAssetBundle(
                    Path.Combine("bg2d", bgid + ".png.unity3d"),
                    ab =>
                    {
                        var names = ab.GetAllAssetNames();
                        string mainName = null;
                        string subName = null;
                        foreach (var name in names)
                        {
                            if (Regex.IsMatch(name, @"bg2d_[a-z]\d{4}_0"))
                                mainName = name;
                            else if (Regex.IsMatch(name, @"bg2d_[a-z]\d{4}_1"))
                                subName = name;
                        }

                        _MainSprite = ab.LoadAsset<Sprite>(mainName);
                        _SubSprite = ab.LoadAsset<Sprite>(subName);
                    }
                )
            );
        }

        public override void Exec()
        {
            var bg2d = GameObject.FindWithTag("bg2d");
            var mainImage = bg2d.transform.Find("main").GetComponent<Image>();
            var subImage = bg2d.transform.Find("sub").GetComponent<Image>();
            mainImage.sprite = _MainSprite;
            subImage.sprite = _SubSprite;
        }
    }
}
