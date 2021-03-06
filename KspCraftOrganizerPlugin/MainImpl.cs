﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;
using KSP.UI.Screens;

namespace KspCraftOrganizer {

	/**
	 * This class does not inherit from MonoBehaviour to make it easy to use Kramax reloader in debug builds and do not use it in normal builds.
	 */
	public class MainImpl {
		private List<BaseWindow> windows = new List<BaseWindow>();

		private OrganizerWindow craftOrganizerWindow;
		private CurrentCraftSettingsWindow manageThisCraftWindow;

		private FileLocationService locationService = FileLocationService.instance;

		private List<ApplicationLauncherButton> appLauncherButtons = new List<ApplicationLauncherButton>();

		public void Start() {
			COLogger.logDebug("Craft organizer plugin - start");
			IKspAlProvider.instance.start();

			ShouldCurrentCraftBeSavedQuestionWindow shouldCraftBeSavedQuestionWindow = addWindow(new ShouldCurrentCraftBeSavedQuestionWindow());
			craftOrganizerWindow = addWindow(new OrganizerWindow(shouldCraftBeSavedQuestionWindow));
			manageThisCraftWindow = addWindow(new CurrentCraftSettingsWindow());

			addLauncherButtonInAllEditors(craftOrganizerWindow.displayWindow, "manage.png");
			addLauncherButtonInAllEditors(manageThisCraftWindow.displayWindow, "tags.png");
			//addLauncherButtonInAllEditors(CleanUp, null);

			foreach (BaseWindow window in windows) {
				window.start();
			}

			EditorListenerService.instance.start();

		}

		private void addLauncherButtonInAllEditors(Globals.Procedure callback, string textureFile) {
			ApplicationLauncherButton button = null;

			Texture2D texture;
			if(textureFile == null){
				texture = Texture2D.blackTexture;
			}else{
				texture = loadTextureFrom(Globals.combinePaths(locationService.getThisPluginDirectory(), "icons", textureFile));
			}

			button = ApplicationLauncher.Instance.AddModApplication(
				delegate () {
					button.SetFalse(false);
					callback();
				}, null, null, null, null, null,
				ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH, texture);
			appLauncherButtons.Add(button);
			
		}

		private T addWindow<T>(T newWindow) where T : BaseWindow  {
			windows.Add(newWindow);
			return newWindow;
		}

		Texture2D loadTextureFrom(string file) {

			Texture2D tex = null;
			byte[] fileData;

			if (File.Exists(file)) {
				fileData = File.ReadAllBytes(file);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData);
			} else {
				COLogger.logError("Cannot find " + file);
				tex = Texture2D.blackTexture;
			}

			return tex;
		}

		private void CleanUp() {
			COLogger.logDebug("Craft organizer plugin - CleanUp in " + EditorDriver.editorFacility);
			EditorListenerService.instance.processOnEditorExit();

			foreach (ApplicationLauncherButton button in appLauncherButtons) {
				ApplicationLauncher.Instance.RemoveModApplication(button);
			}
			EditorListenerService.instance.destroy();
			IKspAlProvider.instance.destroy();

		}

		public void Update() {
			foreach (BaseWindow window in windows) {
				window.update();
			}
		}

		public void OnGUI() {
			//if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) {
			//	COLogger.Log(Event.current);
			//}
			foreach (BaseWindow window in windows) {
				window.onGUI();
			}
		}

		public void OnDestroy() {
			COLogger.logDebug("Craft organizer plugin - OnDestroy in " + EditorDriver.editorFacility);
			CleanUp();
		}

		public void OnDisable() {
			COLogger.logDebug("Craft organizer plugin - OnDisable in " + EditorDriver.editorFacility);
		}
	}
}

