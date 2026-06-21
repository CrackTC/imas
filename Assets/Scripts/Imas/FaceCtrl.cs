using System;
using UnityEngine;

namespace Imas
{
    [Serializable]
    public class MeshBlendParam
    {
        public float[] param32open;
        public float[] param32close;
    }

    [Serializable]
    public class FaceBlendParam
    {
        public string cid;
        public MeshBlendParam[] expressions;
    }

    enum CharacterEyeMode
    {
        None = -1,
        Blink = 0,
        Open = 1,
        Close = 2,
        Num,
    }

    class FaceCtrl
    {
        const float EYE_BLINK_TIME = 6f / 60;
        const float CHEEK_TRANS_TIME = 6f / 60;
        const float LIP_TRANS_TIME = 12f / 60;
        const float FACE_TRANS_TIME = 8f / 60;
        const float FLAP_OPEN_TIME = 0.125f;
        const float FLAP_CLOSE_TIME = 0.225f;
        const float FLAP_TRANS_TIME = 0.07f;
        const int MOUTH_SHAPES = 14;
        const int EYE_SHAPES = 18;
        static readonly string[] lipChar;
        static readonly string[] facialMenu;
        readonly float[,] lipParam;
        bool singing;
        bool eyesBlink;
        bool talking;
        bool lipOpen;
        bool eyesClose;
        bool lastEyesClose;
        float eyeTransTimer;
        bool eyeToClose;
        bool lipChanged;
        readonly float cheekBaseIntensity;
        int cheekLV;
        int cheeklastLV;
        float cheektranstimer;
        MeshBlendParam[] expressions;
        FaceBlendParam[] paramsrc;
        readonly float[] mouthOpenBuff;
        readonly float[] mouthCloseBuff;
        readonly float[] mouthMixBuff;
        readonly float[] eyeOpenBuff;
        readonly float[] eyeCloseBuff;
        readonly SkinnedMeshRenderer render;
        Material face_material;
        readonly int shaderPropID_IsReddish;
        readonly int shaderPropID_ReddishIntensity;
        float nextflaptime;
        bool mouthopen;
        float flaptranstimer;
        float flapopenscale;
        float nextblinktime;
        float lipNexttime;
        int curLip;
        int nextLip;
        float faceNexttime;
        int curFace;
        int nextFace;
        bool lipTestMode;
        float lipValue;
        int selectedLipEventIdx;
        readonly Character _Character;
        CharacterEyeMode eye_mode;

        static FaceCtrl()
        {
            lipChar = new string[60];
            lipChar[0] = "ア";
            lipChar[1] = "イ";
            lipChar[2] = "ウ";
            lipChar[3] = "エ";
            lipChar[4] = "オ";
            lipChar[5] = "カ";
            lipChar[6] = "キ";
            lipChar[7] = "ク";
            lipChar[8] = "ケ";
            lipChar[9] = "コ";
            lipChar[10] = "サ";
            lipChar[11] = "シ";
            lipChar[12] = "ス";
            lipChar[13] = "セ";
            lipChar[14] = "ソ";
            lipChar[15] = "タ";
            lipChar[16] = "チ";
            lipChar[17] = "ツ";
            lipChar[18] = "テ";
            lipChar[19] = "ト";
            lipChar[20] = "ナ";
            lipChar[21] = "ニ";
            lipChar[22] = "ヌ";
            lipChar[23] = "ネ";
            lipChar[24] = "ノ";
            lipChar[25] = "ハ";
            lipChar[26] = "ヒ";
            lipChar[27] = "フ";
            lipChar[28] = "ヘ";
            lipChar[29] = "ホ";
            lipChar[30] = "マ";
            lipChar[31] = "ミ";
            lipChar[32] = "ム";
            lipChar[33] = "メ";
            lipChar[34] = "モ";
            lipChar[35] = "ヤ";
            lipChar[37] = "ユ";
            lipChar[39] = "ヨ";
            lipChar[40] = "ラ";
            lipChar[41] = "リ";
            lipChar[42] = "ル";
            lipChar[43] = "レ";
            lipChar[44] = "ロ";
            lipChar[45] = "ワ";
            lipChar[49] = "ヲ";
            lipChar[50] = "ン";
            lipChar[54] = "＿";
            lipChar[55] = "ァ";
            lipChar[56] = "ィ";
            lipChar[57] = "ゥ";
            lipChar[58] = "ェ";
            lipChar[59] = "ォ";

            facialMenu = new string[28]
            {
                "通常",
                "明るい",
                "興味",
                "安堵",
                "照れる",
                "微笑み",
                "泣き笑い",
                "困り笑い",
                "真剣",
                "不敵な笑い",
                "企み",
                "考える",
                "きょとん",
                "驚き",
                "超驚き",
                "呆れ",
                "不機嫌",
                "怒り",
                "不安",
                "哀しみ",
                "泣き",
                "痛い",
                "拗ねる",
                "嫌そう",
                "つまらない",
                "がっかり",
                "右ウィンク",
                "左ウィンク",
            };
        }

        public FaceCtrl(Character character)
        {
            lipParam = new float[11, MOUTH_SHAPES];
            singing = true;
            cheekBaseIntensity = 0.2f;
            mouthOpenBuff = new float[MOUTH_SHAPES];
            mouthCloseBuff = new float[MOUTH_SHAPES];
            mouthMixBuff = new float[MOUTH_SHAPES];
            eyeOpenBuff = new float[19];
            eyeCloseBuff = new float[19];

            _Character = character;
            render = character
                .Head.transform.Find("obj_head_GP/face")
                .GetComponent<SkinnedMeshRenderer>();
            shaderPropID_IsReddish = Shader.PropertyToID("_IsReddish");
            shaderPropID_ReddishIntensity = Shader.PropertyToID("_ReddishIntensity");
            curFace = 0;
            nextFace = 0;
            nextblinktime = 1.2f;
            lipNexttime = 0.0f;
            curLip = -1;
            nextLip = -1;
            nextflaptime = 0.0f;
        }

        public void Init()
        {
            var param = Resources.Load<FaceScrObj>("facial_chara_sobj").charaParams;
            paramsrc = param;
            for (int i = 0; i < param.Length; i++)
                if (param[i].cid == _Character.CharacterID)
                    expressions = param[i].expressions;

            for (int i = 0; i < MOUTH_SHAPES; i++)
            {
                mouthOpenBuff[i] = expressions[0].param32open[i];
                mouthCloseBuff[i] = expressions[0].param32close[i];
            }

            for (int i = 0; i < EYE_SHAPES; i++)
            {
                eyeOpenBuff[i] = expressions[0].param32open[MOUTH_SHAPES + i];
                eyeCloseBuff[i] = expressions[0].param32close[MOUTH_SHAPES + i];
            }

            if (cheekLV != 0)
            {
                cheeklastLV = cheekLV;
                cheekLV = 0;
                cheektranstimer = CHEEK_TRANS_TIME;
            }

            eyesClose = false;
            talking = false;
        }

        public void SetCheek(int lv)
        {
            if (cheekLV != lv)
            {
                cheeklastLV = cheekLV;
                cheekLV = lv;
                cheektranstimer = CHEEK_TRANS_TIME;
            }
        }

        public void SetEyeClose(bool sw) => eyesClose = sw;

        public void SetTalking(bool sw)
        {
            if (sw)
                curLip = -1;
            talking = sw;
        }

        public void SetEyesBlink(bool sw)
        {
            eyesBlink = sw;
            if (sw)
                nextblinktime = EYE_BLINK_TIME * 2;
        }

        public void SetLastEyesClose(bool sw) => lastEyesClose = sw;

        public void Init2()
        {
            if (paramsrc != null)
                Init();

            singing = true;
            eyeTransTimer = 0.0f;
            eyeToClose = false;
            lipChanged = false;
            cheektranstimer = 0.0f;
            flaptranstimer = 0.0f;
            lipTestMode = false;
            lipValue = 0.0f;
            lastEyesClose = false;
            eyesBlink = false;
            talking = false;
            lipOpen = false;
            eyesClose = false;
            faceNexttime = 0.0f;
            curFace = 0;
            nextFace = 0;
            nextflaptime = 0.0f;
            cheekLV = 0;
            cheeklastLV = 0;
            nextblinktime = 1.2f;
            lipNexttime = 0.0f;
            curLip = -1;
            nextLip = -1;
            mouthopen = false;
            SetFaceDirectNow(0);
            SetCheekDirect(0);
            eyesClose = false;
            lastEyesClose = false;
            SetFaceDirect(curFace);
            talking = false;
            selectedLipEventIdx = 0;
        }

        public void SetFaceDirectNow(int n)
        {
            SetFace(n);
            faceNexttime = 0.0f;
            curFace = n;
            nextblinktime = 0.0f;

            var expression = expressions[n];
            if (curLip == -1)
            {
                for (int i = 0; i < MOUTH_SHAPES; i++)
                {
                    mouthOpenBuff[i] = expression.param32open[i];
                    mouthCloseBuff[i] = expression.param32close[i];
                }

                if (!lipOpen)
                    for (int i = 0; i < MOUTH_SHAPES; i++)
                        render.SetBlendShapeWeight(i, mouthCloseBuff[i]);
                else
                    for (int i = 0; i < MOUTH_SHAPES; i++)
                        render.SetBlendShapeWeight(i, mouthOpenBuff[i]);
            }

            for (int i = 0; i < EYE_SHAPES; i++)
            {
                eyeOpenBuff[i] = expression.param32open[MOUTH_SHAPES + i];
                eyeCloseBuff[i] = expression.param32close[MOUTH_SHAPES + i];
            }

            if (!eyesClose)
                for (int i = 0; i < EYE_SHAPES; i++)
                    render.SetBlendShapeWeight(MOUTH_SHAPES + i, eyeOpenBuff[i]);
            else
                for (int i = 0; i < EYE_SHAPES; i++)
                    render.SetBlendShapeWeight(MOUTH_SHAPES + i, eyeCloseBuff[i]);

            if (!eyesClose)
                _Character.EyeTracking.scale =
                    100f / expression.param32open[MOUTH_SHAPES + EYE_SHAPES];
            else
                _Character.EyeTracking.scale =
                    100f / expression.param32close[MOUTH_SHAPES + EYE_SHAPES];
        }

        public void SetCheekDirect(int lv)
        {
            cheekLV = lv;
            cheeklastLV = lv;
            cheektranstimer = 0.0f;
            SetCheekValue(
                lv switch
                {
                    < 1 => 0.0f,
                    1 => cheekBaseIntensity / 3,
                    _ => cheekBaseIntensity,
                }
            );
        }

        public void SetEyeCloseDirect(bool sw)
        {
            eyesClose = sw;
            lastEyesClose = sw;
            SetFaceDirect(curFace);
        }

        public int ConvLip(int n)
        {
            if (n == 50)
                return 5;

            if (n == 54)
                return -1;

            if (n is >= 55 and <= 59)
                return n - 49;

            return n % 5;
        }

        public void SetFaceDirect(int n)
        {
            SetFace(n);
            faceNexttime = 0.0f;
            curFace = n;
            nextblinktime = 0.0f;

            var expression = expressions[n];

            if (curLip == -1)
            {
                for (int i = 0; i < MOUTH_SHAPES; i++)
                {
                    mouthOpenBuff[i] = expression.param32open[i];
                    mouthCloseBuff[i] = expression.param32close[i];
                    SelectMouth();
                    render.SetBlendShapeWeight(i, mouthMixBuff[i]);
                }
            }

            var params32 = eyesClose ? expression.param32close : expression.param32open;
            for (int i = 0; i < EYE_SHAPES; i++)
                render.SetBlendShapeWeight(MOUTH_SHAPES + i, params32[MOUTH_SHAPES + i]);

            _Character.EyeTracking.scale = 100f / params32[MOUTH_SHAPES + EYE_SHAPES];
        }

        public void SetLipOpen(bool sw)
        {
            lipOpen = sw;
            lipChanged = true;
        }

        public void SetLipOpenDirect(bool sw)
        {
            lipOpen = sw;
            var expression = expressions[curFace];
            var mouthParams = lipOpen ? mouthOpenBuff : mouthCloseBuff;
            for (int i = 0; i < MOUTH_SHAPES; i++)
            {
                mouthParams[i] = expression.param32open[i];
            }
            SetFaceDirect(curFace);
        }

        public void SetTestLip(float val) => lipValue = val;

        public void SetTestLipEnable(bool enabled) => lipTestMode = enabled;

        public void SetLipDirect(int n, int lipIdx)
        {
            SetLip(n, lipIdx);
            curLip = ConvLip(n);
            lipNexttime = 0.0f;

            if (curLip != -1 && singing)
            {
                for (int i = 0; i < MOUTH_SHAPES; i++)
                    render.SetBlendShapeWeight(i, lipParam[curLip, i]);
            }
            else
            {
                lipChanged = true;
                for (int i = 0; i < MOUTH_SHAPES; i++)
                    render.SetBlendShapeWeight(i, expressions[curFace].param32close[i]);
            }
        }

        public void SetLip(int n, int lipIdx)
        {
            if (singing && (n == -1 || selectedLipEventIdx == lipIdx))
            {
                var convLip = ConvLip(n);
                if (nextLip != convLip)
                {
                    if (curLip != nextLip)
                        curLip = nextLip;
                    nextLip = convLip;
                    lipNexttime = LIP_TRANS_TIME;
                }
            }
        }

        public void SetSinging(bool sw)
        {
            if (!sw && singing && nextLip != -1)
            {
                if (curLip != nextLip)
                {
                    curLip = nextLip;
                }
                nextLip = -1;
                lipNexttime = LIP_TRANS_TIME;
            }

            singing = sw;
        }

        public void SetFace(int n)
        {
            if (nextFace == n)
                return;

            if (curFace != nextFace)
            {
                faceNexttime = 0.0f;
                curFace = nextFace;

                var expression = expressions[curFace];
                for (int i = 0; i < MOUTH_SHAPES; i++)
                {
                    mouthOpenBuff[i] = expression.param32open[i];
                    mouthCloseBuff[i] = expression.param32close[i];
                }

                SelectMouth();

                for (int i = 0; i < EYE_SHAPES + 1; i++)
                {
                    eyeOpenBuff[i] = expression.param32open[MOUTH_SHAPES + i];
                    eyeCloseBuff[i] = expression.param32close[MOUTH_SHAPES + i];
                }
            }

            nextFace = n;
            faceNexttime = FACE_TRANS_TIME;
        }

        public void SelectMouth()
        {
            if (!lipOpen)
            {
                for (int i = 0; i < MOUTH_SHAPES; i++)
                    mouthMixBuff[i] = mouthCloseBuff[i];
            }
            else
            {
                for (int i = 0; i < MOUTH_SHAPES; i++)
                    mouthMixBuff[i] = mouthOpenBuff[i];
            }
        }

        public void FaceCalc()
        {
            bool faceBuffChanged;
            if (faceNexttime == 0.0f)
            {
                faceBuffChanged = false;
            }
            else
            {
                faceBuffChanged = true;
                faceNexttime -= Time.deltaTime;
                if (faceNexttime <= 0.0f)
                {
                    faceNexttime = 0.0f;
                    curFace = nextFace;
                }
                float percent = 1.0f - faceNexttime / FACE_TRANS_TIME;

                for (int i = 0; i < MOUTH_SHAPES; i++)
                {
                    mouthOpenBuff[i] = Mathf.Lerp(
                        expressions[curFace].param32open[i],
                        expressions[nextFace].param32open[i],
                        percent
                    );
                    mouthCloseBuff[i] = Mathf.Lerp(
                        expressions[curFace].param32close[i],
                        expressions[nextFace].param32close[i],
                        percent
                    );
                }

                SelectMouth();
                if (curLip == -1)
                    lipChanged = true;

                for (int i = 0; i < EYE_SHAPES + 1; i++)
                {
                    eyeOpenBuff[i] = Mathf.Lerp(
                        expressions[curFace].param32open[MOUTH_SHAPES + i],
                        expressions[nextFace].param32open[MOUTH_SHAPES + i],
                        percent
                    );
                    eyeCloseBuff[i] = Mathf.Lerp(
                        expressions[curFace].param32close[MOUTH_SHAPES + i],
                        expressions[nextFace].param32close[MOUTH_SHAPES + i],
                        percent
                    );
                }

                var eyeBuff = eyesClose ? eyeCloseBuff : eyeOpenBuff;
                _Character.EyeTracking.scale = 100f / eyeBuff[EYE_SHAPES];
            }

            EyesBlink(faceBuffChanged);

            if (curLip != -1)
                return;

            if (!lipTestMode)
            {
                if (talking == false)
                {
                    SelectMouth();
                    return;
                }

                if (nextflaptime <= 0.0f)
                {
                    flaptranstimer = FLAP_TRANS_TIME;
                    var mouthopen = this.mouthopen;
                    this.mouthopen = !mouthopen;
                    if (!mouthopen)
                    {
                        flapopenscale = UnityEngine.Random.Range(0.2f, 1.0f);
                        nextflaptime = FLAP_OPEN_TIME;
                    }
                    else
                    {
                        nextflaptime = FLAP_CLOSE_TIME;
                    }
                }

                if (flaptranstimer != 0.0f)
                {
                    if (flaptranstimer < 0.0f)
                        flaptranstimer = 0.0f;
                    float percent;
                    if (mouthopen)
                        percent = 1.0f - flaptranstimer / FLAP_TRANS_TIME;
                    else
                        percent = flaptranstimer / FLAP_TRANS_TIME;

                    for (int i = 0; i < MOUTH_SHAPES; i++)
                    {
                        mouthMixBuff[i] = Mathf.Lerp(
                            mouthCloseBuff[i],
                            Mathf.Lerp(mouthCloseBuff[i], mouthOpenBuff[i], flapopenscale),
                            percent
                        );
                    }

                    if (flaptranstimer > 0.0f)
                        flaptranstimer -= Time.deltaTime;
                }

                nextflaptime -= Time.deltaTime;
            }
            else
                for (int i = 0; i < MOUTH_SHAPES; i++)
                    mouthMixBuff[i] = Mathf.Lerp(mouthCloseBuff[i], mouthOpenBuff[i], lipValue);
        }

        void EyesBlink(bool faceBuffChanged)
        {
            if (eyesBlink == false)
            {
                if (eyesClose != lastEyesClose)
                {
                    eyeTransTimer = EYE_BLINK_TIME;
                    lastEyesClose = eyesClose;
                    eyeToClose = eyesClose;
                }

                if (eyeTransTimer <= 0.0f && !faceBuffChanged)
                    return;

                eyeTransTimer -= Time.deltaTime;

                if (eyeTransTimer < 0.0f)
                    eyeTransTimer = 0.0f;

                if (eyeToClose)
                {
                    SetFaceBlink(1 - eyeTransTimer / EYE_BLINK_TIME);
                    return;
                }
                else
                {
                    SetFaceBlink(eyeTransTimer / EYE_BLINK_TIME);
                    return;
                }
            }
            else
            {
                nextblinktime -= Time.deltaTime;
                if (nextblinktime <= 0.0f)
                {
                    SetFaceBlink(0.0f);
                    nextblinktime = UnityEngine.Random.Range(4.0f, 5.0f);
                    return;
                }

                if (nextblinktime > EYE_BLINK_TIME)
                {
                    if (nextblinktime >= EYE_BLINK_TIME * 2)
                        SetFaceBlink(0.0f);
                    else
                        SetFaceBlink((EYE_BLINK_TIME * 2 - nextblinktime) / EYE_BLINK_TIME);
                }
                else
                    SetFaceBlink(nextblinktime / EYE_BLINK_TIME);
            }
        }

        void LipCalc()
        {
            if (lipTestMode)
            {
                for (int i = 0; i < MOUTH_SHAPES; i++)
                    render.SetBlendShapeWeight(i, mouthMixBuff[i]);

                lipChanged = false;
                return;
            }

            if (lipNexttime != 0.0f)
            {
                lipNexttime -= Time.deltaTime;
                if (lipNexttime <= 0.0f)
                {
                    lipNexttime = 0.0f;
                    curLip = nextLip;
                    for (int i = 0; i < MOUTH_SHAPES; i++)
                    {
                        if (nextLip == -1)
                            render.SetBlendShapeWeight(i, mouthMixBuff[i]);
                        else
                            render.SetBlendShapeWeight(i, lipParam[curLip, i]);
                    }
                }
                else
                {
                    var percent = 1 - lipNexttime / LIP_TRANS_TIME;
                    for (int i = 0; i < MOUTH_SHAPES; i++)
                    {
                        var curWeight = curLip == -1 ? mouthMixBuff[i] : lipParam[curLip, i];
                        var nextWeight = nextLip == -1 ? mouthMixBuff[i] : lipParam[nextLip, i];
                        render.SetBlendShapeWeight(i, Mathf.Lerp(curWeight, nextWeight, percent));
                    }
                }

                lipChanged = false;
                return;
            }

            if ((curLip != -1 || !talking) && !lipChanged)
                return;

            for (int i = 0; i < MOUTH_SHAPES; i++)
                render.SetBlendShapeWeight(i, mouthMixBuff[i]);

            lipChanged = false;
        }

        void SetFaceBlink(float val)
        {
            for (int i = 0; i < EYE_SHAPES; i++)
                render.SetBlendShapeWeight(
                    MOUTH_SHAPES + i,
                    Mathf.Lerp(eyeOpenBuff[i], eyeCloseBuff[i], val)
                );
        }

        void CheekCalc()
        {
            if (cheektranstimer == 0.0f)
                return;

            cheektranstimer -= Time.deltaTime;
            if (cheektranstimer <= 0.0f)
            {
                cheektranstimer = 0.0f;
                SetCheekValue(GetCheekValue(cheekLV));
            }
            else
            {
                SetCheekValue(
                    Mathf.Lerp(
                        GetCheekValue(cheeklastLV),
                        GetCheekValue(cheekLV),
                        1 - cheektranstimer / CHEEK_TRANS_TIME
                    )
                );
            }
        }

        float GetCheekValue(int lv) =>
            lv switch
            {
                <= 0 => 0.0f,
                1 => cheekBaseIntensity / 2,
                _ => cheekBaseIntensity,
            };

        void SetCheekValue(float val)
        {
            var sharedMaterial = render.sharedMaterial;
            if (face_material != null)
            {
                sharedMaterial = face_material;
            }

            if (sharedMaterial != null)
            {
                sharedMaterial.SetFloat(shaderPropID_ReddishIntensity, val);
                sharedMaterial.SetMaterialToggle(
                    shaderPropID_IsReddish,
                    "_ISREDDISH_ON",
                    val != 0.0f
                );
            }
        }

        public void Action()
        {
            FaceCalc();
            LipCalc();
            CheekCalc();
        }

        void SetFaceMaterial(Material material) => face_material = material;

        public void SetEyeMode(CharacterEyeMode mode, bool isInterpolate)
        {
            bool eyeClose;
            switch (mode)
            {
                case CharacterEyeMode.Blink:
                    SetEyesBlink(true);
                    SetLastEyesClose(false);
                    eyeClose = false;
                    break;
                case CharacterEyeMode.Open:
                    SetEyesBlink(false);
                    eyeClose = false;
                    break;
                case CharacterEyeMode.Close:
                    SetEyesBlink(false);
                    eyeClose = true;
                    break;
                default:
                    eyeClose = false;
                    break;
            }

            if (isInterpolate)
            {
                SetEyeClose(eyeClose);
            }
            else
            {
                SetEyeCloseDirect(eyeClose);
            }
        }
    }
}
