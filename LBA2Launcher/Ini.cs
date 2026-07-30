using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class Ini {
	Dictionary<string, Dictionary<string, string>> ini =
		new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);
	string file;

	/// <summary>
	/// Initialize an INI file.
	/// Load it if it exists.
	/// </summary>
	/// <param name="file">Full path where the INI file has to be read from or written to</param>
	public Ini(string file) {
		this.file = file;

		if(!File.Exists(file))
			return;

		Load();
	}

	/// <summary>
	/// Load the INI file content
	/// </summary>
	public void Load() {
		var lines = File.ReadAllLines(file);

		Dictionary<string, string> currentSection =
			new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		ini[""] = currentSection;

		for(int i = 0; i < lines.Length; i++) {
			var line = lines[i].Trim();

			// Drop blank / whitespace-only lines entirely
			if(string.IsNullOrWhiteSpace(line))
				continue;

			// Preserve actual comments
			if(line.StartsWith(";")) {
				currentSection[";" + i.ToString()] = line;
				continue;
			}

			if(line.StartsWith("[") && line.EndsWith("]")) {
				currentSection =
					new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
				ini[line.Substring(1, line.Length - 2)] = currentSection;
				continue;
			}

			var idx = line.IndexOf("=");
			if(idx == -1)
				currentSection[line] = "";
			else
				currentSection[line.Substring(0, idx).Trim()] = line.Substring(idx + 1).Trim();
		}
	}

	/// <summary>
	/// Get a parameter value at the root level
	/// </summary>
	public string GetValue(string key) {
		return GetValue(key, "", "");
	}

	/// <summary>
	/// Get a parameter value in the section
	/// </summary>
	public string GetValue(string key, string section) {
		return GetValue(key, section, "");
	}

	/// <summary>
	/// Returns a parameter value in the section, with a default value if not found
	/// </summary>
	public string GetValue(string key, string section, string @default) {
		if(!ini.ContainsKey(section))
			return @default;

		if(!ini[section].ContainsKey(key))
			return @default;

		return ini[section][key];
	}

	/// <summary>
	/// Save the INI file
	/// </summary>
	public void Save() {
		var sb = new StringBuilder();

		foreach(var section in ini) {
			if(sb.Length > 0)
				sb.AppendLine();

			if(section.Key != "") {
				sb.AppendFormat("[{0}]", section.Key);
				sb.AppendLine();
			}

			foreach(var keyValue in section.Value) {
				if(keyValue.Key.StartsWith(";")) {
					sb.Append(keyValue.Value);
					sb.AppendLine();
				}
				else {
					sb.AppendFormat("{0}={1}", keyValue.Key, keyValue.Value);
					sb.AppendLine();
				}
			}
		}

		File.WriteAllText(file, sb.ToString());
	}

	/// <summary>
	/// Write a parameter value at the root level
	/// </summary>
	public void WriteValue(string key, string value) {
		WriteValue(key, "", value);
	}

	/// <summary>
	/// Write a parameter value in a section
	/// </summary>
	public void WriteValue(string key, string section, string value) {
		Dictionary<string, string> currentSection;
		if(!ini.ContainsKey(section)) {
			// FIX #2: use the same case-insensitive comparer as everywhere else
			currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			ini.Add(section, currentSection);
		}
		else
			currentSection = ini[section];

		currentSection[key] = value;
	}

	/// <summary>
	/// Get all the keys names in a section
	/// </summary>
	public string[] GetKeys(string section) {
		if(!ini.ContainsKey(section))
			return new string[0];

		return ini[section].Keys.ToArray();
	}

	/// <summary>
	/// Get all the section names of the INI file
	/// </summary>
	public string[] GetSections() {
		return ini.Keys.Where(t => t != "").ToArray();
	}
}