using System;
using UnityEngine;

namespace Imas
{
    class EyeTracking : MonoBehaviour
    {
        const float FRONT_DEPTH = 20.0f;
        const float ANGLE_LIMIT_X = 30.0f;
        const float ANGLE_LIMIT_Y = 10.0f;

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
        private Vector2 eyeDirectionRate = Vector2.zero;

        public Vector2 GetEyeDirectionRate() => eyeDirectionRate;

        public void Init(Character character)
        {
            var headName = character.Head.name;
            var eyes = GameObject.Find(headName + "/obj_head_GP/eyes");
            rendEye = eyes.GetComponent<Renderer>();
            var eyeTgtName =
                "EyeTgT" + headName.Replace("_sub", string.Empty).Replace("Idol", string.Empty);
            var eyeTgt = GameObject.Find(eyeTgtName);
            if (eyeTgt == null)
                eyeTgt = new GameObject(eyeTgtName);
            eyeTarget = eyeTgt.transform;
            SetTarget(eyedirtype.Front, 0.0f, 0.0f, 0, 0);
            evCamNo = 0;
            camNo = 0;
            id_SubTex = Shader.PropertyToID("_SubTex");
            initf = true;
        }

        void SetTarget(eyedirtype type0, float offX0, float offY0, int cam, int idol)
        {
            type = type0;
            camSampled = false;
            //SetTargetCamera(cam);
            if (idol > 0)
            {
                tgtIdol = idol;
            }
            if (type == eyedirtype.Fixed || type == eyedirtype.FaceRelative)
            {
                offX = offX0;
                offY = offY0;
            }
        }

        public void EyeUpdate()
        {
            if (!initf)
                return;

            var a = Vector3.zero;
            //if (MainLoop.Instance != null)
            //{
            //    var liveParams = LiveSettings.Instance?.liveParams;
            //    if (liveParams != null && liveParams.UseNewEyeFrontTarget())
            //    {
            //        GameObject eyeObj = info.model.Eye;
            //        Vector3 eyePos = eyeObj.transform.position;
            //        a = new Vector3(eyePos.x, 0f, eyePos.z);
            //    }
            //}

            var b = Vector3.zero;
            bool directSet = false;
            var directTarget = Vector3.zero;

            switch (type)
            {
                case eyedirtype.Front:
                    b = new Vector3(0.0f, transform.position.y, FRONT_DEPTH);
                    break;
                case eyedirtype.LookCamera:
                    directTarget = GetCamTgtPos();
                    directSet = true;
                    break;
                case eyedirtype.LookRFinger:
                    throw new NotImplementedException();
                //directTarget = info.model.RFinger.transform.position;
                //directSet = true;
                //break;
                case eyedirtype.LookLFinger:
                    throw new NotImplementedException();
                //directTarget = info.model.LFinger.transform.position;
                //directSet = true;
                //break;
                case eyedirtype.BustDir:
                    throw new NotImplementedException();
                //{
                //    Transform mune2 = info.model.Mune2.transform;
                //    Vector3 munePos = mune2.position;
                //    aX = munePos.x; aY = munePos.y; aZ = munePos.z;
                //    b = mune2.up * 100f;
                //    break;
                //}
                case eyedirtype.Fixed:
                    b = new Vector3(offX, transform.position.y + offY, FRONT_DEPTH);
                    break;
                case eyedirtype.FaceRelative:
                    directTarget = transform.TransformPoint(-FRONT_DEPTH, -offX, offY);
                    directSet = true;
                    break;
                case eyedirtype.OtherIdol:
                    throw new NotImplementedException();
                //{
                //    LiveSettings settings = LiveSettings.Instance;
                //    LiveParams liveParams = settings?.liveParams;
                //    DanceIdolInfo[] idols = liveParams.GetCurrentIdols();
                //    int idx = tgtIdol - 1;
                //    directTarget = idols[idx].model.Atama.transform.position;
                //    directSet = true;
                //    break;
                //}
                case eyedirtype.FmtFront:
                    throw new NotImplementedException();
                //{
                //    Vector3 eyePos = info.model.Eye.transform.position;
                //    if (info.formationRy == 0f)
                //    {
                //        b = new Vector3(0f, eyePos.y, 20f);
                //    }
                //    else
                //    {
                //        Vector3 v = new Vector3(0f, eyePos.y, 20f);
                //        v = info.rotFmtRy * v;          // Quaternion * Vector3
                //        b = v;
                //    }
                //    break;
                //}
                case eyedirtype.CamFix:
                    if (!camSampled)
                    {
                        var camPos = GetCamTgtPos();
                        directTarget = camPos;

                        var localPos = transform.InverseTransformPoint(camPos);
                        if (localPos.x < 0.0f)
                        {
                            localPos = -localPos;
                            localPos /= (localPos.x / FRONT_DEPTH);
                            offX = localPos.y;
                            offY = -localPos.z;
                        }
                        camSampled = true;
                    }
                    else
                    {
                        var localPos = new Vector3(-FRONT_DEPTH, -offX, offY);
                        directTarget = transform.TransformPoint(localPos);
                    }
                    directSet = true;
                    break;
            }

            if (directSet)
            {
                eyeTargetPos = directTarget;
            }
            else
            {
                eyeTargetPos = a + b;
            }

            var localDir = transform.InverseTransformPoint(eyeTargetPos);
            eyeTarget.position = eyeTargetPos;
            eye_rx = Mathf.Atan2(localDir.y, -localDir.x) * Mathf.Rad2Deg;
            eye_ry = Mathf.Atan2(localDir.z, -localDir.x) * Mathf.Rad2Deg;

            eye_rx = Mathf.Clamp(eye_rx, -ANGLE_LIMIT_X, ANGLE_LIMIT_X);
            eye_ry = Mathf.Clamp(eye_ry, -ANGLE_LIMIT_Y, ANGLE_LIMIT_Y);

            if (!on)
            {
                if (!useEyeMats)
                {
                    foreach (var mat in rendEye.sharedMaterials)
                    {
                        mat.mainTextureOffset = Vector2.zero;
                        mat.SetTextureOffset(id_SubTex, Vector2.zero);
                    }
                }
                else
                {
                    foreach (var mat in eyeMats)
                    {
                        mat.mainTextureOffset = Vector2.zero;
                        mat.SetTextureOffset(id_SubTex, Vector2.zero);
                    }
                }
            }
            else
            {
                var offset = new Vector2(eye_rx * -0.004f, eye_ry * -0.004f);
                var scaleVec = new Vector2(scale, scale);

                if (!useEyeMats)
                {
                    foreach (var mat in rendEye.sharedMaterials)
                    {
                        mat.mainTextureOffset = offset;
                        mat.mainTextureScale = scaleVec;
                        mat.SetTextureOffset(id_SubTex, offset);
                        mat.SetTextureScale(id_SubTex, scaleVec);
                    }
                }
                else
                {
                    foreach (var mat in eyeMats)
                    {
                        mat.mainTextureOffset = offset;
                        mat.mainTextureScale = scaleVec;
                        mat.SetTextureOffset(id_SubTex, offset);
                        mat.SetTextureScale(id_SubTex, scaleVec);
                    }
                }
            }
        }

        Vector3 GetCamTgtPos()
        {
            throw new NotImplementedException();
            //Vector3 result = Vector3.one;

            //CameraCtrl camCtrl = CameraCtrl.camCtrl;
            //if (camCtrl != null)
            //{
            //    if (camCtrl.upCam != null && camCtrl.upCam.enabled)
            //    {
            //        result = camCtrl.panTra.position;
            //    }
            //    else
            //    {
            //        MayaCameraSet[] mayaCameraSet = camCtrl.mayaCameraSet;
            //        int index = (int)camNo;

            //        if (mayaCameraSet[index].camera.enabled)
            //        {
            //            result = mayaCameraSet[index].camObj.transform.position;
            //        }
            //    }
            //}

            //return result;
        }

        void SetMaterials(Material material_eyeR, Material material_eyeL)
        {
            useEyeMats = true;
            eyeMats = new Material[2];
            eyeMats[0] = material_eyeR;
            eyeMats[1] = material_eyeL;
        }

        void SetMaterial(Material material_eye)
        {
            useEyeMats = true;
            eyeMats = new Material[1];
            eyeMats[0] = material_eye;
        }

        void SetTargetCamera(int cam)
        {
            throw new NotImplementedException();
            //evCamNo = cam;
            //camNo = cam;

            //CameraCtrl camCtrl = CameraCtrl.camCtrl;
            //if (camCtrl != null)
            //{
            //    int selectedCam = camCtrl.SelectScreenCamera(cam);
            //    camNo = selectedCam;
            //}
        }

        internal void SetEyeDirectionByRate(float hRate, float vRate)
        {
            eyeDirectionRate = new Vector2(hRate, vRate);

            float tx = Mathf.InverseLerp(-1f, 1f, hRate);
            float ty = Mathf.InverseLerp(-1f, 1f, vRate);

            float x = Mathf.Lerp(
                -FRONT_DEPTH * Mathf.Tan(ANGLE_LIMIT_X * Mathf.Deg2Rad),
                FRONT_DEPTH * Mathf.Tan(ANGLE_LIMIT_X * Mathf.Deg2Rad),
                tx
            );
            float y = Mathf.Lerp(
                -FRONT_DEPTH * Mathf.Tan(ANGLE_LIMIT_Y * Mathf.Deg2Rad),
                FRONT_DEPTH * Mathf.Tan(ANGLE_LIMIT_Y * Mathf.Deg2Rad),
                ty
            );

            SetTarget(eyedirtype.FaceRelative, x, y, 0, -1);
        }

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
