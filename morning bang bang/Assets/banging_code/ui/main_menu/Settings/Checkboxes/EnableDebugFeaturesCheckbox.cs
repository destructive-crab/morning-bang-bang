using MothDIed.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace banging_code.ui.main_menu
{
    [RequireComponent(typeof(Image))]
    public class EnableDebugFeaturesCheckbox : MonoBehaviour, IPointerClickHandler
    {
        public PitchedAudio clickSound;
        [SerializeField] private Sprite checkboxChecked;
        [SerializeField] private Sprite checkboxUnchecked;

        public bool IsChecked { get; private set; }
        private Image buttonImage;

        private void Start()
        {
            buttonImage = GetComponent<Image>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clickSound.Play();
            IsChecked = !IsChecked;

            if (IsChecked)
            {
                buttonImage.sprite = checkboxChecked;
            }
            else
            {
                buttonImage.sprite = checkboxUnchecked;
            }
        }
    }
}