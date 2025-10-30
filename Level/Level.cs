using System;
using System.Text.Json;
using OpenTK.Mathematics;

public class Level{

    public Dictionary<string,Actor> ActorCollection {get; set;}
    public Vector3 PlayerStartPosition {get; set;}
    public float PlayerStartRotationAngle {get;set;}
    public Vector3 PlayerStartRotationAxis {get;set;}

    public Level()
    {
        ActorCollection = new Dictionary<string,Actor>();
        PlayerStartPosition = new Vector3(0.0f,0.0f,0.0f);
        PlayerStartRotationAngle = 0;
        PlayerStartRotationAxis = new Vector3(0.0f,1.0f,0.0f);
    }

    public Level(string levelFilePath)
    {
        _levelFilePath = levelFilePath;
        ActorCollection = new Dictionary<string,Actor>();
        PlayerStartPosition = new Vector3(0.0f,0.0f,0.0f);
        PlayerStartRotationAngle = 0;
        PlayerStartRotationAxis = new Vector3(0.0f,1.0f,0.0f);
    }

    public void LoadLevel(Dictionary<string, Mesh> AssetCollection)
    {
        // Leer fichero JSON del nivel
        string ?text = null;
        if(_levelFilePath is not null)
            text = File.ReadAllText(_levelFilePath);
        if((text is not null) && (_jsonOptions is not null))
            _retrievedLevel =  JsonSerializer.Deserialize<RetrievedLevel>(text,_jsonOptions);
        if(_retrievedLevel is null)
            throw new Exception("Error retrieving main level");

        // Cargar assets
        for(int i = 0; i < _retrievedLevel.mesh_list.Length; i++)
        {
            RetrievedMeshMeta meshMeta = _retrievedLevel.mesh_list[i];
            if(meshMeta.file is not null)
               text=File.ReadAllText(meshMeta.file);
            RetrievedMesh ?retMesh = null; // Posible error
            if((text is not null) && (_jsonOptions is not null))
            {
                retMesh =  JsonSerializer.Deserialize<RetrievedMesh>(text,_jsonOptions); 
            }
            if(retMesh is null)
               throw new Exception($"Error retrieving mesh from file {meshMeta.file}");
            Mesh mesh = new Mesh(retMesh);
            mesh.Make();
            AssetCollection.Add(meshMeta.id,mesh);
        }

        // Cargar actores
        for(int i=0;i<_retrievedLevel.actor_list.Length;i++)
        {
            RetrievedActor retActor=_retrievedLevel.actor_list[i];
            Actor actor = new Actor();
            actor.Enabled=retActor.enabled;
            actor.StaticMeshId=retActor.sm;
            actor.SetTransform(
                new Vector3(retActor.position[0],retActor.position[1],retActor.position[2]),
                new Vector3(retActor.orientation.axis[0],retActor.orientation.axis[1],retActor.orientation.axis[2]),
                retActor.orientation.angle, 
                new Vector3(retActor.scale[0],retActor.scale[1],retActor.scale[2]));
            ActorCollection.Add(retActor.id,actor); // Los actores tienen su matriz Model inicializada con la transformación indicada en el JSON
        }
        
        // Cargar posición inicial del jugador
        PlayerStartPosition = new Vector3(_retrievedLevel.playerstartposition[0],_retrievedLevel.playerstartposition[1],_retrievedLevel.playerstartposition[2]);
        PlayerStartRotationAngle = _retrievedLevel.playerstartrotationangle;
        PlayerStartRotationAxis= new Vector3(_retrievedLevel.playerstartrotationaxis[0],_retrievedLevel.playerstartrotationaxis[1],_retrievedLevel.playerstartrotationaxis[2]);
    }

    public List<string> GetActiveMeshes(){
        List<string> activeIds=new List<string>();
        foreach (var keyval in ActorCollection)
        {
            if(keyval.Value.Enabled )
            {
                if(!activeIds.Contains(keyval.Value.StaticMeshId))
                    activeIds.Add(keyval.Value.StaticMeshId);
            }
        }
        return activeIds;
    }

    // Private fields
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private string ?_levelFilePath = "level.json";
    private RetrievedLevel ?_retrievedLevel = null;
    private List<RetrievedMesh> ?_levelMeshes = new List<RetrievedMesh>();
}