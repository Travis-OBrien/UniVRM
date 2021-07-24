using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.IO;

namespace VRM {
	public class KiteaSetup : MonoBehaviour {
		[field: SerializeField]
		public string SpringbonesConfigurationFile { get; set; }
		[field: SerializeField]
		public string ColliderGroupsConfigurationFile { get; set; }

		public object GetChild (Transform T, string name){
			foreach (Transform g in transform.GetComponentsInChildren<Transform>()) {
	            if (g.name == name) {
	            	return g;
	            }
	        }
	        return null;
		}

		void GenerateComponents (Transform T){
			// generate collider components -- must be first and available for springbones.

			Dictionary<string, 
							List<Dictionary<string, object>>> colliderGroupsConfigurationFile = loadJsonFile(ColliderGroupsConfigurationFile);

			foreach (string key in colliderGroupsConfigurationFile.Keys) {
				MakeColliderGroupComponent(GetChild(T, key) as Transform, colliderGroupsConfigurationFile[key]);
			}

			// generate springbone components
			Dictionary<string, 
							List<Dictionary<string, object>>> springbonesConfigurationFile = loadJsonFile(SpringbonesConfigurationFile);

			foreach (string key in springbonesConfigurationFile.Keys) {
				foreach(Dictionary<string, object> springbone in springbonesConfigurationFile[key]) {
					float force = (float)(double)springbone["force"];
					Newtonsoft.Json.Linq.JArray rootbones = (Newtonsoft.Json.Linq.JArray)springbone["rootbones"];
					Newtonsoft.Json.Linq.JArray collidergroups = (Newtonsoft.Json.Linq.JArray)springbone["collidergroups"];
					MakeSpringboneComponent(GetChild(T, key) as Transform, force, rootbones, collidergroups);
				}
			}	
		}

		void DestroyAllComponents (Transform T) {
			foreach (Transform g in transform.GetComponentsInChildren<Transform>()) {
				foreach (VRMSpringBone springboneComponent in g.GetComponents(typeof(VRMSpringBone))) {
					DestroyImmediate(springboneComponent);
				}
				foreach (VRMSpringBoneColliderGroup springboneColliderGroupComponent in g.GetComponents(typeof(VRMSpringBoneColliderGroup))) {
					DestroyImmediate(springboneColliderGroupComponent);
				}
	        }
		}

		public void SaveComponentsToJson (Transform T) {
			Dictionary<string, 
							List<Dictionary<string, object>>> springboneComponents = new Dictionary<string, List<Dictionary<string, object>>>();
			Dictionary<string, 
							List<Dictionary<string, object>>> colliderGroupsComponents = new Dictionary<string, List<Dictionary<string, object>>>();

			foreach (Transform g in transform.GetComponentsInChildren<Transform>()) {

				// Springbones
				Component[] vrmSpringBones = g.GetComponents(typeof(VRMSpringBone));
				if (getListSize(vrmSpringBones) > 0) {
					springboneComponents.Add(g.name, new List<Dictionary<string, object>>());
					foreach (VRMSpringBone springboneComponent in vrmSpringBones) {
						Dictionary<string, object> springboneConfiguration = new Dictionary<string, object>();

						springboneConfiguration.Add("force", springboneComponent.m_stiffnessForce);

						List<string> rootBoneNames = new List<string>();
						foreach (Transform rootBone in springboneComponent.RootBones) {
							rootBoneNames.Add(rootBone.name);
						}
						springboneConfiguration.Add("rootbones", rootBoneNames);

						List<string> springBoneColliderGroupNames = new List<string>();
						foreach (VRMSpringBoneColliderGroup springboneCollider in springboneComponent.ColliderGroups) {
							springBoneColliderGroupNames.Add(springboneCollider.name);
						}
						springboneConfiguration.Add("collidergroups", springBoneColliderGroupNames);

						springboneComponents[g.name].Add(springboneConfiguration);
					}
				}
				

				// ColliderGroups
				Component[] vrmColliderGroup = g.GetComponents(typeof(VRMSpringBoneColliderGroup));
				if (getListSize(vrmColliderGroup) > 0) {
					colliderGroupsComponents.Add(g.name, new List<Dictionary<string, object>>());
					foreach (VRMSpringBoneColliderGroup springboneColliderGroupComponent in vrmColliderGroup) {
						//VRMSpringBoneColliderGroup.SphereCollider[] sphereColliders = springboneColliderGroupComponent.Colliders;
						foreach (VRMSpringBoneColliderGroup.SphereCollider sphereCollider in springboneColliderGroupComponent.Colliders) {
							Dictionary<string, object> colliderGroupsConfiguration = new Dictionary<string, object>();
							colliderGroupsConfiguration.Add("radius", sphereCollider.Radius);
							List<float> offsetValues = new List<float>();
							// foreach (float offset in sphereCollider.Offset) {

							// }
							offsetValues.Add(sphereCollider.Offset[0]);
							offsetValues.Add(sphereCollider.Offset[1]);
							offsetValues.Add(sphereCollider.Offset[2]);
							colliderGroupsConfiguration.Add("offset", offsetValues);
							colliderGroupsComponents[g.name].Add(colliderGroupsConfiguration);
						}
						//colliderGroupsConfiguration.Add("radius", sphereCollider.Radius);
						//colliderGroupsConfiguration.Add("offset", sphereCollider.Offset);		
					}
				}
	        }
		
	    	print("SAVING springbones: ");
	    	print(springboneComponents);
	    	JsonSerializerSettings settings = new JsonSerializerSettings();
			// settings.Formatting = Formatting.Indented;
			// settings.ContractResolver = new DictionaryAsArrayResolver();
			string springboneComponentsJson = JsonConvert.SerializeObject(springboneComponents, settings);
			print(springboneComponentsJson);

			string colliderGroupsComponentsJson = JsonConvert.SerializeObject(colliderGroupsComponents, settings);
			print(colliderGroupsComponentsJson);

	    	//string springboneComponentsString = GetLine(springboneComponents);
	    	// Save to disk.
	    	//Application.persistentDataPath
        	//File.WriteAllText("/home/travis/unity_save_test/SpringBonesTest.json", springboneComponentsJson);
        	//File.WriteAllText("/home/travis/unity_save_test/ColliderGroupsTest.json", colliderGroupsComponentsJson);
	    	File.WriteAllText(Path.Combine( Application.persistentDataPath, "SpringBonesTest.json"), springboneComponentsJson);
	    	File.WriteAllText(Path.Combine( Application.persistentDataPath, "ColliderGroupsTest.json"), colliderGroupsComponentsJson);
	    	//print("SAVING collidergroups: ");
	    	//print(colliderGroupsComponents["Head"]);
		}

		static string GetLine(Dictionary<string, List<Dictionary<string, object>>> data)
	    {
	        // Build up the string data.
	        StringBuilder builder = new StringBuilder();
	        foreach (var pair in data)
	        {
	            builder.Append(pair.Key).Append( ":").Append(pair.Value).Append(',');
	        }
	        string result = builder.ToString();
	        // Remove the end comma.
	        result = result.TrimEnd(',');
	        return result;
	    }

	    public int getListSize(object[] objects) {
	    	int i = 0;
	    	foreach (object o in objects) {
	    		i += 1;
	    	}
	    	return i;
	    }

		public void MakeSpringboneComponent(Transform T, float force, Newtonsoft.Json.Linq.JArray rootbones, Newtonsoft.Json.Linq.JArray collidergroups) {
			// Add new SpringBone component to game object.
			VRMSpringBone springBone = T.gameObject.AddComponent<VRMSpringBone>();
			
			// set debugger visualization.
			springBone.m_drawGizmo = true;

			// set force.
			springBone.m_stiffnessForce = force;
			
			// set RootBones.
			foreach (string rootbone in rootbones) {
				springBone.RootBones.Add(GetChild(T, rootbone) as Transform);
			}

			// SETUP COLLIDERS ----------------------------------------------------------------------------------------------------------------------
			// get size of collidergroup
			int colliderGroupSize = 0;
			foreach (string collidergroup in collidergroups) { colliderGroupSize+=1; }
			
			// initialize springbone.ColliderGroups.
			springBone.ColliderGroups = new VRMSpringBoneColliderGroup[colliderGroupSize];
			
			// setup springbone.ColliderGroups.
			int i = 0;
			foreach (string collidergroup in collidergroups) {
				springBone.ColliderGroups[i] = (GetChild(T, collidergroup) as Transform).GetComponent<VRMSpringBoneColliderGroup>();
				i += 1;
			}
		}

		public void MakeColliderGroupComponent(Transform T, List<Dictionary<string, object>> colliderGroups) {
			//float radius, Newtonsoft.Json.Linq.JArray offset
			
			// get size of collider group
			int size = 1;
			foreach(Dictionary<string, object> colliderGroup in colliderGroups) {
				size += 1;
			}

			// setup ColliderGroup Component.
			VRMSpringBoneColliderGroup colliderGroupComponent = T.gameObject.AddComponent<VRMSpringBoneColliderGroup>();
			VRMSpringBoneColliderGroup.SphereCollider[] colliderArray = new VRMSpringBoneColliderGroup.SphereCollider[size]; // {sphereCollider};
			colliderGroupComponent.Colliders = colliderArray;

			int i = 0;
			foreach(Dictionary<string, object> colliderGroup in colliderGroups) {
				float radius = (float)(double)colliderGroup["radius"];
				Newtonsoft.Json.Linq.JArray _offset = (Newtonsoft.Json.Linq.JArray)colliderGroup["offset"];
				Vector3 offset = new Vector3((float)_offset[0], (float)_offset[1], (float)_offset[2]);

				VRMSpringBoneColliderGroup.SphereCollider sphereCollider = new VRMSpringBoneColliderGroup.SphereCollider();
                sphereCollider.Radius=radius;
                sphereCollider.Offset=offset;

		        colliderGroupComponent.Colliders[i] = sphereCollider;//= colliderArray;
				
				i += 1;
			}
		}

		public Dictionary<string, 
						List<Dictionary<string, object>>> loadJsonFile(string path) {

			//File.WriteAllText(Path.Combine( Application.persistentDataPath, "SpringBonesTest.json"), springboneComponentsJson);
	    	//File.WriteAllText(Path.Combine( Application.persistentDataPath, "ColliderGroupsTest.json"), colliderGroupsComponentsJson);

			string text = File.ReadAllText(Path.Combine( Application.persistentDataPath, path + ".json"));

			// Object[] asset = Resources.LoadAll(path);
			// string kek = "";
			// foreach(Object o in asset) {
			// 	kek += o.ToString();
			// }
					
					
			// Dictionary<string, 
			// 	Dictionary<string, 
			// 		List<Dictionary<string, object>>>> dictionary = 
			// 	JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<Dictionary<string, object>>>>>(kek);
			return JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(text);
		}

		public void Build() {
			//print(transform);
			//print(GetChild(transform, "Chest"));
			GenerateComponents(transform);
		}

		public void Remove() {
			DestroyAllComponents(transform);
		}

		public void Save() {
			SaveComponentsToJson(transform);
		}
	}

	// class DictionaryAsArrayResolver : DefaultContractResolver
	// {
	// 	protected override JsonContract CreateContract(Object objectType)
	// 	{
	// 		if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) || 
	// 			(i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
	// 		{
	// 			return base.CreateArrayContract(objectType);
	// 		}
			
	// 		return base.CreateContract(objectType);
	// 	}
	// }
}