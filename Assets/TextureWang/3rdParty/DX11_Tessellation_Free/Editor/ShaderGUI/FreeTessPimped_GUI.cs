using System;
using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif

#if UNITY_EDITOR
namespace UnityEditor
{
	internal class FreeTessPimped_GUI : ShaderGUI
	{
		private enum WorkflowMode
		{
			Distance,
			EdgeLength,
		}

		private static bool hasDispTexture = true;

		private static class Styles
		{
			public static GUIStyle optionsButton = "PaneOptions";
			public static string emptyTootip = "";
			public static GUIContent edgelenText = new GUIContent("Edge Length in PX", "Edge Length in PX. Smaller means more detail.");
			public static GUIContent tessFacText = new GUIContent("Tessellation Factor", "Tessellation Factor Up Close");
			public static GUIContent tessMaxText = new GUIContent("Tess / Disp Fade Distance", "Tessellation & Displacement Max Distance");
			public static GUIContent tessPhongText = new GUIContent("Tess Phong", "Tessellation Phong Smoothing");
			public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB)");
			public static GUIContent albedoText2 = new GUIContent("Albedo/ Disp", "Albedo (RGB) and Displacement (A)");
			public static GUIContent dispmapText = new GUIContent("Displacement", "Displacement (R)");
			public static GUIContent metallicText = new GUIContent("Metallic", "");
			public static GUIContent smoothnessText = new GUIContent("Smoothness", "");
			public static GUIContent specularAOText = new GUIContent("Specular AO", "Specular Occlusion");
			public static GUIContent diffuseAOText = new GUIContent("Diffuse AO", "Diffuse Occlusion (Burn)");
			public static GUIContent giAOText = new GUIContent("GI AO", "GI Occlusion");
			public static GUIContent specBurnText = new GUIContent("Spec Variation", "Smoothness Burn From Diffuse Red Channel");
			public static GUIContent detailAOText = new GUIContent("Detail Map AO", "Detail Map Occlusion");
			public static GUIContent normalMapText = new GUIContent("Normal Map / AO Tile", "Normal Map / Controls AO From Displacement Tiling and Offset");
			public static GUIContent detailAlbedoText = new GUIContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
			public static GUIContent detailNormalMapText = new GUIContent("Detail Normal Map", "Detail Normal Map");
			public static GUIContent dispScaleText = new GUIContent("Disp Scale", "Displacement Scale");
			public static GUIContent dispOffsetText = new GUIContent("Disp Offset", "Displacement Offset");
			public static GUIContent dispTileText = new GUIContent("Disp Tex Tile / Offset", "Displacement Texture Tile / Offset");

			public static string whiteSpaceString = " ";
			public static string primaryMapsText = "Main Maps";
			public static string secondaryMapsText = "Secondary Maps";
			public static string tessellationText = "Tessellation Settings";
			public static string displacementText = "Displacement Settings";
			public static string matParamsText = "Material Parameters";
		
		}

		MaterialProperty tessellation = null;
		MaterialProperty maxdist = null;
		MaterialProperty tessphong = null;
		MaterialProperty albedoMap = null;
		MaterialProperty dispMap = null;
		MaterialProperty albedoColor = null;
		MaterialProperty metallic = null;
		MaterialProperty smoothness = null;
		MaterialProperty bumpScale = null;
		MaterialProperty bumpMap = null;
		MaterialProperty displacement = null;
		MaterialProperty dispoffset = null;
		MaterialProperty displacementto = null;
		MaterialProperty specao = null;
		MaterialProperty diffuseao = null;
		MaterialProperty giao = null;
		MaterialProperty specburn = null;
		MaterialProperty detailao = null;
		MaterialProperty detailAlbedoMap = null;
		MaterialProperty detailNormalMapScale = null;
		MaterialProperty detailNormalMap = null;
		//MaterialProperty uvSetSecondary = null;

		MaterialEditor m_MaterialEditor;
		WorkflowMode m_WorkflowMode = WorkflowMode.EdgeLength;

		bool m_FirstTimeApply = true;

		public void FindProperties (MaterialProperty[] props)
		{
			tessellation = FindProperty ("_Tess", props);
			maxdist = FindProperty ("_maxDist", props, false);
			tessphong = FindProperty ("_DispPhong", props);
			if (maxdist != null)
				m_WorkflowMode = WorkflowMode.Distance;
			else
				m_WorkflowMode = WorkflowMode.EdgeLength;
			albedoMap = FindProperty ("_MainTex", props);
			dispMap = FindProperty ("_DispTex", props);
			albedoColor = FindProperty ("_Color", props);
			metallic = FindProperty ("_Metallic", props, false);
			smoothness = FindProperty ("_Glossiness", props);
			bumpScale = FindProperty ("_BumpScale", props);
			bumpMap = FindProperty ("_BumpMap", props);
			displacement = FindProperty ("_Displacement", props);
			dispoffset = FindProperty ("_DispOffset", props);
			displacementto = FindProperty ("_Displacement_TO", props);
			specao = FindProperty ("_SpecAO", props);
			diffuseao = FindProperty ("_DiffuseAO", props);
			giao = FindProperty ("_GI_AO", props);
			specburn = FindProperty ("_SpecBurn", props);
			detailao = FindProperty ("_DetailAO", props);
			detailAlbedoMap = FindProperty ("_DetailAlbedoMap", props);
			detailNormalMapScale = FindProperty ("_DetailNormalMapScale", props);
			detailNormalMap = FindProperty ("_DetailNormalMap", props);
			//uvSetSecondary = FindProperty ("_UVSec", props);
		}

		public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
		{
			FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
			m_MaterialEditor = materialEditor;
			Material material = materialEditor.target as Material;

			ShaderPropertiesGUI (material);

			// Make sure that needed keywords are set up if we're switching some existing
			// material to a new shader.
			if (m_FirstTimeApply)
			{
				SetKeywords(material);
				m_FirstTimeApply = false;
			}
		}

		public void ShaderPropertiesGUI (Material material)
		{
			// Use default labelWidth
			EditorGUIUtility.labelWidth = 0f;

			// Detect any changes to the material
			EditorGUI.BeginChangeCheck();
			{
				GUILayout.Label (Styles.tessellationText, EditorStyles.boldLabel);
				if (m_WorkflowMode == WorkflowMode.Distance) {
					m_MaterialEditor.ShaderProperty(tessellation, Styles.tessFacText.text);
					m_MaterialEditor.ShaderProperty(maxdist, Styles.tessMaxText.text);
				} else {
					m_MaterialEditor.ShaderProperty(tessellation, Styles.edgelenText.text);
				}
				m_MaterialEditor.ShaderProperty(tessphong, Styles.tessPhongText.text);
				EditorGUILayout.Space();

				GUILayout.Label (Styles.primaryMapsText, EditorStyles.boldLabel);
				if(!hasDispTexture) 
					m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText2, albedoMap, albedoColor);
				else
					m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
				m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
				m_MaterialEditor.TexturePropertySingleLine(Styles.dispmapText, dispMap);
				m_MaterialEditor.TextureScaleOffsetProperty(dispMap);	
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
				m_MaterialEditor.TextureScaleOffsetProperty(bumpMap);
				EditorGUILayout.Space();

				GUILayout.Label (Styles.displacementText, EditorStyles.boldLabel);
				m_MaterialEditor.ShaderProperty(displacement, Styles.dispScaleText.text);
				m_MaterialEditor.ShaderProperty(dispoffset, Styles.dispOffsetText.text);
				if(!hasDispTexture) 
					m_MaterialEditor.ShaderProperty(displacementto, Styles.dispTileText.text);
				else 
					GUILayout.Label (Styles.displacementText, EditorStyles.boldLabel);
				EditorGUILayout.Space();

				GUILayout.Label (Styles.matParamsText, EditorStyles.boldLabel);
				m_MaterialEditor.ShaderProperty(metallic, Styles.metallicText.text);
				m_MaterialEditor.ShaderProperty(smoothness, Styles.smoothnessText.text);
				m_MaterialEditor.ShaderProperty(specburn, Styles.specBurnText.text);
				m_MaterialEditor.ShaderProperty(specao, Styles.specularAOText.text);
				m_MaterialEditor.ShaderProperty(diffuseao, Styles.diffuseAOText.text);
				m_MaterialEditor.ShaderProperty(giao, Styles.giAOText.text);
				EditorGUILayout.Space();

				GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
				m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
				m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
				m_MaterialEditor.ShaderProperty(detailao, Styles.detailAOText.text);
				m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
				//m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);
				SetKeywords(material);
			}
		}

		internal void DetermineWorkflow(MaterialProperty[] props)
		{
			if (maxdist != null)
				m_WorkflowMode = WorkflowMode.Distance;
			else
				m_WorkflowMode = WorkflowMode.EdgeLength;
		}

		public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
		{
			base.AssignNewShaderToMaterial(material, oldShader, newShader);

			if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
				return;

			DetermineWorkflow( MaterialEditor.GetMaterialProperties(new Material[] { material }) );
			MaterialChanged(material, m_WorkflowMode);
		}
			
		static void SetKeywords(Material material)
		{
			SetKeyword (material, "_NORMALMAP", material.GetTexture ("_BumpMap") || material.GetTexture ("_DetailNormalMap"));
			SetKeyword (material, "_DETAIL_MULX2", material.GetTexture ("_DetailAlbedoMap") || material.GetTexture ("_DetailNormalMap"));
			hasDispTexture = material.GetTexture ("_DispTex");
			SetKeyword (material, "_DISPALPHA", !material.GetTexture ("_DispTex"));
		}

		static void MaterialChanged(Material material, WorkflowMode workflowMode)
		{
			SetKeywords(material);
		}

		static void SetKeyword(Material m, string keyword, bool state)
		{
			if (state)
				m.EnableKeyword (keyword);
			else
				m.DisableKeyword (keyword);
		}
	}

} // namespace UnityEditor
#endif