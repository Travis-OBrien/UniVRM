using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace VRM {
[CustomEditor(typeof(KiteaSetup))]
public class ObjectBuilderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		KiteaSetup kiteaSetup = (KiteaSetup)target;

		if (GUILayout.Button("Generate VRM Components")) {
			kiteaSetup.Build();
		}

		if (GUILayout.Button("Destroy VRM Components")) {
			kiteaSetup.Remove();
		}

		if (GUILayout.Button("Save To Disk VRM Components")) {
			kiteaSetup.Save();
		}

		if (GUILayout.Button("Mirror VRM ColliderGroups Components")) {
			kiteaSetup.Save();
		}
	}
}
}
//@MenuItem ("Component/Kitea/Setup")

// static function Target_asign () {
	
// }

// public class kitea_setup : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
