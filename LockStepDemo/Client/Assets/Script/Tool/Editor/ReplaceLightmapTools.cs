﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

class ReplaceLightmapTools
{
	//RenderTexture to png
	static bool SaveRenderTextureToPNG(Texture inputTex,Shader outputShader, string contents, string pngName)
	{
		RenderTexture temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
		Material mat = new Material(outputShader);
		Graphics.Blit(inputTex, temp, mat);
		bool ret = SaveRenderTextureToPNG(temp, contents,pngName);
		RenderTexture.ReleaseTemporary(temp);
		
		return ret;
	}
	
	static bool SaveRenderTextureToPNG(RenderTexture rt,string contents, string pngName)
	{
		RenderTexture prev = RenderTexture.active;
		RenderTexture.active = rt;
		
		Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
		png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		byte[] bytes = png.EncodeToPNG();
		
		if (!Directory.Exists(contents))
		{
			Directory.CreateDirectory(contents);
		}
		FileStream file = File.Open(contents + "/" + pngName + ".png", FileMode.Create);
		
		BinaryWriter writer = new BinaryWriter(file);
		writer.Write(bytes);
		file.Close();
		
		Texture2D.DestroyImmediate(png);
		png = null;
		
		RenderTexture.active = prev;
		
		return true;
	}

	[MenuItem("Lightmap/转换当前场景的Lightmap(LogLuv编码)")]
	static void CreateCurrentSceneLightmap()
	{
		//替换光照贴图
		Shader replaceShader = Shader.Find("ReplaceLightmap/OutputLightmap");
		
		string abSceneName = EditorApplication.currentScene;
		string contents = Application.dataPath + "/Data/Lightmap" + abSceneName.Remove(0, EditorApplication.currentScene.LastIndexOf('/'));
		LightmapData[] lightmaps = LightmapSettings.lightmaps;
		foreach (LightmapData lmd in lightmaps)
		{
			Texture f = lmd.lightmapLight;
			Texture n = lmd.lightmapDir;
			if (n != null)
			{
				Debug.LogError("有Near类型的Lightmap!");
				return;
			}
			else
			{
				if (SaveRenderTextureToPNG(f, replaceShader, contents, f.name))
				{
					Debug.Log("exr光照图convert to png : " + contents + "/" + f.name + ".png");
				}
			}
			
		}
	}
	
	[MenuItem("Lightmap/替换当前场景的Lightmap")]
	static void ReplaceCurrentSceneLightmap()
	{
		//替换光照贴图
		LightmapData[] lightmaps = LightmapSettings.lightmaps;
		if (lightmaps.Length == 0)
			return;
		string abSceneName = EditorApplication.currentScene;
		
		string lightmapName = lightmaps[0].lightmapLight.name;
		lightmapName = lightmapName.Remove(lightmapName.LastIndexOf('-'));
		
		Renderer[] renderer = GameObject.FindObjectsOfType(typeof(Renderer)) as Renderer[];
		foreach (Renderer rd in renderer)
		{
			Material oldMat = rd.material;
			if (oldMat.shader.name.Equals("Diffuse"))
			{
				Shader shader = Shader.Find("ReplaceLightmap/ReplaceDiffuse");
				
				string lightmapPath = "Assets/Data/Lightmap" + abSceneName.Remove(0, EditorApplication.currentScene.LastIndexOf('/')) + "/" + lightmapName + "-" + rd.lightmapIndex.ToString() + ".png";
				Texture lightmapTex = (Texture)AssetDatabase.LoadAssetAtPath(lightmapPath, typeof(Texture));
				
				rd.material.shader = shader;
				rd.material.SetTexture("_LightmapTex", lightmapTex);

			}
		}
	}
	
}
