using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Imas
{
    class ScenarioManager : MBSingleton<ScenarioManager>
    {
        private string _ScenarioId;
        private Scenario _Scenario;
        public readonly Dictionary<string, FHout[]> _FHoutTexts = new();
        private readonly Dictionary<string, Cut> _Cuts = new();
        private bool _IsPlaying;
        private Coroutine _PlayCoroutine;

        public IEnumerator LoadScenario(string scenarioId)
        {
            CharacterManager.Clear();
            _ScenarioId = scenarioId;
            _IsPlaying = false;
            if (_PlayCoroutine != null)
            {
                StopCoroutine(_PlayCoroutine);
                _PlayCoroutine = null;
            }

            string json = null;
            yield return AssetBundleManager.LoadAssetBundle(
                Path.Combine("scenario", scenarioId + ".unity3d"),
                ab => json = ab.LoadAsset<TextAsset>(scenarioId).text
            );

            _Scenario = JsonUtility.FromJson<Scenario>(json);

            yield return AssetBundleManager.LoadAssetBundle(
                Path.Combine("scenario", $"fhout_{scenarioId}.json.unity3d"),
                ab => json = ab.LoadAsset<TextAsset>($"fhout_{scenarioId}").text
            );

            var fhoutTexts = JsonUtility.FromJson<FHoutTexts>(json);
            _FHoutTexts.Clear();
            foreach (var fhoutText in fhoutTexts.Body)
                _FHoutTexts[fhoutText.TextId] = fhoutText.List;

            _Cuts.Clear();
            Cut currentCut = null;
            foreach (var seq in _Scenario.datas.CutRecord)
            {
                switch (seq.command)
                {
                    case "create_cut":
                        if (currentCut != null)
                            _Cuts[currentCut.id] = currentCut;
                        currentCut = new Cut(seq);
                        break;
                    case "actor_load_character":
                        currentCut.Actors.Add(new LoadCharacterActor(seq));
                        break;
                    case "actor_camera_set_position_lookat":
                        currentCut.Actors.Add(new CameraSetPositionLookAtActor(seq));
                        break;
                    case "actor_visible_character":
                        currentCut.Actors.Add(new VisibleCharacterActor(seq));
                        break;
                    case "actor_set_position":
                        currentCut.Actors.Add(new SetPositionActor(seq));
                        break;
                    case "actor_set_rotation":
                        currentCut.Actors.Add(new SetRotationActor(seq));
                        break;
                    case "actor_pose":
                        currentCut.Actors.Add(new PoseActor(seq));
                        break;
                    case "actor_facial":
                        currentCut.Actors.Add(new FacialActor(seq));
                        break;
                    case "actor_text":
                        currentCut.Actors.Add(new TextActor(seq));
                        break;
                    case "actor_2d_bg":
                        currentCut.Actors.Add(new Set2dBGActor(seq));
                        break;
                    case "actor_idol_layer":
                        currentCut.Actors.Add(new IdolLayerActor(seq));
                        break;
                    case "actor_wait":
                        currentCut.Actors.Add(new WaitActor(seq));
                        break;
                    case "actor_yes_no":
                        currentCut.Actors.Add(new YesNoActor(seq));
                        break;
                    case "actor_eye_blink":
                        currentCut.Actors.Add(new EyeBlinkActor(seq));
                        break;
                    case "actor_eye_direction":
                        currentCut.Actors.Add(new EyeDirectionActor(seq));
                        break;
                    case "actor_chara_dorotate":
                        currentCut.Actors.Add(new CharaDoRotateActor(seq));
                        break;
                    default:
                        Debug.LogWarning($"Unknown command: {seq.command}");
                        break;
                }
            }

            if (currentCut != null)
                _Cuts[currentCut.id] = currentCut;

            InitializePass();
            foreach (var character in CharacterManager.GetAllCharacters())
            {
                if (!character.IsInitialized)
                    yield return new WaitUntil(() => character.IsInitialized);
                yield return character.CharacterActorCommu.OverridePoseClip(is_sit: false);
            }
        }

        private void InitializePass()
        {
            foreach (var cut in _Cuts.Values)
            foreach (var actor in cut.Actors)
                actor.Init();
        }

        public void Play()
        {
            _IsPlaying = true;
            _PlayCoroutine ??= StartCoroutine(PlayPass());
        }

        public const float SCR_FRAME_TIME = 1f / 60f;
        private float _DeltaTime = 0.0f;

        private IEnumerator PlayPass()
        {
            var index = 0;
            while (true)
            {
                if (!_IsPlaying)
                    yield return new WaitUntil(() => _IsPlaying);

                var seq = _Scenario.datas.Scenario[index];
                switch (seq.command)
                {
                    case "play_cut":
                        var cutId = seq.arg1;
                        var cut = _Cuts[cutId];
                        if (cut.CutType == CutType.NORMAL)
                            _IsPlaying = false;
                        var currentTime = 0.0f;

                        Debug.Log($"Playing cut: {cutId}");

                        while (currentTime / SCR_FRAME_TIME < cut.frameCount)
                        {
                            var currentFrame = currentTime / SCR_FRAME_TIME;
                            foreach (var actor in cut.Actors)
                            {
                                if (
                                    actor.from_cut <= currentFrame
                                    && actor.from_cut > (currentTime - _DeltaTime) / SCR_FRAME_TIME
                                )
                                {
                                    Debug.Log(
                                        $"Executing actor: {actor.GetType().Name} at frame {currentFrame}"
                                    );
                                    actor.Exec();
                                }

                                if (currentFrame > actor.from_cut)
                                    actor.OnUpdate((int)currentFrame - actor.from_cut);
                            }

                            _DeltaTime = 0.0f;
                            while (true)
                            {
                                yield return new WaitForEndOfFrame();
                                if (_DeltaTime > 0.0f)
                                {
                                    currentTime += _DeltaTime;
                                    break;
                                }
                            }
                        }
                        break;

                    default:
                        Debug.LogWarning($"Unknown command: {seq.command}");
                        break;
                }
                index++;
            }
        }

        private void Update()
        {
            _DeltaTime += Time.deltaTime;
        }
    }
}
