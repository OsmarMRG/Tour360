using UnityEngine;
namespace Pact.VirtualTour
{
    public enum PactAction { Navigate, Replay, Stop, Home, RoomInfo, BeginTour }
    public class PactTarget : MonoBehaviour
    {
        public PactAction action;
        public int destination;
        public PactTourController tour;
        public Renderer surface;
        public GameObject hoverLabel;
        public PactRoomInfo roomInfo;
        Vector3 originalScale;
        bool highlighted;
        void Awake() { originalScale=transform.localScale; }
        public void Highlight(bool value)
        {
            if(highlighted==value)return;
            highlighted=value; transform.localScale=originalScale*(value?1.08f:1f);
            if(hoverLabel)hoverLabel.SetActive(value);
        }
        public void Activate()
        {
            if(!tour||tour.Busy)return;
            switch(action)
            {
                case PactAction.BeginTour:tour.BeginTour();break;
                case PactAction.Navigate:tour.GoTo(destination);break;
                case PactAction.Home:tour.ShowMenu();break;
                case PactAction.Replay:tour.Replay();break;
                case PactAction.Stop:tour.StopVoice();break;
                case PactAction.RoomInfo:tour.PresentRoom(roomInfo);break;
            }
        }
    }
}
