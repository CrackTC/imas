using UnityEngine;
using UnityEngine.EventSystems;

namespace Imas
{
    class Overlay : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            ScenarioManager.Instance.Play();
        }
    }
}
