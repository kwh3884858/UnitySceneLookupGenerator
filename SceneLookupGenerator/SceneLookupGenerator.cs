﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SceneLookupGenerator
{

	public enum ErrorType
	{
		NoError,
		NoSceneRootPath,
		NoOutputPath,
		NoTemplateFile,

	}

	public enum MessageType
	{

	}

	public class SceneLookupGenerator
	{
		static readonly string m_outputName = "/SceneLookup.cs";
		static readonly string m_macroConstant = "#CONSTANT#";
		static readonly string m_macroTotal = "#TOTAL#";
		static readonly string m_macroSceneName = "#SCENENAME#";
		static readonly string m_macroSceneEnum = "#SCENEENUM#";

		static string m_content;

		string m_sceneRootPath;
		string m_outputPath;
		string m_templatePath;

		List<string> scenes;

		public SceneLookupGenerator ()
		{
			m_sceneRootPath = "";
			m_outputPath = "";
			m_templatePath = "/Users/cookie/Documents/CSharpProjects/SceneLookupGenerator/SceneLookupGenerator/SceneLookupTemplate.txt";
		}

		public ErrorType Execute ()
		{

			//Find template
			if (!Directory.Exists (m_sceneRootPath)) {
				return ErrorType.NoSceneRootPath;
			}


			DirectoryInfo folder = new DirectoryInfo (m_sceneRootPath);
			FileInfo [] infos = folder.GetFiles ("*.unity");

			scenes = new List<string> ();
			foreach (FileInfo info in infos) {
				scenes.Add (info.Name.Remove (info.Name.LastIndexOf (".", StringComparison.Ordinal)));
			}

			//Chceck Template errpr
			ErrorType error = CheckFilePath (m_templatePath, ErrorType.NoTemplateFile);
			if (error != ErrorType.NoError) {
				return ErrorType.NoTemplateFile;
			}

			//Read template
			using (FileStream fs = File.OpenRead (m_templatePath)) {
				byte [] b = new byte [1024];
				UTF8Encoding temp = new UTF8Encoding (true);
				while (fs.Read (b, 0, b.Length) > 0) {

					m_content = temp.GetString (b);

				}
			}


			//Replace macro constant int
			StringBuilder content = new StringBuilder ();

			for (int i = 0; i < scenes.Count; i++) {
				content.Append ("public const int ");
				content.Append (scenes [i]);
				content.Append (" = ");
				content.Append (i);
				content.Append (";\n");

			}
			m_content = m_content.Replace (m_macroConstant, content.ToString ());
			//Replace total count
			m_content = m_content.Replace (m_macroTotal, scenes.Count.ToString ());


			content.Clear ();

			for (int i = 0; i < scenes.Count; i++) {
				content.Append ("\"");
				content.Append (scenes [i]);
				content.Append ("\",\n");


			}
			//Remove last comma
			content.Remove (content.Length - 2, 2);
			m_content = m_content.Replace (m_macroSceneName, content.ToString ());

			//Repalce macro enum scene name
			content.Clear ();
			for (int i = 0; i < scenes.Count; i++) {
				content.Append (scenes [i]);
				content.Append (" = ");
				content.Append (i);
				content.Append (",\n");
			}

			content.Remove (content.Length - 2, 2);

			m_content = m_content.Replace (m_macroSceneEnum, content.ToString ());

			if (!Directory.Exists (m_outputPath)) {
				return ErrorType.NoOutputPath;
			}

			m_content = m_content.Substring (0, m_content.IndexOf ('\0'));

			m_outputPath += m_outputName;

			//Write scene lookup
			if (File.Exists (m_outputPath)) {
				File.Delete (m_outputPath);
			}
			// Write the byte array to the other FileStream.
			using (FileStream fsNew = new FileStream (m_outputPath,
					 FileMode.Create, FileAccess.Write)) {
				Byte [] bs = Encoding.UTF8.GetBytes (m_content);
				fsNew.Write (bs, 0, bs.Length);
			}


			return ErrorType.NoError;

		}

		public string GetSceneRootPath ()
		{
			return m_sceneRootPath;
		}

		public ErrorType SetSceneRootPath (string projectRoot)
		{
			m_sceneRootPath = projectRoot;
			return CheckDirectoryPath (m_sceneRootPath, ErrorType.NoSceneRootPath);
		}

		public ErrorType SetOutputPath (string output)
		{
			m_outputPath = output;

			return CheckDirectoryPath (m_outputPath, ErrorType.NoOutputPath);

		}

		public ErrorType SetTemplatePath (string templatePath)
		{
			m_templatePath = templatePath;

			return CheckFilePath (m_templatePath, ErrorType.NoTemplateFile);
		}

		private ErrorType CheckDirectoryPath (string path, ErrorType pathError)
		{
			if (!Directory.Exists (path)) {
				return pathError;
			}

			return ErrorType.NoError;
		}

		private ErrorType CheckFilePath (string path, ErrorType pathError)
		{
			if (!File.Exists (path)) {
				return pathError;
			}

			return ErrorType.NoError;
		}
	}
}
