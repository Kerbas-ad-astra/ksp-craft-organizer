﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace KspCraftOrganizer
{

	public class CraftType{

		public static readonly CraftType SPH = new CraftType("SPH");
		public static readonly CraftType VAB = new CraftType("VAB");

		private string _directoryName;

		public CraftType(String directoryName){
			this._directoryName = directoryName;
		}
		public string directoryName { get { return _directoryName; } }	

		public string id { get { return _directoryName; } }	

		public override string ToString(){
			return id;
		}
	};




	public class OrganizerCraftModel{
		
		private bool _selectedPrimary;
		private bool _selected;
		private string _craftFile;
		private SortedList<string, string> _tags = new SortedList<string, string>();
		private readonly OrganizerService service;
		private Texture2D thumbTextureCache;
		private bool duringCreation;
		private CraftDaoDto craftDtoLazy;

		public CraftDaoDto craftDto {
			get {
				if (craftDtoLazy == null) {
					craftDtoLazy = service.getCraftInfo(_craftFile);
					if (nameFromSettingsFile != craftDtoLazy.name) {
						COLogger.logDebug(
							"The name from settings is different than name from craft file. Settings has: '" 
							+ nameFromSettingsFile 
							+ "', craft file has: '" + craftDtoLazy.name + "'");
						craftSettingsFileIsDirty = true;
					}
				}
				return craftDtoLazy;
			}}

		public OrganizerCraftModel(OrganizerService service, string craftFile){
			this.service = service;
			this._craftFile = craftFile;
			this.duringCreation = true;
		}

		public void finishCreationMode() {
			this.duringCreation = false;
		}

		public string name { get {
				if (craftDtoLazy == null) {
					if (string.IsNullOrEmpty(nameFromSettingsFile)) {
						return craftDto.name;
					} else {
						return nameFromSettingsFile;
					}
				} else {
					return craftDto.name;
				}

			}}

		public string nameToDisplay { get {
				string toRet = "";
				if (isAutosaved)
				{
					toRet += "[AUTOSAVED] ";
				}
				toRet += name;
				if (isStock) {
					toRet += " (Stock) ";
				}
				if (name != nameFromFile && !isAutosaved)
				{
					toRet += " [" + nameFromFile + "]";
				}
				return toRet; 
			}
		}


		public float cost { get { return craftDto.cost; } }

		public string costToDisplay { get { 
				float cost = this.cost;
				if (cost > 10000000) {
					return roundDiv (cost, 1000000) + "M";
				}
				if (cost > 1000) {
					return roundDiv (cost, 1000) + "k";
				}
				return roundDiv(cost, 1);
			}}

		public int partCount { get { return craftDto.partCount; } }

		public float mass { get { return craftDto.mass; } }

		public int stagesCount { get { return craftDto.stagesCount; } }

		public bool isAutosaved { get; set; }

		public string massToDisplay {
			get {
				float mass = this.mass;
				if (mass > 1000000000) {
					return roundDiv (mass, 1000000000) + "Gt";
				}
				if (mass > 1000000) {
					return roundDiv (mass, 1000000) + "Mt";
				}
				if (mass > 1000) {
					return roundDiv (mass, 1000) + "kt";
				}
				return roundDiv(mass, 1) + "t";
			}
		}

		public bool containsMissedParts { get { return craftDto.containsMissedParts; } }

		public bool notEnoughScience { get { return craftDto.notEnoughScience; } }

		private string roundDiv(float toDiv, int  divisor){
			return (((double)toDiv) / divisor).ToString("#0.000");
		}

		public void setSelectedPrimaryInternal(bool selectedPrimary){
			if (_selectedPrimary != selectedPrimary) {
				inRenameState = false;
			}
			_selectedPrimary = selectedPrimary;
		}

		public void setSelectedInternal(bool selected){
			_selected = selected;
		}


		public bool inRenameState { set ; get; }

		public bool isSelectedPrimary{ get {return _selectedPrimary; }}

		public bool isSelected { 
			get {  
				return _selected;
			} 
			set{
				if (_selected != value) {
					_selected = value;
					if (!value) {
						service.onOneCraftUnselected ();
					}
				}
			}
		}

		public ICollection<string> tags{
			get{
				return new ReadOnlyCollection<string>(_tags.Keys);
			}
		}

		public string tagsString{
			get{
				return Globals.join(_tags.Values, tag => tag, ", ");
			}
		}

		public void addTag(string tag){
			if (!_tags.ContainsKey (tag)) {
				_tags.Add (tag, tag);
				craftSettingsFileIsDirty = true;
				if (!duringCreation) {
					service.markFilterAsChanged();
				}
			}
		}

		public void removeTag(string tag){
			if (_tags.ContainsKey (tag)) {
				_tags.Remove (tag);
				craftSettingsFileIsDirty = true;
				service.markFilterAsChanged ();
			}
		}

		public bool craftSettingsFileIsDirty { get ; set ;}

		public string nameFromSettingsFile { get; set;  }


		public bool containsTag(string tag){
			return _tags.ContainsKey (tag);
		}

		public float guiWidth { get; set; }
		public float guiHeight { get; set ; }

		public float tagGroupNameWidth { get; set; }

		public float tagsHeight { get; set; }

		public string craftFile { 
			get{ 
				return _craftFile;
			} 
		}

		public void setCraftFileInternal(string craftFile){
			this._craftFile = craftFile;
		}

		public bool isStock { get; set; }

		public string nameFromFile
		{
			get
			{
				return Path.GetFileNameWithoutExtension(craftFile);
			}
		}

		public Texture2D thumbTexture {
			get {
				if (thumbTextureCache == null) {
					thumbTextureCache = service.getThumbnailForFile(craftFile);
				}
				return thumbTextureCache;
			}
			set {
				thumbTextureCache = value;
			}
		}

		public bool inDeleteState { get; set; }
		public string description {
			get {
				return craftDto.description;
			}
		}
	}
}

