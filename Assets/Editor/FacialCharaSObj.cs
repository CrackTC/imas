using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Imas.Facial
{
    public static class FacialCharaSObj
    {
        private const string Path = "Assets/Editor/facial_chara_sobj.asset";
        private static FaceScrObj _Instance = ScriptableObject.CreateInstance<FaceScrObj>();
        private static readonly Dictionary<string, List<(double[] Open, double[] Close)>> _Data = new();

        private static void AddOrCreate(string name, double[] openParams, double[] closeParams)
        {
            if (_Data.TryGetValue(name, out var list))
            {
                list.Add((openParams, closeParams));
            }
            else
            {
                _Data[name] = new() { (openParams, closeParams) };
            }
        }

        [MenuItem("GameObject/Create FacialCharaSObj")]
        public static void CreateFacialCharaSObj()
        {
            _Instance.charaParams = _Data.Select(kv => new FaceBlendParam()
            {
                cid = kv.Key,
                expressions = kv.Value.Select(v => new MeshBlendParam()
                {
                    param32open = v.Open.Select(x => (float)x).ToArray(),
                    param32close = v.Close.Select(x => (float)x).ToArray()
                }).ToArray()
            }).ToArray();

            AssetDatabase.CreateAsset(_Instance, Path);
        }
    }
}
