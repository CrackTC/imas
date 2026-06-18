using System.Linq;
using UnityEngine;

namespace Imas
{
    class IdolLayerActor : Actor
    {
        string layerName;
        string GameObjectName;
        bool withItems;

        public IdolLayerActor(Sequence seq)
        {
            cutId = seq.arg1;
            from_cut = int.Parse(seq.arg2);
            to = int.Parse(seq.arg3);
            seqId = seq.seq_id;
            GameObjectName = seq.arg4;
            layerName = seq.arg5;
            withItems = seq.arg6 == "1";
        }

        private void SetLayerRec(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetLayerRec(child.gameObject, layer);
            }
        }

        public override void Exec()
        {
            var character = CharacterManager.GetCharacters(GameObjectName).Single();
            SetLayerRec(character.Body, LayerMask.NameToLayer(layerName));
        }
    }
}
