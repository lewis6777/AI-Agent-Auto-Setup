using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.AI.Navigation;
using GT_CustomMapSupportRuntime;
using GT_CustomMapSupportEditor;

public class AIAgentAutoSetup : EditorWindow
{
    // Made by Ghosty!!
    // If you use this, please give some sort of credit in your map.
    // ^ It's fine if you don't though :)

    /// The aim of this editor script is to help people with the basic setup of AI Agents for their virtual stump maps.
    /// This creates preloaded agents for your map.
    /// If you want to use the agent spawning ability of luau you will need to create your own luau script.
    /// If you encouter any issues while using this, please ping me @Ghosty in the Gorilla Tag Modding Group.

    /// For setting up project.

    private bool showSetup = true;


    private bool addNewAgent = false;
    private string newAgentNameSetup = "Default";
    private bool useCustomSetup = false;
    private bool overrideCustomSetup = true;

    private float setupMoveSpeed = 6f;
    private float setupTurnSpeed = 100f;
    private float setupAcceleration = 10f;

    private int agentIndexSetup = 0;
    private string[] agentNames = { "Humanoid", "Small", "Medium", "Large" };

    /// For adding an Agent after setup.

    private bool showAddAgent = false;
    private string newAgentName = "Default";
    private bool useCustom;
    private bool overrideCustom = true;

    private float moveSpeed = 6f;
    private float turnSpeed = 100f;
    private float acceleration = 10f;

    private int agentIndex = 0;

    [MenuItem("Tools/AI Agent Setup")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(AIAgentAutoSetup));
        window.minSize = new Vector2(350, 540);
    }

    void OnGUI()
    {
        // in v2 ill try give it a scrollview so that it doesnt need large min y size

        GUILayout.Space(8);

        GUILayout.Label("PLEASE READ THE TOOLTIPS! THEY GIVE NEEDED INFO!", EditorStyles.boldLabel);

        showSetup = EditorGUILayout.Foldout(showSetup, "AI Agent Setup Settings");
        if (showSetup)
        {
            GUIContent addAgent = new GUIContent("Add Agent in setup", "This will include an AI Agent in your project setup. You can always add more agents later on in **Add New Agent**.");
            addNewAgent = EditorGUILayout.BeginToggleGroup(addAgent, addNewAgent);
            GUIContent agentName = new GUIContent("Agent Name", "This will be what the Agent is called in Unity.");
            newAgentNameSetup = EditorGUILayout.TextField(agentName, newAgentNameSetup);
            GUIContent navmeshAgentContent = new GUIContent("Agent Type", "Gorilla Tag gives 4 preset Navmesh Agent types to use. You can view the sizes of each in the Functionality Overview scenes.");
            agentIndexSetup = EditorGUILayout.Popup(navmeshAgentContent, agentIndexSetup, agentNames);

            setupMoveSpeed = EditorGUILayout.FloatField("Move Speed", setupMoveSpeed);
            setupTurnSpeed = EditorGUILayout.FloatField("Turn Speed", setupTurnSpeed);
            setupAcceleration = EditorGUILayout.FloatField("Acceleration", setupAcceleration);

            GUIContent useCustomBehaviour = new GUIContent("Use custom behaviour", "This will use the custom AI Agent luau code I made, instead of Gorilla Tag's default.\nREQUIRES GAMEMODE TO BE ON CUSTOM IN GAME!");
            useCustomSetup = EditorGUILayout.BeginToggleGroup(useCustomBehaviour, useCustomSetup);
            GUIContent applyNew = new GUIContent("Apply new gamemode", "This will apply the new generated gamemode to your Map Descriptor if enabled.");
            overrideCustomSetup = EditorGUILayout.Toggle(applyNew, overrideCustomSetup);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.EndToggleGroup();
            if (GUILayout.Button("Setup AI Agents", EditorStyles.miniButton))
            {
                SetupProject();
            }
        }

        GUILayout.Space(8);

        showAddAgent = EditorGUILayout.Foldout(showAddAgent, "Add New Agent");
        if (showAddAgent)
        {
            GUIContent agentName = new GUIContent("Agent Name", "This will be what the Agent is called in Unity.");
            newAgentName = EditorGUILayout.TextField(agentName, newAgentName);
            GUIContent navmeshAgentContent = new GUIContent("Agent Type", "Gorilla Tag gives 4 preset Navmesh Agent types to use. You can view the sizes of each in the Functionality Overview scenes.");
            agentIndex = EditorGUILayout.Popup(navmeshAgentContent, agentIndex, agentNames);

            moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
            turnSpeed = EditorGUILayout.FloatField("Turn Speed", turnSpeed);
            acceleration = EditorGUILayout.FloatField("Acceleration", acceleration);

            GUIContent useCustomBehaviour = new GUIContent("Use custom behaviour", "This will use the custom AI Agent luau code I made, instead of Gorilla Tag's default.\nREQUIRES GAMEMODE TO BE ON CUSTOM IN GAME!");
            useCustom = EditorGUILayout.BeginToggleGroup(useCustomBehaviour, useCustom);
            GUIContent applyNew = new GUIContent("Apply new gamemode", "This will apply the new generated gamemode to your Map Descriptor if enabled.");
            overrideCustom = EditorGUILayout.Toggle(applyNew, overrideCustom);
            EditorGUILayout.EndToggleGroup();
            if (GUILayout.Button("Add New Agent", EditorStyles.miniButton))
            {
                AddAgent(false, agentIndex, newAgentName, useCustom, overrideCustom, moveSpeed, turnSpeed, acceleration);
            }
        }
        GUILayout.Label("Made by Ghosty!");

        GUILayout.Space(8);
        GUILayout.Label("If not using custom behaviour, there are additional values\non the AI Agent you may need to set up.");
        GUILayout.Space(6);
        GUILayout.Label("To add a monster model, OPEN THE PREFAB FILE in\nAssets/Agents and put your monster's model under\nAGENT_MODEL_HERE.");
    }

    void SetupProject()
    {
        // some making stuff isnt set up wrong
        var mapDescriptors = GameObject.FindObjectsOfType<MapDescriptor>();
        if (mapDescriptors.Length < 1)
        {
            Debug.LogError("You don't have a map descriptor!");
            return;
        }
        else if (mapDescriptors.Length > 1)
        {
            Debug.LogError("You have more than one map descriptor!");
            return;
        }
        else if (!mapDescriptors[0].IsInitialScene)
        {
            Debug.LogError("This scene is not the initial scene! AI Agents currently only work in single-zone maps.");
            return;
        }
        MapDescriptor mapDescriptor = mapDescriptors[0];
        // whoo they set their project up correctly
        AISpawnManager existingAISpawnManager = GameObject.FindObjectOfType<AISpawnManager>();
        if (existingAISpawnManager == null)
        {
            GameObject AISpawnManagerObj = new GameObject("AI Spawn Manager");
            AISpawnManagerObj.transform.parent = mapDescriptor.transform;
            AISpawnManagerObj.AddComponent<AISpawnManager>();
        }

        GameObject Agents = GameObject.Find("Agents");
        Agents ??= new GameObject("Agents");
        Agents.transform.parent = mapDescriptor.transform;

        GameObject NavMeshSurfacesObj = GameObject.Find("Navmesh Surfaces");
        NavMeshSurfacesObj ??= new GameObject("Navmesh Surfaces");
        NavMeshSurfacesObj.transform.parent = mapDescriptor.transform;

        Debug.Log("Successfully setup project for AI Agents!");

        if (addNewAgent)
        {
            AddAgent(true, agentIndexSetup, newAgentNameSetup, useCustomSetup, overrideCustomSetup, setupMoveSpeed, setupTurnSpeed, setupAcceleration);
        }
    }

    void AddAgent(bool setup, int agentI, string agentN, bool useCust, bool overrideCust, float mSpeed, float tSpeed, float accel)
    {
        // a bunch more making sure stuff doesnt go wrong
        var mapDescriptors = GameObject.FindObjectsOfType<MapDescriptor>();
        if (mapDescriptors.Length < 1)
        {
            Debug.LogError("You don't have a map descriptor!");
            return;
        }
        else if (mapDescriptors.Length > 1)
        {
            Debug.LogError("You have more than one map descriptor!");
            return;
        }
        else if (!mapDescriptors[0].IsInitialScene)
        {
            Debug.LogError("This scene is not the initial scene! AI Agents currently only work in single-zone maps.");
            return;
        }
        else if (string.IsNullOrEmpty(agentN))
        {
            Debug.LogError("You must set a name for the Agent!");
            return;
        }

        MapDescriptor mapDescriptor = mapDescriptors[0];

        AISpawnManager AISpawnObject = GameObject.FindObjectOfType<AISpawnManager>();
        if (AISpawnObject == null)
        {
            Debug.LogError("AI Spawn Manager not found. Setup AI Agents before trying to add new agents!");
            return;
        }
        GameObject Agents = GameObject.Find("Agents");
        if (Agents == null)
        {
            Debug.LogError("Agents object not found! Did you delete this? Run Setup AI Agents to create the Agents object.");
            return;
        }

        AIAgent[] allAgents = GameObject.FindObjectsOfType<AIAgent>();
        List<int> agentIDs = new List<int>();
        foreach (AIAgent aiAgent in allAgents)
        {
            if (!agentIDs.Contains(aiAgent.enemyTypeId))
            {
                agentIDs.Add(aiAgent.enemyTypeId);
            }
        }

        int selectedID = 0;
        while (agentIDs.Contains(selectedID))
        {
            selectedID++;
        }

        GameObject templateObj = new GameObject(agentN + "_TEMPLATE");
        templateObj.transform.parent = AISpawnObject.transform;

        GameObject modelHere = new GameObject("AGENT_MODEL_HERE");
        modelHere.transform.parent = templateObj.transform;

        // apply those agent properties
        AIAgent templateAgent = templateObj.AddComponent<AIAgent>();
        templateAgent.isTemplate = true;
        templateAgent.enemyTypeId = (byte)selectedID;
        templateAgent.navAgentType = (NavAgentType)agentI;
        templateAgent.movementSpeed = mSpeed;
        templateAgent.turnSpeed = tSpeed;
        templateAgent.acceleration = accel;

        if (!useCust) { templateAgent.agentBehaviours = new List<AgentBehaviours> { AgentBehaviours.Chase, AgentBehaviours.Search }; }

        // make Assets/Agents which is where the prefabs go (and make the prefabs and put them there)
        if (!Directory.Exists("Assets/Agents"))
        {
            Directory.CreateDirectory("Assets/Agents");
            AssetDatabase.Refresh();
        }
        GameObject agentPrefab = PrefabUtility.SaveAsPrefabAsset(templateObj, $"Assets/Agents/{agentN}_TEMPLATE.prefab");
        GameObject.DestroyImmediate(templateObj);
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Agents/{agentN}_TEMPLATE.prefab");
        GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        prefabInstance.transform.parent = AISpawnObject.transform;

        // now we use that prefab and create a new one for the non template

        GameObject nonTemplateObj = (GameObject)PrefabUtility.InstantiatePrefab(agentPrefab);
        nonTemplateObj.name = agentN;
        nonTemplateObj.transform.parent = Agents.transform;

        AIAgent nonTemplateAgent = nonTemplateObj.GetComponent<AIAgent>();
        nonTemplateAgent.isTemplate = false;

        // create and bake a navmesh surface based on their navmesh choice.
        GameObject existingNavmesh = GameObject.Find("Navmesh Surface (" + agentNames[agentI] + ")");
        if (existingNavmesh == null)
        {
            GameObject NavMeshSurfacesObj = GameObject.Find("Navmesh Surfaces");
            NavMeshSurfacesObj ??= new GameObject("Navmesh Surfaces");
            NavMeshSurfacesObj.transform.parent = mapDescriptor.transform;
            GameObject NavmeshSurfaceObj = new GameObject("Navmesh Surface (" + agentNames[agentI] + ")");
            NavmeshSurfaceObj.transform.parent = NavMeshSurfacesObj.transform;
            NavMeshSurface surface = NavmeshSurfaceObj.AddComponent<NavMeshSurface>();
            surface.agentTypeID = NavMesh.GetSettingsByIndex(agentI).agentTypeID;
            surface.BuildNavMesh();
        }

        if (useCust)
        {
            // get gt editor script to generate luau agent ids
            MapToolsMenuButtons.GenerateAgentIDs();

            // ive already added a warning to tell people to set this up correctly
            // hoping i dont get asked how to set this up :sob:
            GameObject pointsObj = GameObject.Find("LuauAgentPoints");
            pointsObj ??= new GameObject("LuauAgentPoints");
            pointsObj.transform.parent = mapDescriptor.transform;

            GameObject agentNumPoints = new GameObject("LuauAgent" + nonTemplateAgent.lua_AgentID.ToString());
            agentNumPoints.transform.parent = pointsObj.transform;

            for (int i = 1; i < 5; i++)
            {
                GameObject agentNumPoint = new GameObject("LuauAgent" + nonTemplateAgent.lua_AgentID.ToString() + "Point" + i.ToString());
                agentNumPoint.transform.parent = agentNumPoints.transform;
            }

            string targetPath = "Assets/Scripts/CustomAgentBehaviour.txt";
            string sourcePath = "Assets/Editor/CustomAgentBehaviour.txt";

            if (!File.Exists(targetPath))
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetPath);
                    AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    // this just happens if someone doesnt import properly or deletes stuff
                    Debug.LogError("Custom Agent Behaviour not found in Assets/Editor. Did you delete this?");
                    return;
                }
            }

            // now we get all the custom gamemode agents and put their ids into the gamemode script
            string ids = "";
            string nums = "";
            AIAgent[] allNewAgents = GameObject.FindObjectsOfType<AIAgent>();
            foreach (AIAgent agent in allNewAgents)
            {
                if ((agent.agentBehaviours == null || agent.agentBehaviours.Count == 0) && !agent.isTemplate)
                {
                    ids += agent.lua_AgentID.ToString() + ", ";
                    nums += "8, ";
                }
            }

            string[] gamemodeLines = File.ReadAllLines(targetPath);
            gamemodeLines[0] = "local agentIDs = { " + ids + "}";
            gamemodeLines[1] = "local sightDists = { " + nums + "}";
            File.WriteAllLines(targetPath, gamemodeLines);
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);

            if (overrideCust)
            {
                // the gamemode.txt takes a bit for unity to realise it exists.
                // to fix this runs every frame for 5 seconds so it will hopefully exist in those 5 secs
                // if it doesnt then uhh that isnt good (it logs error its fine)
                double startTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += WaitForGamemode;

                void WaitForGamemode()
                {
                    TextAsset gamemode = AssetDatabase.LoadAssetAtPath<TextAsset>(targetPath);
                    if (gamemode != null)
                    {
                        mapDescriptor.CustomGamemode = gamemode;
                        EditorApplication.update -= WaitForGamemode;
                    }
                    else if (EditorApplication.timeSinceStartup - startTime > 5.0)
                    {
                        EditorApplication.update -= WaitForGamemode;
                        Debug.LogError("Timed out waiting for CustomAgentBehaviour.txt to be imported as TextAsset :*(");
                        return;
                    }
                }
            }

            // woah double log
            Debug.Log("Gamemode generated! You can find it in Scripts/CustomAgentBehaviour.");
            // v2 probably needs a better way to get people to do this.
            Debug.LogWarning("MAKE SURE to move the Luau Agent Points around, their positions are where the monsters will move between when not chasing a person!");
        }

        // success!
    }
}