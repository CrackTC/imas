using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Imas
{
    class CharacterActorCommu
    {
        const string s_idle_pose_name = "p01";
        const string s_costume_pose_name = "a19";
        const string s_birth_costume_pose_name_pattern = @"^a(?<id>6\d)$";
        const string s_idle_state_name = "p01";
        const string s_private_pose_name = "p18";
        const string s_private_state_name = "priv00";
        const string s_idle_pose_name_volley = "p81";
        const string s_idle_state_name_volley = "p81.loop";
        const string s_idle_pose_name_salmon = "p101";
        const string s_idle_state_name_salmon = "p101.loop";

        StepYesNo step_yes_no;

        StepComeIn step_come_in;

        StepGoOut step_go_out;

        /*0x1a8*/string current_pose;

        /*0x1b0*/bool is_pose_changing;

        /*0x1c0*/string pose_set_name;

        /*0x1c8*/
        public PoseSet pose_set;

        /*0x1d8*/
        UniqueMotionSet umset;

        /*0x1e0*/bool req_skip;

        /*0x1e1*/bool ignore_pose_changing;

        /*0x1e2*/
        QueuedYesNo queued_yes_no;

        /*0x1e8*/
        PlayAnimationRequest play_anim_req;

        /*0x200*/
        Dictionary<string, PoseInfo> pose_infos;

        Character _Character;

        static bool IsPrivatePose(string pose_name) => pose_name == s_private_pose_name;

        static string GetPoseAssociateIdlePoseName(string pose_name) =>
            pose_name switch
            {
                "p81"
                or "a82"
                or "a83"
                or "a84"
                or "a85"
                or "a86"
                or "a87"
                or "a88"
                or "a89"
                or "a90" => "p81",
                "a110" or "a111" => "p101",
                _ => "p01",
            };

        public CharacterActorCommu(Character character)
        {
            step_yes_no = new StepYesNo() { is_yes = true };
            step_come_in = new StepComeIn();
            step_go_out = new StepGoOut();
            current_pose = "p01";
            pose_set_name = "all_a";
            pose_infos = new Dictionary<string, PoseInfo>();
            // umset = new UniqueMotionSet(this);
            queued_yes_no = new QueuedYesNo() { is_requested = false, is_yes = true };
            _Character = character;
        }

        string GetPoseSetName() => pose_set.name;

        void ForceIdlePose() => throw new NotImplementedException();

        UniqueMotionSet GetUniqueMotionSet() => throw new NotImplementedException();

        void SetIgnorePoseChanging(bool is_ignore) => throw new NotImplementedException();

        void log_error(string mes) => throw new NotImplementedException();

        void log_mes(string mes) => throw new NotImplementedException();

        void OnAnimatorAttached() => throw new NotImplementedException();

        void on_create_entity() => throw new NotImplementedException();

        void create_unique_motion_set(string umset_name) => throw new NotImplementedException();

        void execute_entity(float delta_time) => throw new NotImplementedException();

        bool IsOmitResetSwayPose(string pose_name) => throw new NotImplementedException();

        bool IsOmitResetSwayState(string state_name) => throw new NotImplementedException();

        bool execute_entity_sub(float delta_time) => throw new NotImplementedException();

        public void PlayYes()
        {
            var animator = _Character.Animator;

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("p01"))
            {
                animator.CrossFadeInFixedTime("p01_yes", 0.1f);
            }
            else
            {
                var index = play_anim_req.state.LastIndexOf('.');
                animator.CrossFadeInFixedTime(play_anim_req.state[..index] + ".yes", 0.1f);
            }
        }

        public void PlayNo()
        {
            var animator = _Character.Animator;
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("p01"))
            {
                animator.CrossFadeInFixedTime("p01_no", 0.1f);
            }
            else
            {
                var index = play_anim_req.state.LastIndexOf('.');
                animator.CrossFadeInFixedTime(play_anim_req.state[..index] + ".no", 0.1f);
            }
        }

        public void step_pose_execute(float delta_time, string nextPoseId, bool isDirect)
        {
            var animator = _Character.Animator;
            var inTransition = animator.IsInTransition(0);
            var currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!inTransition)
            {
                if (current_pose != "p01" && currentStateInfo.IsName("p01"))
                    current_pose = "p01";
                if (current_pose != "p81" && currentStateInfo.IsName("p81.loop"))
                    current_pose = "p81";
                if (current_pose != "p101" && currentStateInfo.IsName("p101.loop"))
                    current_pose = "p101";
            }

            var poseInfo = pose_infos[current_pose];

            string stateName;
            if (current_pose == "p01")
            {
                is_pose_changing = true;
                if (!inTransition)
                {
                    stateName = "p01";
                    if (currentStateInfo.IsName("p01"))
                    {
                        is_pose_changing = false;
                    }
                }
            }
            else
            {
                if (current_pose == "p101")
                {
                    is_pose_changing = true;
                    if (!inTransition)
                    {
                        stateName = "p101.loop";
                        if (currentStateInfo.IsName("p101.loop"))
                        {
                            is_pose_changing = false;
                        }
                    }
                }
                else
                {
                    if (!poseInfo.is_private)
                    {
                        if ((!poseInfo.is_action) && (!poseInfo.has_loop))
                        {
                            stateName = poseInfo.id + ".loop";
                            check_pose_changing_loop(stateName);
                        }
                        else
                        {
                            is_pose_changing = true;
                        }
                    }
                    else
                    {
                        switch (poseInfo.private_pose_type)
                        {
                            case PrivatePoseType.Pose:
                            case PrivatePoseType.Switch:
                                check_pose_changing_loop("priv00.loop");
                                break;
                            case PrivatePoseType.Etc:
                                switch (poseInfo.private_pose_etc_info.type)
                                {
                                    case PrivatePoseEtcType.OneShot:
                                        is_pose_changing = true;
                                        if (poseInfo.private_pose_etc_info.terminate_motion)
                                        {
                                            throw new NotImplementedException();
                                        }
                                        break;
                                    default:
                                        stateName =
                                            $"priv_etc{poseInfo.private_pose_etc_info.state_name}.loop";
                                        check_pose_changing_loop(stateName);
                                        break;
                                }
                                break;
                            case PrivatePoseType.Action:
                                is_pose_changing = true;
                                break;
                        }
                    }
                }
            }

            if (play_anim_req.state == string.Empty && is_pose_changing)
            {
                step_pose_execute_private_etc(delta_time);
                step_pose_execute_volley_salmon();
                return;
            }

            is_pose_changing = false;

            if (nextPoseId == null)
            {
                if (!poseInfo.has_loop)
                    return;

                if (
                    poseInfo.is_private
                    && poseInfo.private_pose_type == PrivatePoseType.Etc
                    && poseInfo.private_pose_etc_info.type == PrivatePoseEtcType.OneShot
                )
                    return;

                throw new NotImplementedException();

                // if (IsNowCurrentAnimationLooped)
                // {
                //     LoopCurrentAnimation();
                // }

                // return false;
            }

            var nextPoseInfo = pose_infos[nextPoseId];
            stateName = string.Empty;

            if (nextPoseId == "p01")
            {
                if (!poseInfo.is_private)
                {
                    if (current_pose == "p01")
                    {
                        stateName = "p01";
                        if (play_anim_req.state == string.Empty)
                        {
                            stateName = string.Empty;
                        }
                    }
                    else
                    {
                        stateName = "p01";
                        if (!isDirect)
                        {
                            stateName = current_pose + ".end";
                        }
                    }
                }
                else
                {
                    stateName = "p01";
                    if (!isDirect)
                    {
                        if (poseInfo.private_pose_type != PrivatePoseType.Etc)
                        {
                            stateName = "priv00.end";
                        }
                        else if (poseInfo.private_pose_etc_info.type == PrivatePoseEtcType.Pose)
                        {
                            stateName =
                                $"priv_etc{poseInfo.private_pose_etc_info.state_name}.action";
                        }
                        else if (poseInfo.private_pose_etc_info.type != PrivatePoseEtcType.OneShot)
                        {
                            stateName = $"priv_etc{poseInfo.private_pose_etc_info.state_name}.end";
                        }
                    }
                }
            }
            else if (nextPoseId == "p101")
            {
                if (current_pose == "p101")
                {
                    if (play_anim_req.state != string.Empty)
                    {
                        stateName = "p101.loop";
                    }
                    else
                    {
                        stateName = string.Empty;
                    }
                }
                else
                {
                    stateName = "p101.loop";
                    if (!isDirect)
                    {
                        if (current_pose != "p01")
                        {
                            stateName = current_pose + ".end";
                        }
                        else
                        {
                            stateName = "p101.start";
                        }
                    }
                }
            }
            else if (!nextPoseInfo.is_private)
            {
                if (nextPoseInfo.switch_to != string.Empty)
                {
                    stateName = nextPoseInfo.switch_to;
                    if (!isDirect)
                    {
                        stateName += ".switch";
                    }
                    else
                    {
                        stateName += ".loop";
                    }
                }
                else
                {
                    if (!nextPoseInfo.is_action)
                    {
                        stateName = nextPoseInfo.id;
                        if (nextPoseInfo.group == PoseGroup.Volley)
                        {
                            stateName += ".loop";
                        }
                        else
                        {
                            if (stateName != "p101")
                            {
                                if (!isDirect)
                                {
                                    stateName += ".start";
                                }
                                else
                                {
                                    stateName += ".loop";
                                }
                            }
                            else
                            {
                                stateName += ".loop";
                            }
                        }
                    }
                    else
                    {
                        if (!isDirect)
                        {
                            stateName = nextPoseInfo.id + ".action";
                        }
                        else
                        {
                            if (nextPoseInfo.group == PoseGroup.Salmon)
                            {
                                stateName = "p101.loop";
                            }
                            else
                            {
                                stateName = "p01";
                                if (nextPoseInfo.group == PoseGroup.Volley)
                                {
                                    stateName = "p81.loop";
                                }
                            }

                            take_over_posture_from_state_end(nextPoseInfo.id + ".action");
                        }
                    }
                }
            }

            switch (nextPoseInfo.private_pose_type)
            {
                case PrivatePoseType.Action:
                    if (isDirect)
                    {
                        stateName = "p01";
                    }
                    else
                    {
                        stateName = "priv00.action";
                    }
                    break;
                case PrivatePoseType.Pose:
                    if (isDirect)
                    {
                        stateName = "priv00.loop";
                    }
                    else
                    {
                        stateName = "priv00.start";
                    }
                    break;
                case PrivatePoseType.Switch:
                    if (isDirect)
                    {
                        stateName = nextPoseInfo.switch_to + ".loop";
                    }
                    else
                    {
                        stateName = nextPoseInfo.switch_to + ".switch";
                    }
                    break;
                case PrivatePoseType.Etc:
                    switch (nextPoseInfo.private_pose_etc_info.type)
                    {
                        case PrivatePoseEtcType.Pose:
                        case PrivatePoseEtcType.Pose2:
                            if (isDirect)
                            {
                                stateName =
                                    $"priv_etc{nextPoseInfo.private_pose_etc_info.state_name}.loop";
                            }
                            else
                            {
                                stateName =
                                    $"priv_etc{nextPoseInfo.private_pose_etc_info.state_name}.start";
                            }
                            break;
                        case PrivatePoseEtcType.Switch:
                            if (isDirect)
                            {
                                stateName =
                                    $"priv_etc{nextPoseInfo.private_pose_etc_info.state_name}.loop";
                            }
                            else
                            {
                                stateName =
                                    $"priv_etc{nextPoseInfo.private_pose_etc_info.state_name}.switch";
                            }
                            break;
                        case PrivatePoseEtcType.OneShot:
                            stateName =
                                $"priv_etc{nextPoseInfo.private_pose_etc_info.state_name}.loop";
                            if (isDirect)
                            {
                                throw new NotImplementedException();
                                // AnimationClip animationAtState = GetAnimationAtState(stateName);
                                // var len = animationAtState.length;
                                // animator.PlayInFixedTime(stateName, 0, len);
                                // animator.Update(0.0f);
                                // take_over_posture();
                                // stateName = "p01";
                                // nextPoseInfo.id = "p01";
                            }
                            break;
                    }
                    break;
            }

            if (stateName != string.Empty)
            {
                if (animator.HasState(0, Animator.StringToHash(stateName)))
                {
                    if (!nextPoseInfo.is_switch())
                    {
                        current_pose = nextPoseInfo.id;
                    }
                    else
                    {
                        current_pose = nextPoseInfo.switch_to;
                    }
                    var len = 0.0f;
                    if (isDirect == false)
                        len = 0.1f;
                    if (current_pose is "a89" or "a90" or "a94" or "a95")
                        len = 0.0f;
                    if (poseInfo.group != nextPoseInfo.group)
                        len = 0.0f;
                    play_anim_req.state = stateName;
                    play_anim_req.start_time = 0.0f;
                    play_anim_req.fade_duration = len;
                    play_anim_req.is_loop_direct = isDirect;
                    if (isDirect)
                        is_pose_changing = false;
                    else
                        is_pose_changing = true;

                    animator.CrossFadeInFixedTime(stateName, len);
                }
            }
            else if (!isDirect)
            {
                check_force_loop(poseInfo);
            }
        }

        void check_pose_changing_loop(string loop_state_name)
        {
            var animator = _Character.Animator;
            if (!animator.IsInTransition(0))
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(loop_state_name))
                {
                    animator.Play(loop_state_name);
                    is_pose_changing = false;
                    return;
                }
            }

            is_pose_changing = true;
        }

        void step_pose_execute_private_etc(float delta_time) => throw new NotImplementedException();

        void step_pose_execute_volley_salmon() => throw new NotImplementedException();

        void step_pose_skip() => throw new NotImplementedException();

        void take_over_posture_from_state_end(string state_name) =>
            throw new NotImplementedException();

        void take_over_posture() => throw new NotImplementedException();

        void take_over_height(bool xy_reset) => throw new NotImplementedException();

        void force_reset_to_idle_pose() => throw new NotImplementedException();

        string get_loop_state_name(PoseInfo pinfo) => throw new NotImplementedException();

        PoseSet _GetPoseSet() => pose_set;

        bool check_will_finish_pose_changing() => throw new NotImplementedException();

        void check_force_loop(PoseInfo pinfo) => throw new NotImplementedException();

        void set_pose_table(PoseSet pose_set)
        {
            this.pose_set = pose_set;
            pose_set_name = pose_set.name;
            set_pose_table_beta(pose_set);
        }

        void insert_event_to_clip(AnimationClip clip)
        {
            if (clip.events.Length > 0)
                return;

            clip.AddEvent(
                new AnimationEvent()
                {
                    time = clip.length,
                    functionName = "_AnimEvent_EndFrame",
                    objectReferenceParameter = clip,
                }
            );
        }

        void create_pose_infos(string attrib, PoseSet pose_set)
        {
            pose_infos.Clear();
            pose_infos["p01"] = PoseInfo.CreatePose("p01", true);

            if (!pose_set.variations.ContainsKey("p02"))
            {
                pose_infos["p02"] = PoseInfo.CreatePose("p02", false);
            }
            else
            {
                var vari = pose_set.variations["p02"];
                switch (attrib)
                {
                    case "cut" when vari is "c":
                    case "col" when vari is "a":
                    case "pas" when vari is "a" or "b" or "d":
                    case "sit" when vari is "a" or "b" or "c":
                        pose_infos["p02"] = PoseInfo.CreateAction("p02");
                        break;
                    default:
                        pose_infos["p02"] = PoseInfo.CreatePose("p02", false);
                        break;
                }
            }

            pose_infos["p03"] = PoseInfo.CreatePose("p03", true);
            pose_infos["p04"] = PoseInfo.CreatePose("p04", false);
            pose_infos["p05"] = PoseInfo.CreatePose("p05", true);
            pose_infos["p06"] = PoseInfo.CreatePose("p06", true);
            pose_infos["p07"] = PoseInfo.CreatePose("p07", true);
            pose_infos["p08"] = PoseInfo.CreatePose("p08", true);
            pose_infos["p09"] = PoseInfo.CreatePose("p09", true);
            pose_infos["a10"] = PoseInfo.CreateAction("a10");
            pose_infos["p11"] = PoseInfo.CreatePose("p11", true);
            pose_infos["a12"] = PoseInfo.CreateAction("a12");
            pose_infos["p13"] = PoseInfo.CreatePose("p13", true);
            pose_infos["a14"] = PoseInfo.CreateAction("a14");
            pose_infos["p15"] = PoseInfo.CreatePose("p15", true);
            pose_infos["a16"] = PoseInfo.CreateAction("a16");
            pose_infos["p17"] = PoseInfo.CreatePose("p17", true);

            if (attrib != "sit")
                pose_infos["a19"] = PoseInfo.CreateAction("a19");

            pose_infos["a21"] = PoseInfo.CreateAction("a21");
            pose_infos["p22"] = PoseInfo.CreatePose("p22", true);
            pose_infos["a10_p11"] = PoseInfo.CreateSwitch("a10", "p11");
            pose_infos["a12_p13"] = PoseInfo.CreateSwitch("a12", "p13");
            pose_infos["a14_p15"] = PoseInfo.CreateSwitch("a14", "p15");
            pose_infos["a16_p17"] = PoseInfo.CreateSwitch("a16", "p17");
            pose_infos["a21_p22"] = PoseInfo.CreateSwitch("a21", "p22");

            if (attrib != "sit")
            {
                var chrId = _Character.CharacterID;
                var privPoseInfo = PoseManager.GetPrivatePoseInfo(chrId);
                if (privPoseInfo.is_has_action)
                {
                    pose_infos["p18_a"] = PoseInfo.CreatePrivateAction("p18_a");
                }
                if (privPoseInfo.is_has_pose)
                {
                    pose_infos["p18_p"] = PoseInfo.CreatePrivatePose(
                        "p18_p",
                        privPoseInfo.is_has_yes_no
                    );
                }
                if (privPoseInfo.is_has_switch)
                {
                    pose_infos["p18_s"] = PoseInfo.CreatePrivateSwitch("p18_s");
                }

                for (int i = 0; i < privPoseInfo.etcs.Length; i++)
                {
                    var name = $"p18_e{i + 1}";
                    pose_infos[name] = PoseInfo.CreatePrivateEtc(name, privPoseInfo.etcs[i]);
                }
            }

            pose_infos["p24"] = PoseInfo.CreatePose("p24", false);
            pose_infos["p28"] = PoseInfo.CreatePose("p28", true);
            pose_infos["a29"] = PoseInfo.CreateAction("a29");
            pose_infos["p30"] = PoseInfo.CreatePose("p30", false);
            pose_infos["p33"] = PoseInfo.CreatePose("p33", true);
            pose_infos["p37"] = PoseInfo.CreatePose("p37", false);
            pose_infos["p38"] = PoseInfo.CreatePose("p38", false);
            pose_infos["p39"] = PoseInfo.CreatePose("p39", false);
            pose_infos["p41"] = PoseInfo.CreatePose("p41", true);
            pose_infos["p51"] = PoseInfo.CreatePose("p51", false);
            pose_infos["p81"] = PoseInfo.CreatePose("p81", true);
            pose_infos["p81"].group = PoseGroup.Volley;
            pose_infos["a82"] = PoseInfo.CreateAction("a82");
            pose_infos["a82"].group = PoseGroup.Volley;
            pose_infos["a83"] = PoseInfo.CreateAction("a83");
            pose_infos["a83"].group = PoseGroup.Volley;
            pose_infos["a84"] = PoseInfo.CreateAction("a84");
            pose_infos["a84"].group = PoseGroup.Volley;
            pose_infos["a85"] = PoseInfo.CreateAction("a85");
            pose_infos["a85"].group = PoseGroup.Volley;
            pose_infos["a86"] = PoseInfo.CreateAction("a86");
            pose_infos["a86"].group = PoseGroup.Volley;
            pose_infos["a87"] = PoseInfo.CreateAction("a87");
            pose_infos["a87"].group = PoseGroup.Volley;
            pose_infos["a88"] = PoseInfo.CreateAction("a88");
            pose_infos["a88"].group = PoseGroup.Volley;
            pose_infos["a89"] = PoseInfo.CreateAction("a89");
            pose_infos["a89"].group = PoseGroup.Volley;
            pose_infos["a90"] = PoseInfo.CreateAction("a90");
            pose_infos["a90"].group = PoseGroup.Volley;
            pose_infos["a91"] = PoseInfo.CreateAction("a91");
            pose_infos["a91"].group = PoseGroup.Volley;
            pose_infos["a92"] = PoseInfo.CreateAction("a92");
            pose_infos["a92"].group = PoseGroup.Volley;
            pose_infos["a93"] = PoseInfo.CreateAction("a93");
            pose_infos["a93"].group = PoseGroup.Volley;
            pose_infos["a94"] = PoseInfo.CreateAction("a94");
            pose_infos["a94"].group = PoseGroup.Volley;
            pose_infos["a95"] = PoseInfo.CreateAction("a95");
            pose_infos["a95"].group = PoseGroup.Volley;
            pose_infos["p97"] = PoseInfo.CreatePose("p97", true);
            pose_infos["p98"] = PoseInfo.CreatePose("p98", true);
            pose_infos["p99"] = PoseInfo.CreatePose("p99", false);
            pose_infos["p100"] = PoseInfo.CreatePose("p100", false);
            pose_infos["p101"] = PoseInfo.CreatePose("p101", true);
            pose_infos["p101"].group = PoseGroup.Salmon;
            pose_infos["p102"] = PoseInfo.CreatePose("p102", true);
            pose_infos["p102"].group = PoseGroup.Salmon;
            pose_infos["p103"] = PoseInfo.CreatePose("p103", true);
            pose_infos["p103"].group = PoseGroup.Salmon;
            pose_infos["p104"] = PoseInfo.CreatePose("p104", false);
            pose_infos["p104"].group = PoseGroup.Salmon;
            pose_infos["p105"] = PoseInfo.CreatePose("p105", false);
            pose_infos["p105"].group = PoseGroup.Salmon;
            pose_infos["p106"] = PoseInfo.CreatePose("p106", false);
            pose_infos["p106"].group = PoseGroup.Salmon;
            pose_infos["p107"] = PoseInfo.CreatePose("p107", true);
            pose_infos["p107"].group = PoseGroup.Salmon;
            pose_infos["p108"] = PoseInfo.CreatePose("p108", true);
            pose_infos["p108"].group = PoseGroup.Salmon;
            pose_infos["p109"] = PoseInfo.CreatePose("p109", true);
            pose_infos["p109"].group = PoseGroup.Salmon;
            pose_infos["a110"] = PoseInfo.CreateAction("a110");
            pose_infos["a110"].group = PoseGroup.Salmon;
            pose_infos["a111"] = PoseInfo.CreateAction("a111");
            pose_infos["a111"].group = PoseGroup.Salmon;
            pose_infos["p112"] = PoseInfo.CreatePose("p112", false);
            pose_infos["p112"].group = PoseGroup.Salmon;
            pose_infos["p113"] = PoseInfo.CreatePose("p113", false);
            pose_infos["a114"] = PoseInfo.CreateAction("a114");
            pose_infos["a114"].group = PoseGroup.Salmon;
        }

        void set_pose_table_beta(PoseSet pose_set)
        {
            create_pose_infos(PoseManager.GetCategoryShortName(pose_set.category), pose_set);

            foreach (var (key, value) in pose_infos)
            {
                if (!is_pose_required(value))
                    continue;

                value.bundle_pose = key;
                value.bundle_vari = string.Empty;
                value.is_valid = true;

                if (key == "p01")
                {
                    value.bundle_vari = "z";
                    continue;
                }

                if (key.StartsWith("p18"))
                    continue;

                if (key == "a19")
                {
                    if (pose_set.variations.ContainsKey(value.id))
                        value.bundle_vari = pose_set.variations[value.id];

                    continue;
                }

                if (value.is_switch())
                {
                    if (pose_set.variations.ContainsKey(value.switch_from))
                    {
                        value.bundle_vari = pose_set.variations[value.switch_from];
                        value.bundle_pose = value.switch_from;
                    }
                }
                else
                {
                    if (pose_set.variations.ContainsKey(value.id))
                    {
                        value.bundle_vari = pose_set.variations[value.id];
                    }
                    else
                    {
                        var name = "a" + (value.index - 1).ToString("00");
                        if (pose_set.variations.ContainsKey(name))
                        {
                            value.bundle_vari = pose_set.variations[name];
                        }
                    }
                }

                if (
                    value.bundle_vari == string.Empty
                    && !PoseManager
                        .ListValidVariation(pose_set.category, value.bundle_pose)
                        .Contains(value.bundle_vari)
                )
                {
                    value.is_valid = false;
                }

                if (
                    pose_set.category == CharacterCommuMotionCategory.Passion
                    && value.bundle_vari == "a"
                    && (key == "a21_p22" || key == "p22")
                )
                {
                    value.is_valid = false;
                }
            }
        }

        /// <summary>
        /// 在收集完需要的pose后调用，将对应的动画clip加载到override controller上
        /// </summary>
        public IEnumerator OverridePoseClip(bool is_sit)
        {
            _GetPoseSetRequiredBundles(PoseManager.GetPoseSet(_Character.CharacterID, is_sit));
            return override_pose_clip(pose_set);
        }

        private IEnumerator override_pose_clip(PoseSet pose_set)
        {
            // if (umset.IsValid())
            //     umset.OnBundleLoaded();

            var category = pose_set.category;
            var category_short_name = PoseManager.GetCategoryShortName(category);

            var pending = 0;

            foreach (var (key, value) in pose_infos)
            {
                if (value.is_valid == false)
                    continue;

                string name;
                if (value.is_private == false)
                {
                    if (key == s_idle_pose_name)
                    {
                        name = PoseManager.GetBundleName(category, "z", string.Empty);
                    }
                    else if (key == s_costume_pose_name)
                    {
                        name = PoseManager.GetBundleNameCostume(category, value.bundle_vari);
                    }
                    else
                    {
                        name = PoseManager.GetBundleName(
                            category,
                            value.bundle_vari,
                            value.bundle_pose
                        );
                    }
                }
                else
                {
                    name = PoseManager.GetBundleNamePrivate(_Character.CharacterID);
                }

                pending++;
                _Character.StartCoroutine(
                    AssetBundleManager.LoadAssetBundle(
                        Path.Combine(
                            "com_anim",
                            Path.GetFileNameWithoutExtension(name) + ".reduc.unity3d"
                        ),
                        bundle =>
                        {
                            pending--;
                            if (bundle == null)
                            {
                                value.is_valid = false;
                                return;
                            }

                            if (value.is_private == false)
                            {
                                if (key == s_idle_pose_name)
                                {
                                    set_clip_to_over_ctrl(
                                        key,
                                        string.Empty,
                                        $"com_{category_short_name}_01z_lop",
                                        bundle
                                    );

                                    if (value.has_yes_no)
                                    {
                                        set_clip_to_over_ctrl(
                                            key,
                                            "yes",
                                            $"com_{category_short_name}_01z_yes",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            key,
                                            "no",
                                            $"com_{category_short_name}_01z_noo",
                                            bundle
                                        );
                                    }
                                }
                                else
                                {
                                    if (value.is_switch())
                                    {
                                        set_clip_to_over_ctrl(
                                            value.switch_to,
                                            "switch",
                                            $"com_{category_short_name}_{value.switch_from[1..]}{value.bundle_vari}_swt",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            value.switch_to,
                                            "loop",
                                            $"com_{category_short_name}_{value.switch_to[1..]}{value.bundle_vari}_lop",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            value.switch_to,
                                            "end",
                                            $"com_{category_short_name}_{value.switch_to[1..]}{value.bundle_vari}_end",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            value.switch_to,
                                            "yes",
                                            $"com_{category_short_name}_{value.switch_to[1..]}{value.bundle_vari}_yes",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            value.switch_to,
                                            "no",
                                            $"com_{category_short_name}_{value.switch_to[1..]}{value.bundle_vari}_noo",
                                            bundle
                                        );
                                    }
                                    else if (value.is_action == false)
                                    {
                                        set_clip_to_over_ctrl(
                                            key,
                                            "start",
                                            $"com_{category_short_name}_{value.number}{value.bundle_vari}_str",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            key,
                                            "end",
                                            $"com_{category_short_name}_{value.number}{value.bundle_vari}_end",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            key,
                                            "loop",
                                            $"com_{category_short_name}_{value.number}{value.bundle_vari}_lop",
                                            bundle
                                        );
                                    }
                                    else
                                    {
                                        set_clip_to_over_ctrl(
                                            key,
                                            "action",
                                            $"com_{category_short_name}_{value.number}{value.bundle_vari}_act",
                                            bundle
                                        );
                                    }

                                    if (value.has_yes_no)
                                    {
                                        set_clip_to_over_ctrl(
                                            key,
                                            "yes",
                                            $"com_{category_short_name}_{value.number}{value.bundle_vari}_yes",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            key,
                                            "no",
                                            $"com_{category_short_name}_{value.number}{value.bundle_vari}_noo",
                                            bundle
                                        );
                                    }
                                }
                            }
                            else
                            {
                                switch (value.private_pose_type)
                                {
                                    case PrivatePoseType.Action:
                                        set_clip_to_over_ctrl(
                                            s_private_state_name,
                                            "action",
                                            $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_act",
                                            bundle
                                        );
                                        break;
                                    case PrivatePoseType.Pose:
                                        set_clip_to_over_ctrl(
                                            s_private_state_name,
                                            "start",
                                            $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_str",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            s_private_state_name,
                                            "end",
                                            $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_end",
                                            bundle
                                        );
                                        set_clip_to_over_ctrl(
                                            s_private_state_name,
                                            "loop",
                                            $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_lop",
                                            bundle
                                        );

                                        if (value.has_yes_no)
                                        {
                                            set_clip_to_over_ctrl(
                                                s_private_state_name,
                                                "yes",
                                                $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_yes",
                                                bundle
                                            );
                                            set_clip_to_over_ctrl(
                                                s_private_state_name,
                                                "no",
                                                $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_noo",
                                                bundle
                                            );
                                        }
                                        break;
                                    case PrivatePoseType.Switch:
                                        set_clip_to_over_ctrl(
                                            s_private_state_name,
                                            "switch",
                                            $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18y_swt",
                                            bundle
                                        );
                                        break;
                                    case PrivatePoseType.Etc:
                                        throw new NotImplementedException();
                                    // if (value.private_pose_etc_info.type != PrivatePoseEtcType.OneShot)
                                    // {
                                    //     set_clip_to_over_ctrl(
                                    //         "priv_etc" + value.private_pose_etc_info.state_name,
                                    //         "start",
                                    //         $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}_str",
                                    //         bundle
                                    //     );
                                    //     set_clip_to_over_ctrl(
                                    //         "priv_etc" + value.private_pose_etc_info.state_name,
                                    //         "loop",
                                    //         $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}_lop",
                                    //         bundle
                                    //     );
                                    //     set_clip_to_over_ctrl(
                                    //         "priv_etc" + value.private_pose_etc_info.state_name,
                                    //         "end",
                                    //         $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}_end",
                                    //         bundle
                                    //     );
                                    //     set_clip_to_over_ctrl(
                                    //         "priv_etc" + value.private_pose_etc_info.state_name,
                                    //         "action",
                                    //         $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}_act",
                                    //         bundle
                                    //     );

                                    //     if (value.private_pose_etc_info.is_has_yes_no)
                                    //     {
                                    //         set_clip_to_over_ctrl(
                                    //             "priv_etc" + value.private_pose_etc_info.state_name,
                                    //             "yes",
                                    //             $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}_yes",
                                    //             bundle
                                    //         );

                                    //         set_clip_to_over_ctrl(
                                    //             "priv_etc" + value.private_pose_etc_info.state_name,
                                    //             "no",
                                    //             $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}_noo",
                                    //             bundle
                                    //         );
                                    //     }
                                    // }
                                    // else
                                    // {
                                    //     if (umset.IsValid() == false)
                                    //     {
                                    //         set_clip_to_over_ctrl(
                                    //             "priv_etc" + value.private_pose_etc_info.state_name,
                                    //             "loop",
                                    //             $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}",
                                    //             bundle
                                    //         );
                                    //     }
                                    //     else
                                    //     {
                                    //         var clipName = umset.GetClipName(value.private_pose_etc_info);
                                    //         if (clipName == string.Empty)
                                    //         {
                                    //             set_clip_to_over_ctrl(
                                    //                 "priv_etc" + value.private_pose_etc_info.state_name,
                                    //                 "loop",
                                    //                 $"com_{Character.RemoveV2FromCharacterID(_Character.CharacterID)}_18{value.private_pose_etc_info.anim_name}",
                                    //                 bundle
                                    //             );
                                    //         }
                                    //         else if (umset.is_load_from_resource == false)
                                    //         {
                                    //             set_clip_to_over_ctrl(
                                    //                 "priv_etc" + value.private_pose_etc_info.state_name,
                                    //                 "loop",
                                    //                 clipName,
                                    //                 umset.set_bundle
                                    //             );
                                    //         }
                                    //     }
                                    // }
                                    // break;
                                }
                            }
                        }
                    )
                );
            }

            var bundleName = PoseManager.GetBundleName(pose_set.category, "z", string.Empty);

            pending++;
            _Character.StartCoroutine(
                AssetBundleManager.LoadAssetBundle(
                    Path.Combine(
                        "com_anim",
                        Path.GetFileNameWithoutExtension(bundleName) + ".reduc.unity3d"
                    ),
                    bundle =>
                    {
                        pending--;
                        if (bundle != null)
                        {
                            set_clip_to_over_ctrl(
                                "in",
                                "right",
                                "com_" + category_short_name + "_20i_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "in",
                                "left",
                                "com_" + category_short_name + "_20n_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "in",
                                "front",
                                "com_" + category_short_name + "_20s_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "out",
                                "right",
                                "com_" + category_short_name + "_20t_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "out",
                                "left",
                                "com_" + category_short_name + "_20o_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "out",
                                "front",
                                "com_" + category_short_name + "_20g_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "run_in",
                                "right",
                                "com_" + category_short_name + "_23i_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "run_in",
                                "left",
                                "com_" + category_short_name + "_23n_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "run_in",
                                "front",
                                "com_" + category_short_name + "_23s_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "run_out",
                                "right",
                                "com_" + category_short_name + "_23t_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "run_out",
                                "left",
                                "com_" + category_short_name + "_23o_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "run_out",
                                "front",
                                "com_" + category_short_name + "_23g_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "hako_in",
                                "right",
                                "com_" + category_short_name + "_40i_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "hako_in",
                                "left",
                                "com_" + category_short_name + "_40n_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "hako_out",
                                "right",
                                "com_" + category_short_name + "_40t_act",
                                bundle
                            );
                            set_clip_to_over_ctrl(
                                "hako_out",
                                "left",
                                "com_" + category_short_name + "_40o_act",
                                bundle
                            );
                        }
                    }
                )
            );

            yield return new WaitUntil(() => pending == 0);

            foreach (var clip in _Character.AnimatorOverrideController.animationClips)
            {
                insert_event_to_clip(clip);
            }

            _Character.Body.AddComponent<AnimEventReceiver>();
        }

        void set_clip_to_over_ctrl(
            string pose_name,
            string postfix,
            string over_name,
            AssetBundle bundle
        )
        {
            var org_name = "dmy_com_" + pose_name;
            if (postfix != string.Empty)
                org_name += "_" + postfix;

            Debug.Log($"set_clip_to_over_ctrl: {org_name} -> {over_name}");

            var clip = bundle.LoadAsset<AnimationClip>(over_name);
            if (clip != null)
                _Character.AnimatorOverrideController[org_name] = clip;
            else
                Debug.LogWarning($"Clip not found in bundle: {over_name}");
        }

        void reset_pose_table() => throw new NotImplementedException();

        public List<string> _GetPoseSetRequiredBundles(PoseSet pose_set)
        {
            var result = new List<string>
            {
                PoseManager.GetBundleName(pose_set.category, "z", string.Empty),
            };

            set_pose_table(pose_set);

            foreach (var (_, value) in pose_infos)
            {
                if (!is_pose_required(value))
                    continue;

                if (!value.is_valid)
                    continue;

                if (value.bundle_vari == "z")
                    continue;

                string name;

                if (!value.is_private)
                {
                    if (value.id == "a19")
                    {
                        name = PoseManager.GetBundleNameCostume(
                            pose_set.category,
                            value.bundle_vari
                        );
                    }
                    else
                    {
                        name = PoseManager.GetBundleName(
                            pose_set.category,
                            value.bundle_vari,
                            value.bundle_pose
                        );
                    }
                }
                else
                {
                    name = PoseManager.GetBundleNamePrivate(_Character.CharacterID);
                }

                if (name != string.Empty && !result.Contains(name))
                {
                    result.Add(name);
                }
            }

            return result;
        }

        bool is_pose_required(PoseInfo pinfo) =>
            pinfo.id == "p01" || _Character.ScenarioMotions.Contains(pinfo.id);

        enum InOutDirection
        {
            None = -1,
            Right = 0,
            Left = 1,
            Front = 2,
            Num = 3,
        }

        enum STEP
        {
            None = -1,
            Pose = 0,
            YesNo = 1,
            ComeIn = 2,
            GoOut = 3,
            Private = 4,
            Waiting = 5,
            Num = 6,
        }

        class StepYesNo
        {
            public bool is_yes = true;
        }

        class StepComeIn
        {
            public InOutDirection direction;

            public float start_time;

            public bool is_run;

            public bool is_hako;
        }

        class StepGoOut
        {
            public InOutDirection direction;

            public float end_time;

            public bool is_run;

            public bool is_hako;
        }

        struct QueuedYesNo
        {
            public bool is_requested;

            public bool is_yes;
        }

        struct PlayAnimationRequest
        {
            public string state;

            public float fade_duration;

            public float start_time;

            public bool is_loop_direct;

            public void Clear()
            {
                state = string.Empty;
                start_time = 0;
                fade_duration = -1;
            }

            public readonly bool IsValid() => state != string.Empty;
        }

        enum PoseGroup
        {
            None = -1,
            Normal = 0,
            Volley = 1,
            Salmon = 2,
            Num = 3,
        }

        enum PrivatePoseType
        {
            None = -1,
            Action = 0,
            Pose = 1,
            Switch = 2,
            Etc = 3,
            Num = 4,
        }

        public enum PrivatePoseEtcType
        {
            None = -1,
            OneShot = 0,
            Pose = 1,
            Pose2 = 2,
            Switch = 3,
            Num = 4,
        }

        internal struct PrivatePoseEtcInfo
        {
            /*0x10*/
            public PrivatePoseEtcType type;

            /*0x14*/public bool is_has_yes_no;

            /*0x18*/public string state_name;

            /*0x20*/public string anim_name;

            /*0x28*/public bool terminate_motion;

            /*0x29*/public bool is_enable_direct;

            public PrivatePoseEtcInfo(
                PrivatePoseEtcType type,
                string state_name,
                string anim_name,
                bool yes_no
            )
            {
                this.type = type;
                this.state_name = state_name;
                this.anim_name = anim_name;
                is_has_yes_no = yes_no;
                terminate_motion = false;
                is_enable_direct = false;
            }

            public void Clear()
            {
                type = PrivatePoseEtcType.None;
                is_has_yes_no = false;
                state_name = string.Empty;
                anim_name = string.Empty;
            }

            public readonly bool IsTypePose() => type == PrivatePoseEtcType.Pose2;
        }

        internal struct PrivatePoseInfo
        {
            public bool is_has_action;

            public bool is_has_pose;

            public bool is_has_switch;

            public bool is_has_yes_no;

            public PrivatePoseEtcInfo[] etcs;

            public PrivatePoseInfo(
                bool is_has_action,
                bool is_has_pose,
                bool is_has_switch,
                bool is_has_yes_no,
                PrivatePoseEtcInfo[] etcs
            )
            {
                this.is_has_action = is_has_action;
                this.is_has_pose = is_has_pose;
                this.is_has_switch = is_has_switch;
                this.is_has_yes_no = is_has_yes_no;
                this.etcs = etcs;
            }
        }

        class PoseInfo
        {
            public string id;

            public string number;

            /*0x20*/
            public PoseGroup group;

            public string bundle_pose;

            public string bundle_vari;

            public bool is_valid;

            public int index;

            public bool has_loop;

            public bool has_yes_no;

            /*0x42*/public bool is_action;

            public string switch_from;

            /*0x50*/public string switch_to;

            public bool is_private;

            /*0x5c*/
            public PrivatePoseType private_pose_type;

            /*0x60*/
            public PrivatePoseEtcInfo private_pose_etc_info;

            public static PoseInfo CreatePose(string id, bool has_yes_no) =>
                new(id) { has_loop = true, has_yes_no = has_yes_no };

            public static PoseInfo CreateAction(string id) => new(id) { is_action = true };

            public static PoseInfo CreateSwitch(string switch_from, string switch_to)
            {
                var result = new PoseInfo(switch_from + "_" + switch_to)
                {
                    number = switch_to,
                    switch_from = switch_from,
                    switch_to = switch_to,
                };

                if (!char.IsNumber(switch_to[0]))
                {
                    result.number = switch_to[1..];
                }

                if (!int.TryParse(result.number, out result.index))
                    result.index = 0;
                return result;
            }

            public static PoseInfo CreatePrivatePose(string id, bool has_yes_no)
            {
                var result = CreatePose(id, has_yes_no);
                result.is_private = true;
                result.private_pose_type = PrivatePoseType.Pose;
                return result;
            }

            public static PoseInfo CreatePrivateAction(string id)
            {
                var result = CreateAction(id);
                result.is_private = true;
                result.private_pose_type = PrivatePoseType.Action;
                return result;
            }

            public static PoseInfo CreatePrivateSwitch(string id) =>
                new(id + "_s")
                {
                    is_private = true,
                    private_pose_type = PrivatePoseType.Switch,
                    switch_from = id + "_a",
                    switch_to = id + "_p",
                    number = "18",
                    index = 18,
                };

            public static PoseInfo CreatePrivateEtc(string id, PrivatePoseEtcInfo etc_info)
            {
                var result = CreateAction(id);
                result.is_private = true;
                result.private_pose_type = PrivatePoseType.Etc;
                result.private_pose_etc_info = etc_info;
                if (result.private_pose_etc_info.type < PrivatePoseEtcType.Num)
                {
                    result.has_loop = true;
                    result.has_yes_no = result.private_pose_etc_info.is_has_yes_no;
                }
                return result;
            }

            PoseInfo(string id)
            {
                this.id = id;
                number = id;
                if (!char.IsNumber(id[0]))
                {
                    number = id[1..];
                }
                group = 0;
                is_valid = false;
                bundle_pose = string.Empty;
                bundle_vari = string.Empty;
                if (!int.TryParse(number, out index))
                    index = 0;
                has_loop = false;
                has_yes_no = false;
                is_action = false;
                is_private = false;
                private_pose_type = PrivatePoseType.None;
                switch_from = string.Empty;
                switch_to = string.Empty;
                private_pose_etc_info.Clear();
            }

            public bool is_switch() => switch_from != string.Empty && switch_to != string.Empty;
        }

        internal class PoseSet
        {
            public CharacterCommuMotionCategory category;

            public string name;

            public Dictionary<string, string> variations;

            public PoseSet()
            {
                category = CharacterCommuMotionCategory.None;
                variations = new Dictionary<string, string>();
            }
        }

        class UniqueMotionSet
        {
            /*0x10*/
            UniqueMotionSetInfo set_info;

            /*0x18*/
            CharacterActorCommu actor;

            /*0x20*/string set_name;

            /*0x28*/string set_bundle_name;

            /*0x30*/
            public AssetBundle set_bundle;

            /*0x38*/bool is_valid;

            ///*0x40*/Imas.CharaCommuUniqueMotionSetInfo.Internal.Data set_data;
            /*0x68*/public bool is_load_from_resource;

            static /*0x538c48c*/
            string CreateBundleName(string set_name, string chr_id) =>
                throw new NotImplementedException();

            static /*0x538c850*/
            string CreateJsonName(string set_name, string chr_id) =>
                throw new NotImplementedException();

            public UniqueMotionSet(CharacterActorCommu actor) =>
                throw new NotImplementedException();

            public bool IsValid() => throw new NotImplementedException();

            /*0x538c0d0*/string GetSetName() => throw new NotImplementedException();

            /*0x538c23c*/bool Create(string set_name) => throw new NotImplementedException();

            public void OnBundleLoaded() => throw new NotImplementedException();

            /*0x538c710*/public string GetClipName(PrivatePoseEtcInfo pinfo) =>
                throw new NotImplementedException();

            /*0x538c5f8*/void load_set_info_from_bundle(string set_name, string chr_id) =>
                throw new NotImplementedException();

            /*0x538c39c*/void load_set_info_from_resource(string set_name, string chr_id) =>
                throw new NotImplementedException();

            /*0x538c8ac*/void load_set_info_common(TextAsset text_asset) => throw new NotImplementedException();
        }

        class UniqueMotionSetInfo
        {
            /*0x10*/string chr_id;

            /*0x18*/
            List<Motion> motions;

            /*0x20*/
            List<string> means;

            /*0x28*/
            List<string> motion_means;

            /*0x538c194*/UniqueMotionSetInfo() => throw new NotImplementedException();

            /*0x538cca4*/
            Motion AddMotion() => throw new NotImplementedException();

            class Motion
            {
                /*0x10*/
                PrivatePoseType privete_type;

                /*0x14*/int privete_etc_index;

                /*0x18*/string motion_name;

                static /*0x538ce8c*/
                string ToSlotStringShort(PrivatePoseType private_type, int private_etc_index) =>
                    throw new NotImplementedException();

                static /*0x538cbe4*/
                bool FromSlotStringShort(
                    string as_str,
                    ref PrivatePoseType private_type,
                    ref int private_etc_index
                ) => throw new NotImplementedException();

                /*0x538cba8*/Motion() => throw new NotImplementedException();

                /*0x538cdd0*/
                Motion(PrivatePoseType privete_type, int privete_etc_index, string motion_name) =>
                    throw new NotImplementedException();

                bool IsValid() => throw new NotImplementedException();
            }
        }
    }
}
