using System.Collections;
using UnityEngine;
namespace Pact.VirtualTour
{
    public class PactTourController : MonoBehaviour
    {
        public PactTourData data;
        public Camera viewCamera;
        public Material panoramaTemplate,hologramMaterial,accentMaterial,darkMaterial,fadeTemplate;
        public Material portraitTemplate;
        public Material avatarSurface,avatarHead,menuTemplate;
        public Texture2D[] menuPhotos;
        PactMainMenu menu;
        bool resourceOwned;
        public bool Busy {get;private set;}
        public int CurrentNode {get;private set;}=-1;
        public string Status {get;private set;}="A iniciar...";
        Material panorama,fade,previousSky;
        Texture2D currentTexture;
        Transform content;
        PactPresenter presenter;
        string currentRoom;
        bool started;
        void Start()
        {
            if(!data||data.nodes==null||data.nodes.Length==0||!viewCamera||!panoramaTemplate||!fadeTemplate||!accentMaterial||!darkMaterial||!avatarSurface||!avatarHead||!menuTemplate)
            {Status="Abre a cena V4 criada em PACT > Criar visita V4.";Debug.LogError(Status,this);enabled=false;return;}
            panorama=new Material(panoramaTemplate);fade=new Material(fadeTemplate);
            previousSky=RenderSettings.skybox;RenderSettings.skybox=panorama;
            var cover=PactVisuals.Shape(PrimitiveType.Sphere,viewCamera.transform,"Fade_ambos_os_olhos",Vector3.zero,Vector3.one*.4f,fade);
            cover.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            fade.SetFloat("_Alpha",0);
            presenter=gameObject.AddComponent<PactPresenter>();presenter.Build(viewCamera,avatarSurface,avatarHead,darkMaterial,accentMaterial,this);
            presenter.SetHUD(false);
            menu=new GameObject("Menu_PACT").AddComponent<PactMainMenu>();
            menu.Build(viewCamera,menuTemplate,menuPhotos,accentMaterial,this);
            menu.Show();
        }
        public void BeginTour()
        {
            if(Busy)return;
            menu.Hide();presenter.SetHUD(true);CurrentNode=-1;started=false;
            GoTo(Mathf.Clamp(data.startNode,0,data.nodes.Length-1));
        }
        public void ShowMenu()
        {
            if(Busy)return;
            StopVoice();if(content)content.gameObject.SetActive(false);
            presenter.SetHUD(false);menu.Show();
        }
        public void GoTo(int index)
        {
            if(Busy||!data||index<0||index>=data.nodes.Length)return;
            if(index==CurrentNode)return;
            Busy=true;StartCoroutine(ChangeNode(index));
        }
        IEnumerator ChangeNode(int index)
        {
            var node=data.nodes[index];StopVoice();
            if(node==null||(!node.panorama&&string.IsNullOrWhiteSpace(node.panoramaResource)))
            {yield return Failed("Falta o caminho da fotografia.");yield break;}
            Status="A carregar: "+node.title;
            // Bloquear também os alvos antigos durante a troca da panorâmica.
            if(content)content.gameObject.SetActive(false);
            var next=node.panorama;bool loadedResource=false;
            if(!next){var request=Resources.LoadAsync<Texture2D>(node.panoramaResource);yield return request;next=request.asset as Texture2D;loadedResource=true;}
            if(!next){yield return Failed("Fotografia não encontrada: "+node.panoramaResource);yield break;}
            yield return FadeTo(1);
            panorama.SetTexture("_MainTex",next);panorama.SetFloat("_Rotation",node.panoramaRotation);
            Texture2D old=currentTexture;currentTexture=next;if(old&&old!=next&&resourceOwned)Resources.UnloadAsset(old);resourceOwned=loadedResource;
            if(content){content.gameObject.SetActive(false);Destroy(content.gameObject);}
            bool first=!started;
            bool roomChanged=currentRoom!=node.roomId;
            currentRoom=node.roomId;CurrentNode=index;started=true;
            BuildContent(node);presenter.Location(node.title);
            yield return FadeTo(0);Busy=false;Status=node.title;
            if(first&&data.welcomeOnStart)
            {
                presenter.Play(data.welcomeTitle,data.welcomeText,data.welcomeVoice,()=>
                {
                    if(this&&CurrentNode==index&&!Busy&&data.narrateOnEntry&&node.showGuide&&!string.IsNullOrWhiteSpace(node.narration))
                        presenter.Play(node.title,node.narration,node.voice);
                });
            }
            else if(data.narrateOnEntry&&node.showGuide&&(!data.onlyOnRoomChange||roomChanged)&&!string.IsNullOrWhiteSpace(node.narration))
                presenter.Play(node.title,node.narration,node.voice);
        }
        IEnumerator Failed(string message){Status=message;Debug.LogError(message,this);yield return FadeTo(0);Busy=false;presenter.SetHUD(false);menu.Show(message);}
        IEnumerator FadeTo(float target)
        {
            float start=fade.GetFloat("_Alpha"),duration=Mathf.Max(.05f,data.fadeSeconds);
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime){fade.SetFloat("_Alpha",Mathf.Lerp(start,target,t/duration));yield return null;}
            fade.SetFloat("_Alpha",target);
        }
        void BuildContent(PactTourNode node)
        {
            content=new GameObject("Ponto_"+CurrentNode).transform;content.SetParent(transform,false);
            if(node.links!=null)foreach(var link in node.links)
            {
                if(link==null||link.targetNode<0||link.targetNode>=data.nodes.Length)continue;
                var root=new GameObject("Passo_"+link.label).transform;root.SetParent(content,false);
                root.localPosition=Direction(link.yaw)*link.distance;root.localRotation=Quaternion.Euler(0,link.yaw,0);
                PactVisuals.Ring(root,accentMaterial,.20f);
                var hit=root.gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(0,.10f,0);hit.size=new Vector3(.56f,.35f,.56f);
                var target=root.gameObject.AddComponent<PactTarget>();target.tour=this;target.destination=link.targetNode;target.action=PactAction.Navigate;
                var label=PactVisuals.Text(root,"Destino",link.label,new Vector3(0,.23f,0),.048f,Color.white);
                PactVisuals.Fit(label,link.label,.7f,.10f,.048f);label.gameObject.SetActive(false);target.hoverLabel=label.gameObject;
            }
            if(node.roomSigns!=null)foreach(var sign in node.roomSigns)
            {
                if(sign==null)continue;
                var root=new GameObject("Identificacao_"+sign.title).transform;root.SetParent(content,false);
                root.localPosition=Direction(sign.yaw)*sign.distance+Vector3.up*sign.height;
                root.localRotation=Quaternion.Euler(0,sign.yaw,0);
                PactVisuals.Shape(PrimitiveType.Cube,root,"Placa",Vector3.zero,new Vector3(.88f,.25f,.025f),darkMaterial);
                var orb=PactVisuals.Shape(PrimitiveType.Sphere,root,"Apresentar_sala",new Vector3(0,-.30f,-.04f),Vector3.one*.13f,accentMaterial,true);
                var target=orb.AddComponent<PactTarget>();target.tour=this;target.action=PactAction.RoomInfo;target.roomInfo=sign;
                orb.GetComponent<SphereCollider>().radius=1.2f;
                var title=PactVisuals.Text(root,"Nome",sign.title,new Vector3(0,.042f,-.021f),.055f,Color.white);
                PactVisuals.Fit(title,sign.title,.76f,.09f,.055f);
                var hint=PactVisuals.Text(root,"Acao",sign.subtitle,new Vector3(0,-.058f,-.022f),.026f,new Color(.67f,.82f,.83f));
                PactVisuals.Fit(hint,sign.subtitle,.77f,.05f,.026f);
            }
        }
        static Vector3 Direction(float yaw){float a=yaw*Mathf.Deg2Rad;return new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));}
        public void Welcome(){if(presenter&&!Busy)presenter.Play(data.welcomeTitle,data.welcomeText,data.welcomeVoice);}
        public void PresentRoom(PactRoomInfo room){if(!Busy&&room!=null&&presenter)presenter.Play(room.title,room.narration,room.voice);}
        public void Replay(){if(!Busy&&presenter)presenter.Replay();}
        public void StopVoice(){if(presenter)presenter.Stop();}
        void OnDestroy()
        {
            if(RenderSettings.skybox==panorama)RenderSettings.skybox=previousSky;
            if(currentTexture&&resourceOwned)Resources.UnloadAsset(currentTexture);
            if(panorama)Destroy(panorama);if(fade)Destroy(fade);
        }
    }
}
