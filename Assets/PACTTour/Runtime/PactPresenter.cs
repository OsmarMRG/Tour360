using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
namespace Pact.VirtualTour
{
    public class PactPresenter : MonoBehaviour
    {
        Camera view;
        Transform portrait;
        GameObject captionPanel;
        TextMesh caption, heading, location;
        AudioSource audioSource;
        PactAvatar3D avatar;
        PactVideoGuide videoGuide;
        VideoClip welcomeVideo;
        PactTourController owner;
        Transform hud;
        Coroutine speech;
        string lastTitle,lastText;
        AudioClip lastVoice;
        public bool HasPresentation => !string.IsNullOrWhiteSpace(lastText);
        public void Build(Camera camera, Material bodyTemplate, Material headTemplate, Material dark, Material white, PactTourController tour)
        {
            view=camera;owner=tour;
            welcomeVideo=Resources.Load<VideoClip>("PACTTour/Video/boas_vindas");
            Shader videoShader=Resources.Load<Shader>("PACTTour/VideoChroma");
            if(welcomeVideo&&videoShader){videoGuide=gameObject.AddComponent<PactVideoGuide>();videoGuide.Build(videoShader);}
            else Debug.LogWarning("PACT: boas_vindas.mp4 ou VideoChroma.shader em falta. Mantida a apresentação provisória.");
            portrait=new GameObject("Guia_3D_estilizado").transform;portrait.SetParent(transform,false);
            audioSource=portrait.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake=false;audioSource.spatialBlend=.5f;audioSource.minDistance=2;audioSource.maxDistance=12;
            avatar=portrait.gameObject.AddComponent<PactAvatar3D>();avatar.Build(bodyTemplate,headTemplate,audioSource);
            avatar.SetPresence(0);portrait.gameObject.SetActive(false);
            hud=new GameObject("Interface_discreta").transform;hud.SetParent(view.transform,false);
            location=PactVisuals.Text(hud,"Localizacao","PACT  /  Visita virtual",new Vector3(0,.78f,2.2f),.052f,Color.white);
            var panel=new GameObject("Legendas"); panel.transform.SetParent(hud,false);
            panel.transform.localPosition=new Vector3(0,-.68f,2.2f); captionPanel=panel;
            PactVisuals.Shape(PrimitiveType.Cube,panel.transform,"Fundo",Vector3.zero,new Vector3(1.80f,.29f,.01f),dark);
            heading=PactVisuals.Text(panel.transform,"Apresentacao","",new Vector3(0,.088f,-.015f),.045f,new Color(.55f,.86f,.87f));
            caption=PactVisuals.Text(panel.transform,"Legenda","",new Vector3(0,-.03f,-.018f),.049f,Color.white);
            captionPanel.SetActive(false);
            Button(hud,"Repetir",-.57f,PactAction.Replay,tour,dark);
            Button(hud,"Fechar guia",0,PactAction.Stop,tour,dark);
            Button(hud,"Menu",.57f,PactAction.Home,tour,dark);
        }
        void Button(Transform parent,string label,float x,PactAction action,PactTourController tour,Material dark)
        {
            var button=PactVisuals.Shape(PrimitiveType.Cube,parent,label,new Vector3(x,-.90f,2.2f),new Vector3(.50f,.105f,.018f),dark,true);
            var target=button.AddComponent<PactTarget>();target.tour=tour;target.action=action;
            var text=PactVisuals.Text(parent,label+"_texto",label,new Vector3(x,-.90f,2.18f),.034f,Color.white);
            PactVisuals.Fit(text,label,.43f,.075f,.034f);
        }
        public void Location(string title) { PactVisuals.Fit(location,"PACT  /  "+title,2.1f,.12f,.052f); }
        public void Play(string title,string text,AudioClip voice,System.Action after=null)
        {
            Stop(); if(string.IsNullOrWhiteSpace(text))return;
            lastTitle=title;lastText=text;lastVoice=voice;
            if(videoGuide&&welcomeVideo&&owner&&title==owner.data.welcomeTitle)
            {speech=StartCoroutine(PresentWelcomeVideo(title,text,voice,after));return;}
            StartFallback(title,text,voice,after);
        }
        void StartFallback(string title,string text,AudioClip voice,System.Action after)
        {
            Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;
            if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
            Vector3 right=Vector3.Cross(Vector3.up,forward);
            portrait.position=view.transform.position+forward*2.8f-right*.45f-Vector3.up*1.6f;
            portrait.rotation=Quaternion.LookRotation(forward,Vector3.up);
            portrait.gameObject.SetActive(true);captionPanel.SetActive(true);
            PactVisuals.Fit(heading,title+" · voz provisória",1.65f,.06f,.039f);
            speech=StartCoroutine(Present(text,voice,after));
        }
        IEnumerator PresentWelcomeVideo(string title,string text,AudioClip voice,System.Action after)
        {
            // O vídeo já contém áudio. Não sobrepor a voz antiga nem legendas de outro guião.
            captionPanel.SetActive(false);
            yield return videoGuide.Present(view,welcomeVideo);
            speech=null;
            if(videoGuide.Succeeded){after?.Invoke();}
            else StartFallback(title,text,voice,after);
        }
        public void Replay(){if(HasPresentation)Play(lastTitle,lastText,lastVoice);}
        IEnumerator Present(string text,AudioClip voice,System.Action after)
        {
            var pages=Pages(text); int chars=0;foreach(var page in pages)chars+=page.Length;
            PactVisuals.Fit(caption,PactVisuals.Wrap(pages[0],48),1.64f,.16f,.046f);
            yield return Fade(0,1);
            float duration=voice?voice.length:Mathf.Max(4,text.Length/12f);
            if(voice){audioSource.clip=voice;audioSource.Play();}
            foreach(var page in pages)
            {
                PactVisuals.Fit(caption,PactVisuals.Wrap(page,48),1.64f,.16f,.046f);
                yield return new WaitForSecondsRealtime(duration*page.Length/Mathf.Max(1,chars));
            }
            // Não cortar o final do áudio por arredondamentos na distribuição das legendas.
            while(audioSource.isPlaying)yield return null;
            yield return new WaitForSecondsRealtime(.4f);
            yield return Fade(1,0);
            captionPanel.SetActive(false);portrait.gameObject.SetActive(false);speech=null;
            after?.Invoke();
        }
        IEnumerator Fade(float from,float to)
        {
            for(float t=0;t<.35f;t+=Time.unscaledDeltaTime){avatar.SetPresence(Mathf.Lerp(from,to,t/.35f));yield return null;}
            avatar.SetPresence(to);
        }
        public static List<string> Pages(string text)
        {
            var pages=new List<string>();string current="";
            foreach(string word in text.Split(' '))
            {
                if(current.Length+word.Length+1>88&&current.Length>0){pages.Add(current);current="";}
                current+=(current.Length==0?"":" ")+word;
                if(current.Length>32&&(word.EndsWith(".")||word.EndsWith("!")||word.EndsWith("?"))){pages.Add(current);current="";}
            }
            if(current.Length>0)pages.Add(current);
            if(pages.Count==0)pages.Add(text);return pages;
        }
        public void Stop()
        {
            if(speech!=null){StopCoroutine(speech);speech=null;}
            if(videoGuide)videoGuide.Stop();
            if(audioSource)audioSource.Stop();
            if(avatar)avatar.SetPresence(0);
            if(portrait)portrait.gameObject.SetActive(false);
            if(captionPanel)captionPanel.SetActive(false);
        }
        void OnDisable(){Stop();}
        public void SetHUD(bool visible){if(hud)hud.gameObject.SetActive(visible);}
    }
}
