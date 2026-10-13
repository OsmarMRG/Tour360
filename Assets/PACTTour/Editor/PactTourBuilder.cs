using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Pact.VirtualTour;

public static class PactTourBuilder
{
    const string Root="Assets/PACTTour";
    [MenuItem("PACT/Criar visita V4")]
    public static void CreateDemo()
    {
        if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("PACT","Sai do Play primeiro.","OK");return;}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        foreach(string name in new[]{"ponto01","ponto02","ponto03"})
            if(!File.Exists(Root+"/Resources/PACTTour/Panoramas/"+name+".jpg"))throw new FileNotFoundException("Falta a fotografia "+name);
        foreach(string shader in new[]{"Skybox/Panoramic","PACT/Flat","PACT/Fade","PACT/AvatarSurface","PACT/AvatarHead","PACT/MenuBackground"})
            if(!Shader.Find(shader))throw new InvalidOperationException("Falta o shader "+shader+". Aguarda a importação completa.");
        string portraitPath=Root+"/Portraits/Professor.png";
        if(!File.Exists(portraitPath))throw new FileNotFoundException("Falta Professor.png. Copia a atualização completa.");
        ConfigureTexture(portraitPath);
        for(int i=1;i<=3;i++){ConfigureTexture(Root+"/Resources/PACTTour/Panoramas/ponto0"+i+".jpg");ConfigureTexture(Root+"/MenuPhotos/pact0"+i+".png");}
        Directory.CreateDirectory(Root+"/Generated");AssetDatabase.Refresh();
        string folder=AssetDatabase.GenerateUniqueAssetPath(Root+"/Generated/VisitaV4");
        Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var sky=MakeMaterial(folder,"Panorama","Skybox/Panoramic",Color.white);
        sky.SetFloat("_Mapping",1);sky.SetFloat("_ImageType",0);sky.SetFloat("_Layout",0);
        sky.SetFloat("_Exposure",1);sky.SetColor("_Tint",Color.gray);sky.SetFloat("_Rotation",90);
        var accent=MakeMaterial(folder,"Marcadores","PACT/Flat",new Color(.88f,.96f,.95f));
        var dark=MakeMaterial(folder,"Interface","PACT/Flat",new Color(.018f,.032f,.04f));
        var fade=MakeMaterial(folder,"Fade","PACT/Fade",Color.black);
        var portrait=MakeMaterial(folder,"Professor","PACT/AvatarHead",new Color(.63f,.43f,.31f));
        var body=MakeMaterial(folder,"Corpo","PACT/AvatarSurface",Color.white);
        var menuMat=MakeMaterial(folder,"Menu","PACT/MenuBackground",Color.white);
        portrait.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(portraitPath));
        var data=ScriptableObject.CreateInstance<PactTourData>();
        data.welcomeTitle="Bem-vindo ao PACT";
        data.welcomeText="Bem-vindo à experiência virtual do PACT. Nesta visita, podes explorar os espaços e conhecer o que acontece em cada sala. Seleciona os anéis no chão para avançar. Quando encontrares a identificação de uma sala, seleciona-a para saber mais. Vamos começar?";
        data.welcomeVoice=Audio("boas_vindas_v2");
        data.onlyOnRoomChange=true;
        var futureLab=new PactRoomInfo {
            title="Future Lab",subtitle="Selecionar para conhecer",yaw=-160,distance=2.5f,height=1.55f,
            narration="Este é o Future Lab, um espaço dedicado à experimentação tecnológica e à criação de novos projetos. Está a ser preparado para reunir computadores, robótica, realidade virtual, impressão 3D e ferramentas de fabrico digital. Aqui será possível testar ideias, desenvolver protótipos e explorar novas aplicações destas tecnologias.",
            voice=Audio("future_lab_v2")
        };
        data.nodes=new[] {
            Node("Corredor","ponto01",new[]{Link("Continuar",1,0)}),
            Node("Corredor","ponto03",new[]{Link("Continuar",2,0),Link("Voltar",0,180)}),
            Node("Corredor","ponto02",new[]{Link("Voltar",1,180)})
        };
        data.nodes[0].roomSigns=new[]{futureLab};
        AssetDatabase.CreateAsset(data,folder+"/VisitaPACT.asset");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("PACT_Visita_V4");
        var rig=new GameObject("Visitante");rig.transform.SetParent(root.transform,false);
        var cameraObject=new GameObject("Main Camera");cameraObject.tag="MainCamera";cameraObject.transform.SetParent(rig.transform,false);
        cameraObject.transform.localPosition=Vector3.up*1.6f;
        var camera=cameraObject.AddComponent<Camera>();camera.nearClipPlane=.03f;camera.farClipPlane=100;camera.fieldOfView=75;
        camera.clearFlags=CameraClearFlags.Skybox;camera.stereoTargetEye=StereoTargetEyeMask.Both;cameraObject.AddComponent<AudioListener>();
        var tour=root.AddComponent<PactTourController>();tour.data=data;tour.viewCamera=camera;
        tour.panoramaTemplate=sky;tour.fadeTemplate=fade;tour.accentMaterial=accent;tour.darkMaterial=dark;tour.portraitTemplate=portrait;
        tour.avatarHead=portrait;tour.avatarSurface=body;tour.menuTemplate=menuMat;
        tour.menuPhotos=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/MenuPhotos/pact0"+i+".png")).ToArray();
        var input=rig.AddComponent<PactTourInput>();input.viewCamera=camera;input.tour=tour;input.pointerMaterial=accent;
        RenderSettings.skybox=sky;
        EditorSceneManager.SaveScene(scene,folder+"/PACT_Visita_V4.unity");AssetDatabase.SaveAssets();Selection.activeObject=root;
        EditorUtility.DisplayDialog("PACT V4","Cena V4 criada. Carrega em Play.\n\nSeleciona Começar visita no menu. O guia dá as boas-vindas e desaparece.\nOlha para trás e para a esquerda para encontrares a placa Future Lab junto à porta C1.01.\nSeleciona a esfera sob a placa para ouvir a apresentação.\n\nA voz ainda é provisória.","OK");
    }
    static PactTourNode Node(string title,string photo,PactTourLink[] links)
    {return new PactTourNode {title=title,roomId="corredor",panorama=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Resources/PACTTour/Panoramas/"+photo+".jpg"),panoramaResource="PACTTour/Panoramas/"+photo,panoramaRotation=90,showGuide=false,narration="",links=links};}
    static PactTourLink Link(string title,int node,float yaw){return new PactTourLink {label=title,targetNode=node,yaw=yaw,distance=2.8f};}
    static AudioClip Audio(string name){return AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/"+name+".wav");}
    static Material MakeMaterial(string folder,string name,string shader,Color color)
    {var mat=new Material(Shader.Find(shader));if(mat.HasProperty("_Color"))mat.SetColor("_Color",color);AssetDatabase.CreateAsset(mat,folder+"/"+name+".mat");return mat;}
    static void ConfigureTexture(string path)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(!importer)throw new InvalidOperationException("Não foi possível importar "+path);
        importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;
        importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
        importer.maxTextureSize=2048;importer.isReadable=false;importer.mipmapEnabled=true;
        importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;
        importer.textureCompression=TextureImporterCompression.CompressedHQ;
        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings {name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.ASTC_6x6,compressionQuality=75});
        importer.SaveAndReimport();
        if(!AssetDatabase.LoadAssetAtPath<Texture2D>(path))throw new InvalidOperationException("Textura indisponível: "+path);
    }
    [MenuItem("PACT/Validar visita selecionada")]
    public static void ValidateSelected()
    {
        var data=Selection.activeObject as PactTourData;
        if(!data){EditorUtility.DisplayDialog("PACT","Seleciona VisitaPACT.asset na janela Project.","OK");return;}
        int errors=0;
        if(data.nodes==null||data.nodes.Length==0){Debug.LogError("A visita não tem pontos.",data);return;}
        if(data.startNode<0||data.startNode>=data.nodes.Length){Debug.LogError("Ponto inicial inválido.",data);errors++;}
        for(int i=0;i<data.nodes.Length;i++)
        {
            var node=data.nodes[i];
            if(node==null){errors++;Debug.LogError("Ponto vazio: "+i,data);continue;}
            if(!node.panorama&&(string.IsNullOrEmpty(node.panoramaResource)||!Resources.Load<Texture2D>(node.panoramaResource))){errors++;Debug.LogError("Falta a fotografia do ponto "+i,data);}
            if(node.links!=null)foreach(var link in node.links)
                if(link==null||link.targetNode<0||link.targetNode>=data.nodes.Length||link.targetNode==i){errors++;Debug.LogError("Destino inválido no ponto "+i,data);}
            if(node.showGuide&&!string.IsNullOrEmpty(node.narration)&&!node.voice)Debug.LogWarning("Ponto "+i+": falta voz.",data);
        }
        EditorUtility.DisplayDialog("PACT",errors==0?"Estrutura sem erros detetados. Confirma o resultado em Play.":errors+" erros. Consulta a Console.","OK");
    }
    [MenuItem("PACT/Adicionar cena aberta ao Build")]
    public static void AddSceneToBuild()
    {
        string path=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if(string.IsNullOrEmpty(path)){EditorUtility.DisplayDialog("PACT","Guarda a cena primeiro.","OK");return;}
        var scenes=EditorBuildSettings.scenes.ToList();var existing=scenes.Find(s=>s.path==path);
        if(existing!=null)existing.enabled=true;else scenes.Add(new EditorBuildSettingsScene(path,true));
        EditorBuildSettings.scenes=scenes.ToArray();
        EditorUtility.DisplayDialog("PACT","Cena adicionada. Confirma também a lista própria do Build Profile, se existir.","OK");
    }
}
