using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace banging_code.ui.main_menu
{
    public class EnableDebugFeaturesCheckbox : MonoBehaviour, IPointerClickHandler 
    {
        public bool isChecked;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            isChecked = !isChecked;
        }
    }
}