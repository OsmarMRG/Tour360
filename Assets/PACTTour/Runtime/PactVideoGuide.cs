using System.Collections;
using UnityEngine;
using UnityEngine.Video;
namespace Pact.VirtualTour
{
    public class PactVideoGuide : MonoBehaviour
    {
        VideoPlayer player;
        AudioSource sound;
        Material material;
        Transform screen;
        Renderer surface;
        RenderTexture texture;
        bool ended, failed;
        public bool Succeeded { get; private set; }
        public void Build(Shader shader)
        {
            material=new Material(shader);
            screen=PactVisuals.Shape(PrimitiveType.Quad,transform,"Professor_video",Vector3.zero,Vector3.one,material).transform;
            surface=screen.GetComponent<Renderer>();surface.enabled=false;
            surface.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            texture=new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32);texture.Create();
            material.SetTexture("_MainTex",texture);material.SetFloat("_Alpha",0);
            sound=gameObject.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=0;
            player=gameObject.AddComponent<VideoPlayer>();player.playOnAwake=false;player.isLooping=false;
            player.renderMode=VideoRenderMode.RenderTexture;player.targetTexture=texture;
            player.audioOutputMode=VideoAudioOutputMode.AudioSource;
            player.waitForFirstFrame=true;player.skipOnDrop=true;
            player.loopPointReached+=OnEnd;player.errorReceived+=OnError;
        }
        void OnEnd(VideoPlayer source){ended=true;}
        void OnError(VideoPlayer source,string message){failed=true;Debug.LogError("PACT: não foi possível reproduzir o vídeo. "+message,this);}
        public IEnumerator Present(Camera camera,VideoClip clip)
        {
            Stop();Succeeded=false;failed=false;ended=false;
            Vector3 forward=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;
            if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
            screen.position=camera.transform.position+forward*3f-Vector3.Cross(Vector3.up,forward)*.4f-Vector3.up*.45f;
            screen.rotation=Quaternion.LookRotation(forward,Vector3.up);
            float aspect=clip.height>0?(float)clip.width/clip.height:16f/9;
            screen.localScale=new Vector3(2.1f*aspect,2.1f,1);
            player.source=VideoSource.VideoClip;player.clip=clip;
            if(clip.audioTrackCount>0){player.controlledAudioTrackCount=1;player.EnableAudioTrack(0,true);player.SetTargetAudioSource(0,sound);}
            player.Prepare();float deadline=Time.realtimeSinceStartup+15;
            while(!player.isPrepared&&!failed&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!player.isPrepared||failed){Debug.LogWarning("PACT: vídeo indisponível; usada a apresentação provisória.");Stop();yield break;}
            // Limpar o último fotograma para que Repetir não o mostre durante a preparação.
            var previous=RenderTexture.active;RenderTexture.active=texture;GL.Clear(true,true,Color.clear);RenderTexture.active=previous;
            player.Play();surface.enabled=true;
            float start=Time.realtimeSinceStartup;
            double timeout=System.Math.Max(10,clip.length+10);
            while(!ended&&!failed&&Time.realtimeSinceStartup-start<timeout)
            {
                float elapsed=Time.realtimeSinceStartup-start;
                float fadeIn=Mathf.Clamp01(elapsed/.30f);
                float fadeOut=clip.length>0?Mathf.Clamp01((float)(clip.length-player.time)/.30f):1;
                material.SetFloat("_Alpha",Mathf.Min(fadeIn,fadeOut));yield return null;
            }
            Succeeded=ended&&!failed;Stop();
        }
        public void Stop(){if(player)player.Stop();if(sound)sound.Stop();if(surface)surface.enabled=false;if(material)material.SetFloat("_Alpha",0);}
        void OnDisable(){Stop();}
        void OnDestroy()
        {
            if(player){player.loopPointReached-=OnEnd;player.errorReceived-=OnError;}
            if(texture){texture.Release();Destroy(texture);}if(material)Destroy(material);
        }
    }
}
