using System;
using System.Collections.Generic;
using UnityEngine;

namespace Imas
{
    static class PoseManager
    {
        static string s_pose_set_json_name = "chara_commu_pose_set";
        static List<string> s_pose_names;
        static Dictionary<string, string> s_pose_labels;

        static List<PoseVariation> bundles_cute;

        static List<PoseVariation> bundles_cool;

        static List<PoseVariation> bundles_passion;

        static List<PoseVariation> bundles_sit;

        static CharaCommuPoseSet.Internal.Data pose_set_data;

        /// <summary>
        /// 获取所有有效的pose名称
        /// </summary>
        static List<string> GetPoseNames()
        {
            if (s_pose_names != null)
                return s_pose_names;
            s_pose_names = new List<string>()
            {
                "p01",
                "p02",
                "p03",
                "p04",
                "p05",
                "p06",
                "p07",
                "p08",
                "p09",
                "a10",
                "p11",
                "a10_p11",
                "a12",
                "p13",
                "a12_p13",
                "a14",
                "p15",
                "a14_p15",
                "a16",
                "p17",
                "a16_p17",
                "p18_a",
                "p18_p",
                "p18_s",
                "p18_e01",
                "p18_e02",
                "p18_e03",
                "p18_e04",
                "a19",
                "a21",
                "p22",
                "a21_p22",
                "p24",
                "p28",
                "a29",
                "p30",
                "p33",
                "p37",
                "p38",
                "p39",
                "p41",
                "p51",
                "p81",
                "a82",
                "a83",
                "a84",
                "a85",
                "a86",
                "a87",
                "a88",
                "a89",
                "a90",
                "a91",
                "a92",
                "a93",
                "a94",
                "a95",
                "p97",
                "p98",
                "p99",
                "p100",
                "p101",
                "p102",
                "p103",
                "p104",
                "p105",
                "p106",
                "p107",
                "p108",
                "p109",
                "a110",
                "a111",
                "p112",
                "p113",
                "a114",
            };
            return s_pose_names;
        }

        /// <summary>
        /// 获取pose分类的短名称，用于bundle命名
        /// </summary>
        public static string GetCategoryShortName(CharacterCommuMotionCategory category) =>
            category switch
            {
                CharacterCommuMotionCategory.Cute => "cut",
                CharacterCommuMotionCategory.Cool => "col",
                CharacterCommuMotionCategory.Passion => "pas",
                CharacterCommuMotionCategory.Sit => "sit",
                // _ => "",
                _ => throw new ArgumentException("无效的pose分类"),
            };

        public static void Create()
        {
            bundles_cute = new()
            {
                new("p02", "a"),
                new("p03", "a"),
                new("p06", "a"),
                new("p07", "a"),
                new("p08", "a"),
                new("a10", "a"),
                new("a12", "a"),
                new("a14", "a"),
                new("a16", "a"),
                new("a21", "a"),
                new("p28", "a"),
                new("a29", "a"),
                new("p30", "a"),
                new("p33", "a"),
                new("p37", "a"),
                new("p38", "a"),
                new("p39", "a"),
                new("p41", "a"),
                new("p51", "a"),
                new("p81", "a"),
                new("a82", "a"),
                new("a83", "a"),
                new("a84", "a"),
                new("a85", "a"),
                new("a86", "a"),
                new("a87", "a"),
                new("a88", "a"),
                new("a89", "a"),
                new("a90", "a"),
                new("a91", "a"),
                new("a92", "a"),
                new("a93", "a"),
                new("a94", "a"),
                new("a95", "a"),
                new("p97", "a"),
                new("p98", "a"),
                new("p101", "a"),
                new("p102", "a"),
                new("p103", "a"),
                new("p104", "a"),
                new("p105", "a"),
                new("p106", "a"),
                new("p107", "a"),
                new("p108", "a"),
                new("p109", "a"),
                new("a110", "a"),
                new("a111", "a"),
                new("p112", "a"),
                new("p113", "a"),
                new("a114", "a"),
                new("p02", "b"),
                new("p04", "b"),
                new("p05", "b"),
                new("p06", "b"),
                new("p07", "b"),
                new("p08", "b"),
                new("p09", "b"),
                new("a10", "b"),
                new("a12", "b"),
                new("a14", "b"),
                new("a16", "b"),
                new("p28", "b"),
                new("p51", "b"),
                new("a114", "b"),
                new("p02", "c"),
                new("p03", "c"),
                new("p04", "c"),
                new("p05", "c"),
                new("p07", "c"),
                new("p08", "c"),
                new("p09", "c"),
                new("a10", "c"),
                new("a12", "c"),
                new("a14", "c"),
                new("p28", "c"),
                new("p51", "c"),
                new("p04", "d"),
                new("p05", "d"),
                new("a10", "d"),
                new("a16", "d"),
                new("p51", "d"),
            };

            bundles_cool = new List<PoseVariation>()
            {
                new("p02", "a"),
                new("p03", "a"),
                new("p05", "a"),
                new("p06", "a"),
                new("p07", "a"),
                new("p08", "a"),
                new("p09", "a"),
                new("a12", "a"),
                new("a14", "a"),
                new("a16", "a"),
                new("p28", "a"),
                new("a29", "a"),
                new("p30", "a"),
                new("p33", "a"),
                new("p37", "a"),
                new("p38", "a"),
                new("p39", "a"),
                new("p41", "a"),
                new("p51", "a"),
                new("p81", "a"),
                new("a82", "a"),
                new("a83", "a"),
                new("a84", "a"),
                new("a85", "a"),
                new("a86", "a"),
                new("a87", "a"),
                new("a88", "a"),
                new("a89", "a"),
                new("a90", "a"),
                new("a91", "a"),
                new("a92", "a"),
                new("a93", "a"),
                new("a94", "a"),
                new("a95", "a"),
                new("p97", "a"),
                new("p101", "a"),
                new("p102", "a"),
                new("p103", "a"),
                new("p104", "a"),
                new("p105", "a"),
                new("p106", "a"),
                new("p107", "a"),
                new("p108", "a"),
                new("p109", "a"),
                new("a110", "a"),
                new("a111", "a"),
                new("p112", "a"),
                new("p03", "b"),
                new("p04", "b"),
                new("p07", "b"),
                new("p08", "b"),
                new("p09", "b"),
                new("a10", "b"),
                new("a14", "b"),
                new("a16", "b"),
                new("p28", "b"),
                new("a29", "b"),
                new("p51", "b"),
                new("p02", "c"),
                new("p04", "c"),
                new("p05", "c"),
                new("p06", "c"),
                new("p09", "c"),
                new("a10", "c"),
                new("a12", "c"),
                new("a16", "c"),
                new("p28", "c"),
                new("p51", "c"),
                new("p04", "d"),
                new("p05", "d"),
                new("p06", "d"),
                new("p07", "d"),
                new("p08", "d"),
                new("a10", "d"),
                new("a12", "d"),
                new("a14", "d"),
                new("p51", "d"),
            };

            bundles_passion = new List<PoseVariation>()
            {
                new("p02", "a"),
                new("p03", "a"),
                new("p04", "a"),
                new("p07", "a"),
                new("p09", "a"),
                new("a10", "a"),
                new("a12", "a"),
                new("a21", "a"),
                new("p28", "a"),
                new("a29", "a"),
                new("p30", "a"),
                new("p33", "a"),
                new("p37", "a"),
                new("p38", "a"),
                new("p39", "a"),
                new("p41", "a"),
                new("p51", "a"),
                new("p81", "a"),
                new("a82", "a"),
                new("a83", "a"),
                new("a84", "a"),
                new("a85", "a"),
                new("a86", "a"),
                new("a87", "a"),
                new("a88", "a"),
                new("a89", "a"),
                new("a90", "a"),
                new("a91", "a"),
                new("a92", "a"),
                new("a93", "a"),
                new("a94", "a"),
                new("a95", "a"),
                new("p97", "a"),
                new("p99", "a"),
                new("p100", "a"),
                new("p101", "a"),
                new("p102", "a"),
                new("p103", "a"),
                new("p104", "a"),
                new("p105", "a"),
                new("p106", "a"),
                new("p107", "a"),
                new("p108", "a"),
                new("p109", "a"),
                new("a110", "a"),
                new("a111", "a"),
                new("p112", "a"),
                new("p02", "b"),
                new("p03", "b"),
                new("p04", "b"),
                new("p05", "b"),
                new("p06", "b"),
                new("p07", "b"),
                new("p08", "b"),
                new("a10", "b"),
                new("a12", "b"),
                new("a14", "b"),
                new("a16", "b"),
                new("p28", "b"),
                new("p51", "b"),
                new("p04", "c"),
                new("p05", "c"),
                new("p06", "c"),
                new("p08", "c"),
                new("p09", "c"),
                new("a14", "c"),
                new("a16", "c"),
                new("p28", "c"),
                new("p51", "c"),
                new("a114", "c"),
                new("p02", "d"),
                new("p05", "d"),
                new("p06", "d"),
                new("p07", "d"),
                new("p08", "d"),
                new("p09", "d"),
                new("a10", "d"),
                new("a12", "d"),
                new("a14", "d"),
                new("a16", "d"),
                new("p51", "d"),
            };

            for (var i = 0; i < 7; i++)
            {
                var item = new PoseVariation("a19", i.ToString());
                bundles_cute.Add(item);
                bundles_cool.Add(item);
                bundles_passion.Add(item);
            }

            bundles_sit = new List<PoseVariation>()
            {
                new("p02", "a"),
                new("p03", "a"),
                new("p04", "a"),
                new("p05", "a"),
                new("p06", "a"),
                new("p07", "a"),
                new("p08", "a"),
                new("p09", "a"),
                new("a10", "a"),
                new("a12", "a"),
                new("a14", "a"),
                new("a16", "a"),
                new("p24", "a"),
                new("p02", "b"),
                new("p03", "b"),
                new("p04", "b"),
                new("p05", "b"),
                new("p06", "b"),
                new("p07", "b"),
                new("p08", "b"),
                new("p09", "b"),
                new("a10", "b"),
                new("a12", "b"),
                new("a14", "b"),
                new("a16", "b"),
                new("p24", "b"),
                new("p02", "c"),
                new("p03", "c"),
                new("p04", "c"),
                new("p05", "c"),
                new("p06", "c"),
                new("p07", "c"),
                new("p08", "c"),
                new("p09", "c"),
                new("a10", "c"),
                new("a12", "c"),
                new("a14", "c"),
                new("a16", "c"),
            };
        }

        public static void Setup()
        {
            load_pose_set_data();
        }

        public static List<string> ListValidVariation(
            CharacterCommuMotionCategory category,
            string pose
        )
        {
            var list = new List<string>();
            var startIndex = pose.IndexOf("_");
            string left = pose;
            if (startIndex > -1)
                left = pose[..startIndex];

            if (
                int.TryParse(left[1..], out var leftNum)
                && (leftNum == 22 || (leftNum < 19 && leftNum % 2 == 1))
            )
            {
                left = "a" + (leftNum - 1).ToString("00");
            }

            if (get_pose_variations(category) is { } bundles)
            {
                foreach (var bundle in bundles)
                {
                    if (bundle.pose == left)
                    {
                        list.Add(bundle.variation);
                    }
                }
            }

            if (pose == "a21_p22" && category == CharacterCommuMotionCategory.Passion)
            {
                list.Add("a");
            }

            if (leftNum == 22 && category == CharacterCommuMotionCategory.Passion)
            {
                list.Add("a");
            }

            return list;
        }

        static List<PoseVariation> get_pose_variations(CharacterCommuMotionCategory category) =>
            category switch
            {
                CharacterCommuMotionCategory.Cute => bundles_cute,
                CharacterCommuMotionCategory.Cool => bundles_cool,
                CharacterCommuMotionCategory.Passion => bundles_passion,
                CharacterCommuMotionCategory.Sit => bundles_sit,
                _ => null,
            };

        public static string GetBundleName(
            CharacterCommuMotionCategory category,
            string variation,
            string pose
        )
        {
            if (
                pose != string.Empty
                && int.TryParse(pose[1..], out var poseNum)
                && (poseNum == 22 || (poseNum > 12 && poseNum < 19 && poseNum % 2 == 1))
            )
                pose = "a" + (poseNum - 1).ToString("00");

            var str = "chr_anim_com";
            if (category < CharacterCommuMotionCategory.Num)
            {
                str = "chr_anim_com_" + GetCategoryShortName(category);
            }

            if (variation != string.Empty)
            {
                str = $"{str}_{variation}";
                if (pose != string.Empty)
                    return $"{str}_{pose[1..]}.unity3d";
            }
            else if (pose != string.Empty)
            {
                return $"{str}_{pose}.unity3d";
            }
            return $"{str}.unity3d";
        }

        public static string GetBundleNameCostume(
            CharacterCommuMotionCategory category,
            string variation
        )
        {
            var str = "chr_anim_com";
            if (category < CharacterCommuMotionCategory.Num)
            {
                str = "chr_anim_com_" + GetCategoryShortName(category);
            }
            return $"{str}_19{variation}.unity3d";
        }

        /// <summary>
        /// 获取角色专用pose的bundle名称
        /// </summary>
        public static string GetBundleNamePrivate(string chara_id) =>
            $"chr_anim_com_{Character.RemoveV2FromCharacterID(chara_id)}.unity3d";

        /// <summary>
        /// p18开头的几个PoseInfo
        /// </summary>
        public static CharacterActorCommu.PrivatePoseInfo GetPrivatePoseInfo(string chara_id)
        {
            var etcs = new CharacterActorCommu.PrivatePoseEtcInfo[4];
            etcs[0] = new(CharacterActorCommu.PrivatePoseEtcType.OneShot, "01", "a_act", false);
            etcs[1] = new(CharacterActorCommu.PrivatePoseEtcType.OneShot, "02", "b_act", false);
            etcs[2] = new(CharacterActorCommu.PrivatePoseEtcType.Pose2, "03", "c", true);
            etcs[3] = new(CharacterActorCommu.PrivatePoseEtcType.Pose2, "04", "d", true);
            switch (chara_id)
            {
                case "010azu":
                case "014mir":
                case "015siz":
                case "016tsu":
                case "017kth":
                case "018ele":
                case "021mat":
                case "022ser":
                case "023aka":
                case "024ann_v2":
                case "025roc":
                case "026yur":
                case "027say":
                case "027say_v2":
                case "028ari":
                case "031tom":
                case "032emi":
                case "033sih":
                case "034ayu":
                case "035hin":
                case "036kan":
                case "038chz":
                case "039kon":
                case "041fuk":
                case "042miy":
                case "043nor":
                case "044miz":
                case "045kar":
                case "046rio":
                case "047sub":
                case "048rei":
                case "050jul":
                case "050jul_v2":
                    return new(false, true, false, true, etcs);
                case "001har":
                case "002chi":
                case "004yuk":
                case "006mak":
                case "008tak":
                case "009rit":
                case "011ami":
                case "012mam":
                case "052kao":
                    return new(true, true, true, true, etcs);
                case "003mik":
                case "019min":
                case "020meg":
                case "030iku":
                case "037nao":
                case "040tam":
                case "051tmg":
                    return new(true, false, false, true, etcs);
                case "005yay":
                    etcs[0] = new(CharacterActorCommu.PrivatePoseEtcType.Pose, "01", "a", false);
                    return new(true, true, true, true, etcs);
                case "007ior":
                case "013hib":
                case "029umi":
                    return new(false, true, false, false, etcs);
                case "049mom":
                    etcs[0] = new(
                        CharacterActorCommu.PrivatePoseEtcType.OneShot,
                        "01",
                        "a_str",
                        false
                    );
                    etcs[1] = new(
                        CharacterActorCommu.PrivatePoseEtcType.OneShot,
                        "02",
                        "a_end",
                        false
                    );
                    etcs[2] = new(
                        CharacterActorCommu.PrivatePoseEtcType.OneShot,
                        "03",
                        "b_str",
                        false
                    );
                    etcs[3] = new(
                        CharacterActorCommu.PrivatePoseEtcType.OneShot,
                        "04",
                        "b_end",
                        false
                    );
                    return new(false, false, false, false, etcs);
                case "101kot":
                case "102mis":
                    etcs[2] = new(
                        CharacterActorCommu.PrivatePoseEtcType.OneShot,
                        "03",
                        "c_act",
                        false
                    );
                    etcs[3] = new(
                        CharacterActorCommu.PrivatePoseEtcType.OneShot,
                        "04",
                        "d_act",
                        false
                    );
                    return new(true, true, true, true, etcs);
                case "201xxx":
                    return new(true, true, false, true, etcs);
                case "202xxx":
                    etcs[0] = new(CharacterActorCommu.PrivatePoseEtcType.Pose2, "01", "a", true);
                    return new(true, true, true, true, etcs);
                default:
                    return new(false, false, false, false, etcs);
            }
        }

        /// <summary>
        /// 从资源文件读取的角色的variation映射
        /// </summary>
        public static CharacterActorCommu.PoseSet GetPoseSet(string chara_id, bool is_sit)
        {
            if (!pose_set_data.IsValid())
                return null;

            var assign = Array.Find(
                pose_set_data.assigns.assigns,
                x =>
                    CharaCommuPoseSet.Internal.Utility.get_string_value(
                        x.means,
                        x.vals,
                        CharaCommuPoseSet.Internal.DataAssign.Param.CharacterId,
                        ""
                    ) == chara_id
            );

            if (!assign.IsValid())
                return null;

            var set_name = CharaCommuPoseSet.Internal.Utility.get_string_value(
                assign.means,
                assign.vals,
                CharaCommuPoseSet.Internal.DataAssign.Param.SetName,
                ""
            );

            if (is_sit)
            {
                set_name = CharaCommuPoseSet.Internal.Utility.get_string_value(
                    assign.means,
                    assign.vals,
                    CharaCommuPoseSet.Internal.DataAssign.Param.SetNameSit,
                    ""
                );
            }

            var set = Array.Find(
                pose_set_data.sets.sets,
                x =>
                    CharaCommuPoseSet.Internal.Utility.get_string_value(
                        x.means,
                        x.vals,
                        CharaCommuPoseSet.Internal.DataSet.Param.SetName,
                        ""
                    ) == set_name
            );

            if (!set.IsValid())
                return null;

            var categoryStr = CharaCommuPoseSet.Internal.Utility.get_string_value(
                set.means,
                set.vals,
                CharaCommuPoseSet.Internal.DataSet.Param.Category,
                ""
            );

            var result = new CharacterActorCommu.PoseSet
            {
                name = CharaCommuPoseSet.Internal.Utility.get_string_value(
                    set.means,
                    set.vals,
                    CharaCommuPoseSet.Internal.DataSet.Param.SetName,
                    ""
                ),
                category =
                    categoryStr == ""
                        ? CharacterCommuMotionCategory.None
                        : Enum.Parse<CharacterCommuMotionCategory>(categoryStr, true),
            };

            foreach (var pose_name in GetPoseNames())
            {
                var variation = CharaCommuPoseSet.Internal.Utility.get_string_value(
                    set.means,
                    set.vals,
                    pose_name,
                    ""
                );
                if (variation != "")
                    result.variations[pose_name] = variation;
            }

            return result;
        }

        static void load_pose_set_data()
        {
            var x = Resources.Load<TextAsset>(s_pose_set_json_name);
            if (x == null)
                throw new Exception("Failed to load pose set data");
            var json = x.text;
            var data = JsonUtility.FromJson<CharaCommuPoseSet.Internal.Data>(json);
            pose_set_data = data;

            for (var i = 0; i < pose_set_data.assigns.assigns.Length; i++)
                pose_set_data.assigns.assigns[i].means = pose_set_data.assigns.means;

            for (var i = 0; i < pose_set_data.sets.sets.Length; i++)
                pose_set_data.sets.sets[i].means = pose_set_data.sets.means;
        }

        struct PoseVariation
        {
            public string pose;

            public string variation;

            public PoseVariation(string pose, string variation)
            {
                this.pose = pose;
                this.variation = variation;
            }
        }
    }
}
