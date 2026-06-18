using UnityEngine;

namespace Imas
{
    static class CustomShaderUtility
    {
        public static void SetMaterialToggle(
            this Material material,
            int prop,
            string keyword,
            bool flag
        )
        {
            var value = 1.0f;
            if (!flag)
                value = 0.0f;

            material.SetFloat(prop, value);
            if (flag)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }
    }
}
