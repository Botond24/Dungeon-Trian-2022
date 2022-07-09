﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrennanHatton.Props
{

	public class PropType : MonoBehaviour
	{
		//Prop data class
		[System.Serializable]
		public class PropData : ChanceMultiplier
		{
			public Prop prop;
			bool Debugging;
			PropType type;
			int propDataId = 0;
			static int propDataCounter = 0;
			
			//Pools track which objects are in use.
			//[HideInInspector]
			public bool inUse
			{
				get{
					
					/*if(Debugging)
					Debug.Log("inUse was 'get' for " + prop.name + " it is now "+_inUse + "  prop id:" + prop.gameObject.GetInstanceID() + "   data id:" + propDataId + " on type:" + type.name);*/
						
					return _inUse;
				}
					
				set{
					
					if(Debugging)
						Debug.LogError("inUse for " + prop.name + " set from "+_inUse+" to " + value + "  id:" + prop.gameObject.GetInstanceID() + "   data id:" + propDataId + " on type:" + type.name);
						
					_inUse = value;
					
					if(Debugging)
						Debug.Log("inUse for " + prop.name + " is now "+_inUse + "  id:" + prop.gameObject.GetInstanceID() + "   data id:" + propDataId + " on type:" + type.name);
					
				}
			}
			bool _inUse = false;
			
			public PropData(Prop _prop, bool _debugging, PropType _type)
			{
				prop = _prop;
				Debugging = _debugging;
				
				propDataId = propDataCounter;
				propDataCounter++;
				type = _type;
			}
		}
		//A list of props and data
		public List<PropData> propData = new List<PropData>();
		
		//will this only make clones, rather than pool
		public bool cloneOnly = false;
		
		//matrix for calculating chane based on mutliplier.
		int[] _propChanceMatrix;
		int[] propChanceMatrix{
			get{
				
				if(_propChanceMatrix == null
				#if UNITY_EDITOR
					|| Application.isPlaying == false
				#endif
				)
				{
					_propChanceMatrix = ChanceMultiplier.CreateChanceMatrix(propData.ToArray());
				}
				
				
				if(_propChanceMatrix.Length == 0)
					Debug.Log("No prop data");
				
				return _propChanceMatrix;
			}
		}
		
		//props found as children
		[SerializeField]
		List<Prop> props;
		public List<Prop> unconvertedProps
		{
			get{
				return props;
			}
		}
		
		public PropType parentType;
		public PropType[] subTypes;
		
		
		public bool Debugging;
		
		//Prop[] originalClones;
		
		#if UNITY_EDITOR
		void Reset()
		{
			//create a new list of PropData
			propData = new List<PropData>();	
			
			//get alist of props in children
			props = new List<Prop>();
			props.AddRange(this.GetComponentsInChildren<Prop>());
		}
		#endif
		
		/// <summary>
		/// Repopulate data from children
		/// </summary>
		void RefreshList()
		{
			
			//create a new list of PropData
			propData = new List<PropData>();	
			
			//get alist of props in children
			props = new List<Prop>();
			props.AddRange(this.GetComponentsInChildren<Prop>());
			
			ConvertPropsToPropData();
			
			parentType = GetTypeParent(this.transform.parent);
			subTypes = GetSubTypes(this.transform).ToArray();
			
		}
		
		//returns first proptype found in parent hierarchy
		PropType GetTypeParent(Transform testSubject)
		{
			//check if we are at the root of the parent tree
			if(testSubject == transform.root)
				//Looks like there is no parent propType
				return null;
			
			//Look for propType on testsubject
			PropType retVal = testSubject.gameObject.GetComponent<PropType>();
			
			//if has a propType
			if(retVal != null)
				//this is it! We found it.
				return retVal;
				
			//look higher!
			return GetTypeParent(testSubject.parent);
		}
		
		//finds all subtypes in children, but not in the children of those subtypes
		//will explore children until first proptype or end of child hierarchy
		List<PropType> GetSubTypes(Transform parent)
		{
			PropType propTypeFound;
			List<PropType> propTypesFound = new List<PropType>();
			
			//for all children
			for(int i = 0; i < parent.childCount; i++)
			{
				//look for prop type on child
				propTypeFound = parent.GetChild(i).gameObject.GetComponent<PropType>();
				
				//if propType found
				if(propTypeFound != null)
				{
					//add to list
					propTypesFound.Add(propTypeFound);
				}
				else
				{
					//see if there are propTypes in its children
					propTypesFound.AddRange(GetSubTypes(parent.GetChild(i)));
				}
			}
			
			return propTypesFound;
		}
		
		void ConvertPropsToPropData()
		{
			//add props list to prop data
			//for all props from children
			for(int i = 0; i < props.Count; i++)
			{
				if(props[i] == null)
				{
					Debug.LogError("Corrupt Prop Data: " + this.gameObject.name);
				}
				//check if it is not alread existing in prop data
				else if(DoesPropDataContain(props[i]) == false)
				{
					//convert prop to prop data
					PropData newPropData = new PropData(props[i], Debugging, this);
					
					//add to propdata list
					propData.Add(newPropData);
				}
			}
			
			if(props.Count > 0)
				props = new List<Prop>();
		}
		
		/// <summary>
		/// Does the prop data contain the provided prop
		/// </summary>
		/// <param name="prop">Prop in question</param>
		/// <returns>true if that prop is in the prop data</returns>
		public bool DoesPropDataContain(Prop prop)
		{
			//for all of prop data
			for(int i = 0; i < propData.Count; i++)
			{
				if(propData[i].prop == prop)
					return true;
			}
			
			return false;
		}
		
		void Awake()
		{
			//check for missing references
			for(int i = 0; i < propData.Count; i++)
			{
				
				if(propData[i] == null)
				{
					Debug.LogWarning("Missing reference in list " + this.gameObject.name + ". Check out your propType list");
					RefreshList();
					i = propData.Count;
				}
			}
			
			//add all editor values to data
			ConvertPropsToPropData();
			
			//turn off all propData
			for(int i = 0; i < propData.Count; i++)
			{
				//if the prop is not in use
				if(!propData[i].inUse)
				{
					propData[i].prop.gameObject.SetActive(false);
					propData[i].prop.Initialization();// This should probably be called elsewhere, but for some reasn this makes the most sense for now
				}
			}
			
			if(parentType == null)
				parentType = GetTypeParent(this.transform.parent);
			if(subTypes == null || subTypes.Length == 0)
				subTypes = GetSubTypes(this.transform).ToArray();
		}
		
		/*void Update()
		{
			if(Debugging)
				for(int i = 0; i < propData.Count; i++)
				{
					Debug.Log("inUse for " + propData[i].prop.name + " is "+propData[i].inUse + "  id:" + propData[i].prop.gameObject.GetInstanceID());
				}
		}*/
		
		public Prop GetProp()
		{
			//conver data over from old data types
			ConvertPropsToPropData();//this should probably be run as an editor tool, so its not needed at runtime. Do this n the future. //TODO
			
			//if this prop only clones
			if(cloneOnly)
			{
				//create a new one and exit.
				return CreateNewProp();
			}
			//if no props in data
			if(propData.Count == 0)
			{
				//send message to dev
				Debug.LogError("Empty Prop Count '" + this.gameObject.name+"'. Prop type might need a refresh, or missing prop componet on children");
				
				return null;
			}
			
			//gets prop that considers chance values assigned to props
			int id = GetRandomPropId();
			
			//check if that prop exists
			if(propData[id] == null)
				Debug.LogError("Missing Prop Refernece on PropType - " + this.gameObject.name);
			
			//counter to make sure loop doent go for ever
			int counter = 0;
			
			bool newMade = false;
			//if object is in use
			///	TODO			--- This could be optamizaed with a list<int> of ids of objects unused
			while(propData[id].inUse && newMade == false)
			{
				//increase counter
				counter++;
				
				//go through objects looking for free object
				id = (id + counter) % propData.Count;
				
				//until we exaust all objects
				if(counter >= propData.Count)
				{
					//than make a new one
					Prop newProp = CreateNewProp();
					id = -1;
					for(int i = 0; i < propData.Count; i++)
					{
						if(propData[i].prop == newProp)
						{
							id = i;
							i =  propData.Count;
						}
					}
					
					newMade = true;
				}
			}
			
			//if propdata is empty
			if(propData[id] == null)
				// let developer know there was an issue
				Debug.LogError("Null prop in list - " + this.gameObject.name);
			
			SetPropInUse(id, true, true, true);
			
			//sets up start 
			//propData[id].prop.Initialization();
			
			return propData[id].prop;
		}
		
		void SetPropInUse(int id, bool inUse, bool children, bool parents)
		{
			//something has asked for this prop, its now in use.
			propData[id].inUse = inUse;
			
			//tell the parent type it is in use
			if(parents && parentType != null)
				parentType.SetPropInUse(propData[id].prop, inUse, false , true);
			
			if(children)
			{
				//for all subtypes
				for(int i = 0; i < subTypes.Length; i++)
				{
					//set inUse
					subTypes[i].SetPropInUse(propData[id].prop, inUse, true, false);
				}
			}
		}
		
		public void SetPropInUse(Prop prop, bool inUse, bool children, bool parents)
		{
			
			for(int i = 0; i < propData.Count; i++) //TODO this loop could be replaced with a dictionary matrix to speed things up. It is probably a good idea as this will be used alot.
			{
				//if we found the right prop
				if(propData[i].prop == prop)
				{
					//set it to be in use
					SetPropInUse(i, inUse, children, parents);
					
					//exit function
					return;
				}
			}
		}
		
		int idToCopy = 0;
		public Prop CreateNewProp()
		{
			//ToDO: Objects are reset when duplicated, copy over start state - I think I did this
			
			//create new prop
			Prop newProp = Instantiate(propData[idToCopy % propData.Count].prop, this.transform);
			
			
			//rename to name of old prop
			newProp.name = propData[idToCopy % propData.Count].prop.name;
			
			
			//subplacers setup and states
			newProp.Initialization();
			
			
			//newProp.RestoreStartState(); This is called in ReturnToPool. Dont delete this comment, you may forget where it is.
			
			//if it is a pool
			if(cloneOnly == false)
			{
				//create prop data
				PropData newPropData = new PropData(newProp, Debugging, this);
				
				newPropData.chanceMultiplier = (int)(newPropData.chanceMultiplier / 10); //quick and easy matrix hack to stop effects being huge
				if(newPropData.chanceMultiplier == 0) newPropData.chanceMultiplier = 1; 
				
				//store in pool
				propData.Add(newPropData);
				
				//matrix is not updated, so its chance is not considered. Instead this item is only accessed if other items are all  used. This results in a skque of the matrix chance in favor of the first item, more so for smaller pools. This could be countered by having the copy reduce the chance of the original, and share the chance between the original and clone. This would require a reclacuation of chance matrix, and could present issues for cases that can no be split (chance value of 1). This could be mitigated by doubling everything else rather than halfing itself.
				//alternatively it could be calculated entirely based on maxtrix and not on props left unused.
				
			}
			
			
			//returns to the pool, and returns all subob
			//newProp.ReturnToPool(); - this is called in place(this). Dont delete this comment, you may forget where it is.
				
			//next time, copy a different object - if implmenting the above advice, this could only be chanced when the chance value has reached 1.
			idToCopy++;
			
			return newProp;
		}
		
		/// <summary>
		/// Gets random prop id using chance matrix
		/// </summary>
		/// <returns>id of random prop</returns>
		public int GetRandomPropId()
		{
			//get random number in chance matrix
			int p = Random.Range(0, propChanceMatrix.Length-1);
				
			//return id from chance matrix
			return propChanceMatrix[p];
		}
		
		//update prop data so this prop is no longer in use
		public void ReturningProp(Prop propToReturn)
		{
			//look through all prop data
			for(int i = 0 ; i < propData.Count; i++)
			{
				//if the right one is found
				if(propData[i].prop == propToReturn)
				{
					//update inUse
					SetPropInUse(i, false, true, true);
					//propData[i].inUse = false;
					
					//mission success
					return;
				}
			}
			
			//Hah, we didnt find it?
			Debug.LogWarning("Prop being returned was not found. Reference was lost, it was never from this type or it was newly created. Soemthing weird has happened. New PropData will be made. " + this.gameObject.name);
			
			//lets add it than
			propData.Add(new PropData(propToReturn, Debugging, this));
		}
		
	}
	
}