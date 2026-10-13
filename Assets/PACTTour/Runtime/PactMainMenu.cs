using UnityEngine;
namespace Pact.VirtualTour
{
 public class PactMainMenu:MonoBehaviour
 {
  Camera view; Material background;Texture2D[] photos;Transform plane;TextMesh status;
  int slide;float elapsed;
  public void Build(Camera camera,Material template,Texture2D[] images,Material buttonMaterial,PactTourController tour)
  {
   view=camera;photos=images;background=new Material(template);
   transform.SetParent(view.transform,false);
   plane=PactVisuals.Shape(PrimitiveType.Quad,transform,"Fotografias_PACT",new Vector3(0,0,4),new Vector3(12,7,1),background).transform;
   Label("Marca","P A C T",.88f,.13f,Color.white,3f);
   Label("Titulo","Descobre o PACT",.45f,.20f,Color.white,3.6f);
   Label("Subtitulo","Uma visita aos espaços, às ideias e às empresas",.08f,.061f,new Color(.8f,.9f,.92f),3.2f);
   var button=PactVisuals.Shape(PrimitiveType.Cube,transform,"Comecar",new Vector3(0,-.38f,3),new Vector3(1.6f,.29f,.025f),buttonMaterial,true);
   var target=button.AddComponent<PactTarget>();target.tour=tour;target.action=PactAction.BeginTour;
   var text=PactVisuals.Text(transform,"Comecar_texto","Começar visita",new Vector3(0,-.38f,2.975f),.08f,new Color(.03f,.07f,.09f));
   PactVisuals.Fit(text,"Começar visita",1.45f,.2f,.08f);
   Label("Instrucoes","Olha em redor · Seleciona os pontos · Conhece cada espaço",-.82f,.045f,Color.white,3.2f);
   status=Label("Estado","3 pontos no corredor  /  Future Lab",-1.03f,.033f,new Color(.6f,.78f,.81f),3f);
   SetSlides();
  }
  TextMesh Label(string name,string value,float y,float height,Color color,float width)
  {var t=PactVisuals.Text(transform,name,value,new Vector3(0,y,3),height,color);PactVisuals.Fit(t,value,width,height*1.6f,height);return t;}
  void SetSlides()
  {
   if(photos==null||photos.Length==0)return;
   var a=photos[slide%photos.Length];var b=photos[(slide+1)%photos.Length];
   background.SetTexture("_TexA",a);background.SetTexture("_TexB",b);
   background.SetFloat("_AspectA",a?(float)a.width/a.height:1.7f);background.SetFloat("_AspectB",b?(float)b.width/b.height:1.7f);
  }
  public void Show(string message="3 pontos no corredor  /  Future Lab")
  {gameObject.SetActive(true);PactVisuals.Fit(status,message,3,.10f,.033f);}
  public void Hide(){gameObject.SetActive(false);}
  void Update()
  {
   if(!background)return;
   elapsed+=Time.unscaledDeltaTime;
   if(elapsed>=6){elapsed-=6;slide++;SetSlides();}
   background.SetFloat("_Blend",Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-5)/1)));
   // Preencher todo o ecrã sem esticar as fotografias.
   float height=2*4*Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad)*1.15f;
   float width=height*Mathf.Max(view.aspect,1);
   plane.localScale=new Vector3(width,height,1);background.SetFloat("_PanelAspect",width/height);
  }
  void OnDestroy(){if(background)Destroy(background);}
 }
}
