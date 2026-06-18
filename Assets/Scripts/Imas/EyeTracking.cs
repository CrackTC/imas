using System;
using UnityEngine;

namespace Imas
{
    class EyeTracking : MonoBehaviour
    {
        const float frontDepth = 20;

        public Transform eye_base;
        public Transform eyeTarget;
        public Vector3 eyeTargetPos;
        public bool on = true;
        public bool initf;
        public eyedirtype type;
        public float scale = 1.0f;
        public int evCamNo;
        public int camNo;
        public bool camSampled;
        public float offX;
        public float offY;
        public int tgtIdol = 1;
        public float eye_rx;
        public float eye_ry;
        public Renderer rendEye;
        public Material[] eyeMats;
        public bool useEyeMats;
        public int id_SubTex;

        void Init() => throw new NotImplementedException();

        void SetTarget(eyedirtype type0, float offX0, float offY0, int cam, int idol) =>
            throw new NotImplementedException();

        void EyeUpdate() => throw new NotImplementedException();

        Vector3 GetCamTgtPose() => throw new NotImplementedException();

        void SetMaterials(Material material_eyeR, Material material_eyeL) =>
            throw new NotImplementedException();

        void SetMaterial(Material material_eye) => throw new NotImplementedException();

        void SetTargetCamera(int cam) => throw new NotImplementedException();

        public enum eyedirtype
        {
            Front = 0,
            LookCamera = 1,
            LookRFinger = 2,
            BustDir = 3,
            Fixed = 4,
            FaceRelative = 5,
            OtherIdol = 6,
            FmtFront = 7,
            CamFix = 8,
            LookLFinger = 9,
        }
    }
}
