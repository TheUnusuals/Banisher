using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

namespace TheUnusuals.Banisher.ExtreStuff
{
    public class SteamVRLaserWrapper : MonoBehaviour
    {
        private SteamVR_LaserPointer steamVrLaserPointer;

        private void Awake()
        {
            steamVrLaserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        }

        private void Start()
        {
            steamVrLaserPointer.PointerIn += OnPointerIn;
            steamVrLaserPointer.PointerOut += OnPointerOut;
            steamVrLaserPointer.PointerClick += OnPointerClick;
        }

        private void OnEnable()
        {
            if (steamVrLaserPointer.holder)
            {
                steamVrLaserPointer.holder.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (steamVrLaserPointer.holder)
            {
                steamVrLaserPointer.holder.SetActive(false);
            }
        }

        private void OnPointerClick(object sender, PointerEventArgs e)
        {
            IPointerClickHandler clickHandler = e.target.GetComponent<IPointerClickHandler>();
            if (clickHandler == null)
            {
                return;
            }

            clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private void OnPointerOut(object sender, PointerEventArgs e)
        {
            IPointerExitHandler pointerExitHandler = e.target.GetComponent<IPointerExitHandler>();
            if (pointerExitHandler == null)
            {
                return;
            }

            pointerExitHandler.OnPointerExit(new PointerEventData(EventSystem.current));
        }

        private void OnPointerIn(object sender, PointerEventArgs e)
        {
            IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
            if (pointerEnterHandler == null)
            {
                return;
            }

            pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
        }
    }
}