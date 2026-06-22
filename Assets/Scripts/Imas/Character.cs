using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Imas
{
    class Character : MonoBehaviour
    {
        public string CharacterID { get; private set; }

        public CharacterActorCommu CharacterActorCommu { get; private set; }

        public bool IsInitialized { get; private set; } = false;

        public GameObject Body { get; private set; }
        public GameObject Head { get; private set; }
        public GameObject Eye { get; private set; }
        public FaceCtrl FaceCtrl { get; private set; }
        public EyeTracking EyeTracking { get; private set; }
        public Animator Animator { get; private set; }
        public AnimatorOverrideController AnimatorOverrideController { get; private set; }
        public HashSet<string> ScenarioMotions { get; } = new HashSet<string>();

        public static string RemoveV2FromCharacterID(string chr_id) =>
            chr_id.EndsWith("_v2") ? chr_id[..^"_v2".Length] : chr_id;

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void SetPosition(Vector3 position) => transform.position = position;

        public void SetRotation(Vector3 eulerAngle) => transform.eulerAngles = eulerAngle;

        public IEnumerator InitializeAsync(string characterID, string resourceID)
        {
            CharacterID = characterID;
            AssetBundle cb_ab = null;
            AssetBundle ch_ab = null;
            yield return AssetBundleManager.LoadAssetBundle(
                Path.Combine("model", $"cb_{resourceID}.unity3d"),
                ab => cb_ab = ab
            );
            yield return AssetBundleManager.LoadAssetBundle(
                Path.Combine("model", $"ch_{resourceID}.unity3d"),
                ab => ch_ab = ab
            );

            Body = Instantiate(cb_ab.LoadAsset<GameObject>($"cb_{resourceID}"), transform);
            var model00 = Body.transform.Find("MODEL_00");
            var bodyScale = new GameObject("BODY_SCALE");
            bodyScale.transform.SetParent(model00, false);
            model00.Find("BASE").SetParent(bodyScale.transform, false);
            model00.gameObject.AddComponent<BoneLib.CalcBone>();

            Head = Instantiate(ch_ab.LoadAsset<GameObject>($"ch_{resourceID}"), Body.transform);

            Eye = new GameObject("Eye_obj");
            Eye.transform.SetParent(Head.transform.Find("KUBI/ATAMA"), false);
            Eye.transform.position = Head
                .transform.Find("obj_head_GP/eyes")
                .GetComponent<SkinnedMeshRenderer>()
                .bounds.center;
            EyeTracking = Eye.AddComponent<EyeTracking>();
            EyeTracking.Init(this);
            EyeTracking.SetEyeDirectionByRate(0f, 0f);

            FaceCtrl = new FaceCtrl(this);
            FaceCtrl.SetEyesBlink(true);
            FaceCtrl.Init();

            foreach (var renderer in Body.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (var material in renderer.materials)
                {
                    material.SetColor("_IllumiColor", new Color(0.7f, 0.7f, 0.7f, 0.7f));
                    material.SetColor("_Color", new Color(0.7f, 0.7f, 0.7f, 0.7f));
                }
            }

            Animator = Body.GetComponent<Animator>();
            AnimatorOverrideController = new AnimatorOverrideController(
                Resources.Load<RuntimeAnimatorController>("AnimCtrl_Commu")
            );
            Animator.runtimeAnimatorController = AnimatorOverrideController;

            CharacterActorCommu = new CharacterActorCommu(this);
            IsInitialized = true;
        }

        public void Initialize(string characterID, string resourceID) =>
            StartCoroutine(InitializeAsync(characterID, resourceID));

        private void SyncHead()
        {
            var ch_atama = Head.transform.Find("KUBI/ATAMA");
            var cb_atama = Body.transform.Find("MODEL_00/BODY_SCALE/BASE/MUNE1/MUNE2/KUBI/ATAMA");
            ch_atama.transform.SetPositionAndRotation(
                cb_atama.transform.position,
                cb_atama.transform.rotation
            );
        }

        private void LateUpdate()
        {
            if (!IsInitialized)
                return;

            SyncHead();
            FaceCtrl.Action();
            EyeTracking.EyeUpdate();
        }
    }
}
