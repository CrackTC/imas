using UnityEngine;
using UnityEngine.Assertions;

namespace Imas
{
    class CameraSetPositionLookAtActor : Actor
    {
        Vector3 position;
        Vector3 lookatPosition;

        public CameraSetPositionLookAtActor(Sequence sequence)
        {
            Assert.IsTrue(sequence.arg4 == string.Empty);

            cutId = sequence.arg1;
            from_cut = int.Parse(sequence.arg2);
            to = int.Parse(sequence.arg3);
            seqId = sequence.seq_id;
            position = new Vector3(
                float.Parse(sequence.arg5),
                float.Parse(sequence.arg6),
                float.Parse(sequence.arg7)
            );
            lookatPosition = new Vector3(
                float.Parse(sequence.arg8),
                float.Parse(sequence.arg9),
                float.Parse(sequence.arg10)
            );
        }

        public override void Exec()
        {
            foreach (var gameObject in GameObject.FindGameObjectsWithTag("LayerCamera"))
            {
                if (gameObject.TryGetComponent<Camera>(out var camera))
                {
                    camera.transform.position = position;
                    camera.transform.LookAt(lookatPosition);
                    camera.fieldOfView = 20f;
                }
            }
        }
    }
}
