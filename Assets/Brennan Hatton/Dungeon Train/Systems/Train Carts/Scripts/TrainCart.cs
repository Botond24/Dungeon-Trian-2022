﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCart : MonoBehaviour
{
	//Tracks if the user is inside the cart
	public bool playerInside = false;
	
	TrainCartArchitecture architecture;
	
	
	public ArchitectureTheme theme;
	
	public float tilesLength = 1;
	public float length
	{
		get{
			return ArchitectureTheme.tileSize * tilesLength;
		}
	}

	public void SetTheme(ArchitectureTheme newTheme)
	{
		theme = newTheme;
		
		if(architecture == null)
			architecture = this.gameObject.AddComponent<TrainCartArchitecture>();
			
		architecture.SetTheme(newTheme);
	}
}
