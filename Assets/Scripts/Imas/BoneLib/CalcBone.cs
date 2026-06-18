using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Imas.BoneLib
{
    static class BoneUtil
    {
        public static Matrix4x4 ToMatrix(Quaternion q) =>
            Matrix4x4.TRS(Vector3.zero, q, Vector3.one);

        public static Vector3 GetRot(rot_order order, Matrix4x4 m)
        {
            float radX;
            float radY;
            float radZ;

            switch (order)
            {
                case rot_order.ROT_XYZ:
                    radX = Mathf.Atan2(m.m12, m.m22);
                    radY = Mathf.Asin(-m.m02);
                    radZ = Mathf.Atan2(m.m01, m.m00);
                    break;

                case rot_order.ROT_YZX:
                    radX = Mathf.Atan2(m.m12, m.m11);
                    radY = Mathf.Atan2(m.m20, m.m00);
                    radZ = Mathf.Asin(-m.m10);
                    break;

                case rot_order.ROT_ZXY:
                    radX = Mathf.Asin(-m.m21);
                    radY = Mathf.Atan2(m.m20, m.m22);
                    radZ = Mathf.Atan2(m.m01, m.m11);
                    break;

                case rot_order.ROT_XZY:
                    radX = Mathf.Atan2(-m.m21, m.m11);
                    radY = Mathf.Atan2(-m.m02, m.m00);
                    radZ = Mathf.Asin(m.m01);
                    break;

                case rot_order.ROT_YXZ:
                    radX = Mathf.Asin(m.m12);
                    radY = Mathf.Atan2(-m.m02, m.m22);
                    radZ = Mathf.Atan2(-m.m10, m.m11);
                    break;

                case rot_order.ROT_ZYX:
                    radX = Mathf.Atan2(-m.m21, m.m22);
                    radY = Mathf.Asin(m.m20);
                    radZ = Mathf.Atan2(-m.m10, m.m00);
                    break;

                default:
                    return Vector3.zero;
            }

            return new Vector3(radX * Mathf.Rad2Deg, radY * Mathf.Rad2Deg, radZ * Mathf.Rad2Deg);
        }

        public enum rot_order
        {
            ROT_XYZ,
            ROT_YZX,
            ROT_ZXY,
            ROT_XZY,
            ROT_YXZ,
            ROT_ZYX,
        }
    }

    public class CalcBone : MonoBehaviour
    {
        static int ForceUpdateFrames;
        readonly int[] param_axis = new int[2];
        readonly float[] param_value = new float[2];
        readonly List<CALC_BONE_INFO> m_infoList = new();
        int forceUpdate;

        static Quaternion SlerpEx(
            Quaternion curRefRot,
            Quaternion initRefRot,
            Vector3 initRefVec,
            float t_all,
            float tx
        ) =>
            MathUtil.Slerp(
                initRefRot,
                MathUtil.Slerp(
                    MathUtil.RotationArc(initRefVec, curRefRot * Vector3.right) * initRefRot,
                    curRefRot,
                    tx
                ),
                t_all
            );

        static Quaternion Twist(
            Quaternion curRefRot,
            Quaternion initRefRot,
            Vector3 initRefVec,
            float t
        ) =>
            Quaternion.Inverse(curRefRot)
            * MathUtil.Slerp(
                MathUtil.RotationArc(initRefVec, curRefRot * Vector3.right) * initRefRot,
                curRefRot,
                t
            );

        static Quaternion ROT(
            Quaternion curRefRot,
            Quaternion initRefRot,
            Vector3 initRefVec,
            Quaternion initMyRot,
            float tx
        ) =>
            initMyRot
            * MathUtil.Slerp(
                Quaternion.identity,
                Quaternion.Inverse(
                    MathUtil.RotationArc(initRefVec, curRefRot * Vector3.right) * initRefRot
                ) * curRefRot,
                tx
            );

        bool IsAssistBone(Transform tr) =>
            tr.name.Contains("__slerp")
            || tr.name.Contains("__twist")
            || tr.name.Contains("__const")
            || tr.name.Contains("__rot");

        string getTargetName(string node)
        {
            var start = node.IndexOf("__");
            if (start < 0)
                return null;

            start = node.IndexOf("_", start + 2) + 1;
            var end = node.IndexOf("__", start);
            if (end < 0)
                return null;
            return node[start..end];
        }

        void getParams(string node, ALGO_TYPE type)
        {
            for (int i = 0; i < 2; i++)
            {
                param_axis[i] = -1;
                param_value[i] = 0f;
            }

            if (type == ALGO_TYPE.CONST)
                return;

            var start = node.IndexOf("__");
            if (start < 0)
                return;

            var end = node.IndexOf("__", start + 2);
            if (end < 0)
            {
                var firstStart = node.IndexOf("_", start + 2);
                if (type != ALGO_TYPE.TWIST)
                    return;

                param_axis[0] = node[firstStart + 1] switch
                {
                    'x' => 0,
                    'y' => 1,
                    'z' => 2,
                    _ => -1,
                };

                param_value[0] = int.Parse(node[(firstStart + 2)..]) / 100f;
            }
            else
            {
                var secondStart = node.IndexOf("_", end + 2);
                for (int i = 0, currentStart = end + 2; i < 2; i++, currentStart = secondStart + 1)
                {
                    param_axis[i] = node[currentStart] switch
                    {
                        'x' => 0,
                        'y' => 1,
                        'z' => 2,
                        _ => -1,
                    };
                    var value = int.Parse(
                        (secondStart > -1 && i == 0)
                            ? node[(currentStart + 1)..secondStart]
                            : node[(currentStart + 1)..]
                    );
                    param_value[i] = value / 100f;

                    if (secondStart < 0)
                        break;
                }
            }
        }

        void Awake()
        {
            var hashTable = new Hashtable();
            var trs = GetComponentsInChildren<Transform>();
            foreach (var tr in trs)
                hashTable[tr.name] = tr;
            foreach (var tr in trs)
            {
                CALC_BONE_INFO info = null;
                if (tr.name.Contains("__slerp"))
                {
                    getParams(tr.name, ALGO_TYPE.SLERP);
                    info = new CALC_BONE_INFO(
                        ALGO_TYPE.SLERP,
                        tr.name,
                        getTargetName(tr.name),
                        param_value[0],
                        param_value[1],
                        param_axis[1]
                    );
                }
                else if (tr.name.Contains("__twist"))
                {
                    getParams(tr.name, ALGO_TYPE.TWIST);
                    info = new CALC_BONE_INFO(
                        ALGO_TYPE.TWIST,
                        tr.name,
                        tr.parent.name,
                        -1f,
                        param_value[0],
                        param_axis[0]
                    );
                }
                else if (tr.name.Contains("__tite"))
                {
                    if (tr.name.Contains("_front_rot"))
                    {
                        getParams(tr.name, ALGO_TYPE.TITE_FRONT_ROT);
                        info = new CALC_BONE_INFO(
                            ALGO_TYPE.TITE_FRONT_ROT,
                            tr.name,
                            "MOMO_L",
                            "MOMO_R",
                            -1f,
                            param_value[0],
                            param_axis[0]
                        );
                    }
                    else if (tr.name.Contains("_front_trans_wait"))
                    {
                        getParams(tr.name, ALGO_TYPE.TITE_FRONT_TRANS);
                        info = new CALC_BONE_INFO(
                            ALGO_TYPE.TITE_FRONT_TRANS,
                            tr.name,
                            tr.parent.name,
                            -1f,
                            param_value[0],
                            param_axis[0]
                        );
                    }
                    else if (tr.name.Contains("_back_rot_YZ"))
                    {
                        getParams(tr.name, ALGO_TYPE.TITE_BACK_ROT_YZ);
                        info = new CALC_BONE_INFO(
                            ALGO_TYPE.TITE_BACK_ROT_YZ,
                            tr.name,
                            "MOMO_L",
                            "MOMO_R",
                            -1f,
                            param_value[0],
                            param_axis[0]
                        );
                    }
                    else if (tr.name.Contains("_back_rot_X_wait"))
                    {
                        getParams(tr.name, ALGO_TYPE.TITE_BACK_ROT_X);
                        info = new CALC_BONE_INFO(
                            ALGO_TYPE.TITE_BACK_ROT_X,
                            tr.name,
                            "MOMO_L",
                            "MOMO_R",
                            -1f,
                            param_value[0],
                            param_axis[0]
                        );
                    }
                }

                if (tr.name.Contains("__const"))
                {
                    getParams(tr.name, ALGO_TYPE.CONST);
                    info = new CALC_BONE_INFO(
                        ALGO_TYPE.CONST,
                        tr.name,
                        getTargetName(tr.name),
                        -1,
                        param_value[0],
                        param_axis[0]
                    );
                }
                else if (tr.name.Contains("__rot"))
                {
                    getParams(tr.name, ALGO_TYPE.ROT);
                    if (param_axis[0] is not 2)
                    {
                        info = new CALC_BONE_INFO(
                            ALGO_TYPE.ROT,
                            tr.name,
                            getTargetName(tr.name),
                            -1f,
                            param_value[0],
                            param_axis[0]
                        );
                    }
                    else
                    {
                        info = new CALC_BONE_INFO(
                            ALGO_TYPE.SLERP,
                            tr.name,
                            getTargetName(tr.name),
                            param_value[0],
                            1f,
                            param_axis[0]
                        );
                    }
                }

                if (info is null)
                    continue;

                info.myTransform = (Transform)hashTable[info.myPath];
                info.initMyRot = info.myTransform.localRotation;
                info.initMyPosition = info.myTransform.localPosition;

                info.referTransform = (Transform)hashTable[info.referPath];
                info.referInitRot = info.referTransform.localRotation;
                info.referInitVec = info.referTransform.localRotation * Vector3.right;

                if (!string.IsNullOrEmpty(info.referPath2))
                {
                    info.referTransform2 = (Transform)hashTable[info.referPath2];
                    info.referInitRot2 = info.referTransform2.localRotation;
                    info.referInitVec2 = info.referTransform2.localRotation * Vector3.right;
                }

                m_infoList.Add(info);
            }
        }

        void LateUpdate()
        {
            //if (forceUpdate < 1)
            //    return;

            //forceUpdate--;
            ExplicitUpdate();
        }

        void RequestUpdate() => forceUpdate = ForceUpdateFrames;

        void ExplicitUpdate()
        {
            foreach (var info in m_infoList)
            {
                var curReferRot = info.referTransform.localRotation;
                Quaternion curReferRot2;
                switch (info.type)
                {
                    case ALGO_TYPE.SLERP:
                        info.myTransform.localRotation = SlerpEx(
                            curReferRot,
                            info.referInitRot,
                            info.referInitVec,
                            info.t,
                            info.tx
                        );
                        break;
                    case ALGO_TYPE.TWIST:
                        info.myTransform.localRotation = Twist(
                            curReferRot,
                            info.referInitRot,
                            info.referInitVec,
                            info.tx
                        );
                        break;
                    case ALGO_TYPE.ROT:
                        info.myTransform.localRotation = ROT(
                            curReferRot,
                            info.referInitRot,
                            info.referInitVec,
                            info.initMyRot,
                            info.tx
                        );
                        break;
                    case ALGO_TYPE.CONST:
                        Constrain(info.myTransform, info.initMyRot, info.referTransform);
                        break;
                    case ALGO_TYPE.TITE_FRONT_ROT:
                        curReferRot2 = info.referTransform2.localRotation;
                        info.myTransform.localRotation = TITE_FRONT_ROT(curReferRot, curReferRot2);
                        break;
                    case ALGO_TYPE.TITE_FRONT_TRANS:
                        info.myTransform.localPosition = TITE_FRONT_TRANS(
                            curReferRot,
                            info.initMyPosition
                        );
                        break;
                    case ALGO_TYPE.TITE_BACK_ROT_YZ:
                        curReferRot2 = info.referTransform2.localRotation;
                        info.myTransform.localRotation = TITE_BACK_ROT_YZ(
                            curReferRot,
                            curReferRot2,
                            info.initMyRot
                        );
                        break;
                    case ALGO_TYPE.TITE_BACK_ROT_X:
                        curReferRot2 = info.referTransform2.localRotation;
                        info.myTransform.localRotation = TITE_BACK_ROT_X(
                            curReferRot,
                            curReferRot2,
                            info.initMyRot
                        );
                        break;
                }
            }
        }

        Quaternion TITE_FRONT_ROT(Quaternion curMOMO_LRot, Quaternion curMOMO_RRot)
        {
            var lrot = BoneUtil.GetRot(BoneUtil.rot_order.ROT_XYZ, BoneUtil.ToMatrix(curMOMO_LRot));
            var rrot = BoneUtil.GetRot(BoneUtil.rot_order.ROT_XYZ, BoneUtil.ToMatrix(curMOMO_RRot));

            var angleZ = CALC_TITE_SKIRT_ROT_Z(lrot, rrot, -0.2f, -0.7f, -0.45f, 0f, -100f);

            float angleY = CALC_TITE_SKIRT_FRONT_ROT_Y(
                new Vector3(lrot.x, -lrot.y, -lrot.z),
                new Vector3(rrot.x, -rrot.y, -rrot.z),
                40f
            );

            float angleX = CALC_TITE_SKIRT_ROT_X(lrot, rrot);

            var result =
                Quaternion.AngleAxis(angleZ, Vector3.forward)
                * Quaternion.AngleAxis(angleY, Vector3.up)
                * Quaternion.AngleAxis(angleX, Vector3.right);
            return new Quaternion(result.x, -result.y, -result.z, result.w);
        }

        Quaternion TITE_BACK_ROT_YZ(
            Quaternion curMOMO_LRot,
            Quaternion curMOMO_RRot,
            Quaternion initMyRot
        )
        {
            var lrot = BoneUtil.GetRot(BoneUtil.rot_order.ROT_XYZ, BoneUtil.ToMatrix(curMOMO_LRot));
            var rrot = BoneUtil.GetRot(BoneUtil.rot_order.ROT_XYZ, BoneUtil.ToMatrix(curMOMO_RRot));

            float angleZ = CALC_TITE_SKIRT_ROT_Z(
                lrot,
                rrot,
                -0.9f,
                -0.1f,
                -0.5f,
                float.PositiveInfinity,
                -100f
            );

            float angleY = CALC_TITE_SKIRT_BACK_ROT_Y(
                new Vector3(lrot.x, -lrot.y, -lrot.z),
                new Vector3(rrot.x, -rrot.y, -rrot.z),
                13f
            );

            var result =
                new Quaternion(initMyRot.x, -initMyRot.y, -initMyRot.z, initMyRot.w)
                * Quaternion.AngleAxis(angleZ, Vector3.forward)
                * Quaternion.AngleAxis(angleY, Vector3.up);

            return new Quaternion(result.x, -result.y, -result.z, result.w);
        }

        Vector3 TITE_FRONT_TRANS(Quaternion curRefCalcBoneRot, Vector3 initMyPosition)
        {
            var rot = BoneUtil.GetRot(
                BoneUtil.rot_order.ROT_XYZ,
                BoneUtil.ToMatrix(curRefCalcBoneRot)
            );

            float x;

            if (rot.z <= 0f || rot.z > 90f)
            {
                if (rot.z <= 90f)
                    x = -0.2f;
                else
                    x = -0.05f;
            }
            else
            {
                x = ((Mathf.Sin(rot.z * -Mathf.Deg2Rad) * 9.0f + 10.0f) * 1.7f + 3.0f) * -0.01f;
            }

            return new Vector3(x, initMyPosition.y, initMyPosition.z);
        }

        Quaternion TITE_BACK_ROT_X(
            Quaternion curMOMO_LRot,
            Quaternion curMOMO_RRot,
            Quaternion initMyRot
        )
        {
            Vector3 lrot = BoneUtil.GetRot(
                BoneUtil.rot_order.ROT_XYZ,
                BoneUtil.ToMatrix(curMOMO_LRot)
            );
            Vector3 rrot = BoneUtil.GetRot(
                BoneUtil.rot_order.ROT_XYZ,
                BoneUtil.ToMatrix(curMOMO_RRot)
            );

            var result =
                new Quaternion(initMyRot.x, -initMyRot.y, -initMyRot.z, initMyRot.w)
                * Quaternion.AngleAxis(CALC_TITE_SKIRT_ROT_X(lrot, rrot), Vector3.right);

            return new Quaternion(result.x, -result.y, -result.z, result.w);
        }

        float CALC_TITE_SKIRT_ROT_Z(
            Vector3 curMOMO_LRot,
            Vector3 curMOMO_RRot,
            float strongWeight,
            float weakWeight,
            float meanWeight,
            float upperLimit,
            float lowerLimit
        )
        {
            float angleZ;
            if (curMOMO_LRot.z == curMOMO_RRot.z)
                angleZ = (curMOMO_LRot.z + curMOMO_RRot.z) * meanWeight;
            else if (curMOMO_LRot.z < curMOMO_RRot.z)
                angleZ = (strongWeight * curMOMO_LRot.z) + (weakWeight * curMOMO_RRot.z);
            else
                angleZ = (weakWeight * curMOMO_LRot.z) + (strongWeight * curMOMO_RRot.z);
            return Mathf.Clamp(angleZ, lowerLimit, upperLimit);
        }

        float CALC_TITE_SKIRT_FRONT_ROT_Y(
            Vector3 curMOMO_LRot,
            Vector3 curMOMO_RRot,
            float threshold
        )
        {
            float zL = curMOMO_LRot.z;
            float zR = curMOMO_RRot.z;
            float yL = curMOMO_LRot.y;
            float yR = curMOMO_RRot.y;
            float diffZ = zL - zR;

            float factor;
            float value;

            if (diffZ >= 0f || yL <= 0f)
            {
                if (diffZ <= 0f || yR >= 0f)
                {
                    factor = 0.5f;
                    value = yL + yR;
                }
                else
                {
                    factor = 0.25f;
                    value = yL + Mathf.Abs(zR) * yR / 10f;
                }
            }
            else
            {
                factor = 0.25f;
                value = yR + Mathf.Abs(zL) * yL / 10f;
            }

            float result = factor * value;
            return Mathf.Clamp(result, -threshold, threshold);
        }

        float CALC_TITE_SKIRT_BACK_ROT_Y(
            Vector3 curMOMO_LRot,
            Vector3 curMOMO_RRot,
            float threshold
        )
        {
            float zL = curMOMO_LRot.z;
            float zR = curMOMO_RRot.z;
            float yL = curMOMO_LRot.y;
            float yR = curMOMO_RRot.y;
            float diffZ = zL - zR;

            float value;

            if (diffZ >= 0f || yL <= 0f)
            {
                if (diffZ > 0f && yR < 0f)
                {
                    value = yL * 1.2f;
                }
                else if (diffZ < 0f && yL < 0f)
                {
                    value = (yL + yR) / 0.9f;
                }
                else
                {
                    if (diffZ > 0f && yR > 0f)
                    {
                        value = (yL + yR) / 0.9f;
                    }
                    else
                    {
                        value = (yL + yR) * 0.5f;
                    }
                }
            }
            else
            {
                value = yR * 1.2f;
            }

            return Mathf.Clamp(value, -threshold, threshold);
        }

        float CALC_TITE_SKIRT_ROT_X(Vector3 curMOMO_LRot, Vector3 curMOMO_RRot)
        {
            float diffZ = curMOMO_RRot.z - curMOMO_LRot.z;
            if (diffZ == 2f)
                return 0f;
            return 0.5f * diffZ;
        }

        void Constrain(Transform curMyTransform, Quaternion initMyRot, Transform curRefTransform)
        {
            curMyTransform.localRotation = initMyRot;
            curMyTransform.rotation =
                MathUtil.RotationArc(
                    -curMyTransform.right,
                    curRefTransform.position - curMyTransform.position
                ) * curMyTransform.rotation;
        }

        static class MathUtil
        {
            public static Quaternion Slerp(Quaternion frm, Quaternion to, float t)
            {
                if (Quaternion.Dot(frm, to) < 0f)
                {
                    to = new Quaternion(-to.x, -to.y, -to.z, -to.w);
                }
                return Quaternion.Slerp(frm, to, t);
            }

            public static Quaternion RotationArc(Vector3 v0, Vector3 v1) =>
                Quaternion.FromToRotation(v0, v1);
        }

        enum ALGO_TYPE
        {
            SLERP,
            TWIST,
            ROT,
            CONST,
            TITE_FRONT_ROT,
            TITE_FRONT_TRANS,
            TITE_BACK_ROT_YZ,
            TITE_BACK_ROT_X,
        }

        class CALC_BONE_INFO
        {
            public ALGO_TYPE type;
            public string myPath;
            public Transform myTransform;
            public Quaternion initMyRot;
            public string referPath;
            public Transform referTransform;
            public Quaternion referInitRot;
            public Vector3 referInitVec;
            public float t;
            public float tx;
            public int axis;
            public string referPath2;
            public Transform referTransform2;
            public Quaternion referInitRot2;
            public Vector3 referInitVec2;
            public Vector3 initMyPosition;

            public CALC_BONE_INFO(
                ALGO_TYPE type_,
                string my_,
                string refer_,
                float t_,
                float tx_,
                int axis_
            )
            {
                type = type_;
                myPath = my_;
                referPath = refer_;
                t = t_;
                tx = tx_;
                axis = axis_;
            }

            public CALC_BONE_INFO(
                ALGO_TYPE type_,
                string my_,
                string refer1_,
                string refer2_,
                float t_,
                float tx_,
                int axis_
            )
            {
                type = type_;
                myPath = my_;
                referPath = refer1_;
                referPath2 = refer2_;
                t = t_;
                tx = tx_;
                axis = axis_;
            }

            public CALC_BONE_INFO(
                ALGO_TYPE type_,
                string my_,
                string refer1_,
                string refer2_,
                string refer3_,
                float t_,
                float tx_,
                int axis_
            )
            {
                type = type_;
                myPath = my_;
                referPath = refer1_;
                referPath2 = refer2_;
                t = t_;
                tx = tx_;
                axis = axis_;
            }
        }
    }
}
