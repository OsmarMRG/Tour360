using System.Collections.Generic;
using UnityEngine;
namespace Pact.VirtualTour
{
 public class PactAvatar3D:MonoBehaviour
 {
  readonly List<Material> materials=new List<Material>();
  Transform head,leftArm,rightArm,leftForearm,rightForearm,chest;
  Material headMaterial;AudioSource voice;float[] samples=new float[128];float energy;
  readonly List<Mesh> meshes=new List<Mesh>();
  public void Build(Material surface,Material face,AudioSource source)
  {
   voice=source;
   var suit=ColorMaterial(surface,new Color(.105f,.13f,.17f));
   var trousers=ColorMaterial(surface,new Color(.08f,.095f,.12f));
   var shirt=ColorMaterial(surface,new Color(.91f,.92f,.91f));
   var skin=ColorMaterial(surface,new Color(.61f,.40f,.28f));
   var shoes=ColorMaterial(surface,new Color(.045f,.05f,.055f));
   headMaterial=new Material(face);materials.Add(headMaterial);
   chest=new GameObject("Tronco").transform;chest.SetParent(transform,false);chest.localPosition=Vector3.up*1.03f;
   Tailored(chest,"Casaco",new[]{-.15f,-.10f,.10f,.29f,.39f,.43f},new[]{.185f,.195f,.182f,.218f,.228f,.12f},new[]{.115f,.13f,.13f,.145f,.12f,.075f},suit);
   Part(PrimitiveType.Cube,chest,"Camisa",new Vector3(0,.325f,-.126f),new Vector3(.14f,.215f,.018f),shirt);
   var lapelL=Part(PrimitiveType.Cube,chest,"Lapela_E",new Vector3(-.106f,.34f,-.14f),new Vector3(.07f,.255f,.024f),suit);lapelL.localRotation=Quaternion.Euler(0,0,18);
   var lapelR=Part(PrimitiveType.Cube,chest,"Lapela_D",new Vector3(.106f,.34f,-.14f),new Vector3(.07f,.255f,.024f),suit);lapelR.localRotation=Quaternion.Euler(0,0,-18);
   for(int i=0;i<3;i++)Part(PrimitiveType.Sphere,chest,"Botao",new Vector3(.012f,.1f-i*.09f,-.151f),Vector3.one*.016f,shoes);
   Part(PrimitiveType.Cylinder,transform,"Pescoco",new Vector3(0,1.49f,0),new Vector3(.125f,.09f,.13f),skin);
   head=Part(PrimitiveType.Sphere,transform,"Cabeca_3D_texturada",new Vector3(0,1.69f,-.005f),new Vector3(.245f,.32f,.255f),headMaterial);
   Part(PrimitiveType.Sphere,transform,"Orelha_E",new Vector3(-.12f,1.675f,0),new Vector3(.025f,.067f,.033f),skin);
   Part(PrimitiveType.Sphere,transform,"Orelha_D",new Vector3(.12f,1.675f,0),new Vector3(.025f,.067f,.033f),skin);
   Part(PrimitiveType.Cube,transform,"Anca",new Vector3(0,.88f,0),new Vector3(.32f,.15f,.20f),trousers);
   for(int side=-1;side<=1;side+=2)
   {
    var leg=new GameObject("Perna").transform;leg.SetParent(transform,false);leg.localPosition=new Vector3(side*.092f,0,0);
    Tailored(leg,"Calca",new[]{.12f,.18f,.46f,.74f,.92f},new[]{.066f,.067f,.072f,.088f,.089f},new[]{.075f,.08f,.085f,.10f,.095f},trousers);
    Part(PrimitiveType.Sphere,transform,"Sapato",new Vector3(side*.1f,.075f,-.045f),new Vector3(.155f,.11f,.25f),shoes);
   }
   leftArm=Arm(-1,suit,skin,out leftForearm);rightArm=Arm(1,suit,skin,out rightForearm);
  }
  Material ColorMaterial(Material source,Color color){var m=new Material(source);m.SetColor("_Color",color);materials.Add(m);return m;}
  Transform Part(PrimitiveType type,Transform parent,string name,Vector3 p,Vector3 s,Material m){return PactVisuals.Shape(type,parent,name,p,s,m).transform;}
  Transform Arm(int side,Material suit,Material skin,out Transform forearm)
  {
   var shoulder=new GameObject(side<0?"Ombro_E":"Ombro_D").transform;shoulder.SetParent(transform,false);shoulder.localPosition=new Vector3(side*.228f,1.38f,0);
   Part(PrimitiveType.Capsule,shoulder,"Braco",new Vector3(0,-.14f,0),new Vector3(.135f,.19f,.135f),suit);
   forearm=new GameObject("Cotovelo").transform;forearm.SetParent(shoulder,false);forearm.localPosition=new Vector3(0,-.29f,0);
   Part(PrimitiveType.Capsule,forearm,"Antebraco",new Vector3(0,-.12f,0),new Vector3(.12f,.17f,.12f),suit);
   Part(PrimitiveType.Sphere,forearm,"Mao",new Vector3(0,-.29f,0),new Vector3(.082f,.13f,.052f),skin);
   Part(PrimitiveType.Sphere,forearm,"Polegar",new Vector3(-side*.052f,-.26f,-.004f),new Vector3(.036f,.075f,.04f),skin);
   return shoulder;
  }
  public void SetPresence(float alpha){foreach(var m in materials)m.SetFloat("_Alpha",alpha);}
  void Update()
  {
   if(!head)return;
   bool talking=voice&&voice.isPlaying;float target=0;
   if(talking){voice.GetOutputData(samples,0);foreach(float v in samples)target+=v*v;target=Mathf.Clamp01(Mathf.Sqrt(target/samples.Length)*12);}
   energy=Mathf.Lerp(energy,target,1-Mathf.Exp(-Time.unscaledDeltaTime*18));headMaterial.SetFloat("_Talk",energy);
   float t=Time.unscaledTime;
   // Postura estável: os braços descansam; a fala não provoca gesticulação repetitiva.
   float idle=Mathf.Sin(t*.65f);
   head.localRotation=Quaternion.Euler(talking?Mathf.Sin(t*.95f)*.65f:0,idle*.65f,0);
   chest.localScale=new Vector3(1,1+.0025f*Mathf.Sin(t*1.25f),1);
   leftArm.localRotation=Quaternion.Euler(-2,0,-3);
   rightArm.localRotation=Quaternion.Euler(-2,0,3);
   leftForearm.localRotation=Quaternion.Euler(-7,0,0);
   rightForearm.localRotation=Quaternion.Euler(-7,0,0);
  }
  void Tailored(Transform parent,string name,float[] heights,float[] widths,float[] depths,Material mat)
  {
   // Secções elípticas contínuas: roupa com ombros e cintura, sem tronco esférico.
   const int sides=32;int rows=heights.Length;
   var vertices=new Vector3[rows*sides+2];var triangles=new List<int>();
   for(int row=0;row<rows;row++)for(int j=0;j<sides;j++)
   {
    float a=j*Mathf.PI*2/sides;int k=row*sides+j;
    vertices[k]=new Vector3(Mathf.Cos(a)*widths[row],heights[row],Mathf.Sin(a)*depths[row]);
    if(row<rows-1){int n=row*sides+(j+1)%sides;triangles.AddRange(new[]{k,k+sides,n,n,k+sides,n+sides});}
   }
   int bottom=rows*sides,top=bottom+1;vertices[bottom]=new Vector3(0,heights[0],0);vertices[top]=new Vector3(0,heights[rows-1],0);
   for(int j=0;j<sides;j++){int n=(j+1)%sides;triangles.AddRange(new[]{bottom,j,n,top,(rows-1)*sides+n,(rows-1)*sides+j});}
   var mesh=new Mesh();mesh.name=name;mesh.vertices=vertices;mesh.triangles=triangles.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
   var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
  }
  void OnDestroy(){foreach(var m in materials)if(m)Destroy(m);foreach(var mesh in meshes)if(mesh)Destroy(mesh);}
 }
}
