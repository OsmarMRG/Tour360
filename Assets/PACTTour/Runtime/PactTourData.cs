using System;
using UnityEngine;
namespace Pact.VirtualTour
{
    [CreateAssetMenu(menuName="PACT/Visita 360",fileName="VisitaPACT")]
    public class PactTourData : ScriptableObject
    {
        public PactTourNode[] nodes=Array.Empty<PactTourNode>();
        [Min(0)] public int startNode;
        [Range(.1f,1f)] public float fadeSeconds=.22f;
        public bool narrateOnEntry=true;
        public bool onlyOnRoomChange=true;
        public bool welcomeOnStart=true;
        public string welcomeTitle="Bem-vindo ao PACT";
        [TextArea(3,8)] public string welcomeText;
        public AudioClip welcomeVoice;
    }
    [Serializable]
    public class PactTourNode
    {
        public string title;
        [Tooltip("Mesmo identificador para todas as fotos da mesma sala.")] public string roomId;
        public string panoramaResource;
        public Texture2D panorama;
        [Range(0,360)] public float panoramaRotation=90;
        [TextArea(3,8)] public string narration;
        public AudioClip voice;
        public bool showGuide=true;
        public Vector3 guidePosition=new Vector3(-.85f,0,2.6f);
        public PactTourLink[] links=Array.Empty<PactTourLink>();
        public PactRoomInfo[] roomSigns=Array.Empty<PactRoomInfo>();
    }
    [Serializable]
    public class PactTourLink
    {
        public string label="Continuar";
        [Min(0)] public int targetNode;
        [Range(-180,180)] public float yaw;
        [Range(.8f,5)] public float distance=2.5f;
    }
    [Serializable]
    public class PactRoomInfo
    {
        public string title="Future Lab";
        public string subtitle="Conhecer o espaço";
        [Range(-180,180)] public float yaw=-160;
        public float distance=2.5f;
        public float height=1.55f;
        [TextArea(3,10)] public string narration;
        public AudioClip voice;
    }
}
