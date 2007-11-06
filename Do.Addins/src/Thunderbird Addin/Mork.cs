//
// Mork.cs: A parser for mork files (used by software such as Firefox and Thunderbird)
//
// Copyright (C) 2006 Pierre Östlund
//

//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace Beagle.Util
{
	public class MorkDatabase : IEnumerable {
		protected string mork_file;
		protected string enum_namespace;
		protected string mork_version;
		
		protected Hashtable dicts;
		protected Hashtable metadicts;
		protected Hashtable rows;
		protected Hashtable tables;
		
		protected string regex_row = @"(?<action>[-!+]?)\[(-|)(?<roid>[0-9A-Za-z:\^]+)(?<cells>(?>[^\[\]]+)?)\]";
		protected string regex_cell = @"\^(?<key>[0-9A-Fa-f]+)(\^(?<pvalue>[0-9A-Fa-f]+)|=(?<value>[0-9A-Fa-f]+))";
		protected string regex_table = @"{.*?:(?<ns>[0-9A-Fa-f\^]+) {\(k\^(?<tbl>[0-9A-Fa-f]+):c\)";

		public MorkDatabase (string mork_file)
		{
			this.mork_file = mork_file;
			this.dicts = new Hashtable ();
			this.metadicts = new Hashtable ();
			this.rows = new Hashtable ();
			this.tables = new Hashtable ();
		}
		
		public void Read ()
		{
			string content;
			StreamReader reader = new StreamReader (mork_file);;
			
			// Check if this is a mork file and save database version if it is. We assume the first line will tell us this.
			if (!IsValid (reader.ReadLine (), out mork_version)) {
				reader.Close ();
				throw new InvalidMorkDatabaseException ("This file is missing a valid mork header");
			}
			
			content  = reader.ReadToEnd ();
			reader.Close ();
			
			Reset ();
			Read (content);
		}
		
		protected bool IsValid (string header, out string version)
		{
			version = null;
			Regex reg = new Regex (@"<!-- <mdb:mork:z v=\""(?<version>(.*))\""/> -->");
			
			if (header == null || header == string.Empty)
				return false;
			
			Match m = reg.Match (header);
			if (!m.Success)
				return false;
			
			version = m.Result ("${version}");
			return true;
		}

		protected void Read (string content)
		{
			int position = -1;

			while (++position != content.Length) {
				
				if (content [position].Equals ('/') && content [position].Equals ('/'))
					// Ignore comments
					position = content.IndexOf ('\n', position);
				else if (content [position].Equals ('<') && content [position+2].Equals ('<'))
					// Parse metadict information
					ParseMetaDict (FindStartIndex (content, ref position, "<(", ")>"), position, content);
				else if (content [position].Equals ('<'))
					// Parse dict information
					ParseDict (FindStartIndex (content, ref position, "<(", ")>"),position, content);
				else if (content [position].Equals ('{')) {
					// Parse table information
					ParseTable (Read (content, ref position, "{", "}"));
				 }else if (content [position].Equals ('[')) 
					// Parse rows
					ParseRows (Read (content, ref position, "[", "]"), null, null);
				else if (content [position].Equals ('@') && content [position+1].Equals ('$'))
					// Parse groups
					ParseGroups (Read (content, ref position, "@$${", "@$$}"));
			}
		}
		
		protected string Read (string content, ref int position, string start, string end)
		{
			int tmp = position, start_position = position;
			
			do {
				position = content.IndexOf (end, position+1);
				if ((tmp = content.IndexOf (start, tmp+1)) < 0)
					break;
			} while (tmp < position);
			
			return content.Substring (start_position, position-start_position+1);
		}
		// This method is complex, and quite hacky, but it basically returns the index of the beginning
		// of the substring, and points position to the end of the substring. Which I use in ParseDict
		// and ParseMetaDict to significantly reduce the number of string allocations we are making.
		protected int FindStartIndex (string content, ref int position, string start, string end)
		{
			int tmp = position, start_position = position;
			
			do {
				position = content.IndexOf (end, position+1);
				if ((tmp = content.IndexOf (start, tmp+1)) < 0)
					break;
			} while (tmp < position);
			
			return start_position;
		}
		
		protected virtual void ParseDict (int start, int end, string dict)
		{
			Regex reg = new Regex (@"(?<id>[0-9A-Fa-f]+)\s*=(?<value>(.*))", RegexOptions.Compiled);
			
			// This is sooo lame that, but it's an easy solution that works. It seems like regex fails
			// here when dealing with big amounts of data.
			foreach (string t in Regex.Replace (dict.Substring (start+2,(end-start)-3).Replace ("\\\n", "").
				Replace ("\n", ""), @"\)\s*\(", "\n").Split ('\n')) {
				
				Match m = reg.Match (t);
				if (m.Success)
					dicts [m.Result ("${id}")] = m.Result ("${value}");
			}
		}
		
		protected virtual void ParseMetaDict (int start, int end, string content)
		{
			Regex reg = new Regex (@"(?<id>[0-9A-Fa-f]+)=(?<value>[^()]+)", RegexOptions.Compiled);
			
			foreach (Match m in reg.Matches (content.Substring(start,end-start+1)))
				metadicts [m.Result ("${id}")] = m.Result ("${value}");
		}
		
		protected virtual void ParseTable (string table)
		{
			int start = table.IndexOf ('}')+1;
			Match m = new Regex (regex_table, RegexOptions.Compiled).Match (table);
			
			ParseRows (table.Substring (start, table.Length-start-1), m.Result ("${ns}"), m.Result ("${tbl}"));
		}
		
		protected virtual void ParseRows (string rows, string ns, string table)
		{
			Regex reg = new Regex (regex_row, RegexOptions.Compiled);
			
			foreach (Match m in reg.Matches (Clean (rows))) {
				// tmp [0] == id, tmp [1] == ns
				string[] tmp = m.Result ("${roid}").Split (':');
				
				if (m.Result ("${action}") == "-" || m.Result ("${cells}") == string.Empty)
					RemoveRow (tmp [0], (tmp.Length > 1 ? tmp [1] : ns));
				else
					AddRow (tmp [0], (tmp.Length > 1 ? tmp [1] : ns), table, m.Result ("${cells}"));
			}
		}
		
		protected virtual void ParseGroups (string groups)
		{
			int start = groups.IndexOf ("{@")+2;
			groups =groups.Substring (start, groups.Length-start-1);
			Read (groups);
		}
		
		protected string Clean (string str)
		{
			return str.Replace ("\n", "").Replace (" ", "");
		}
		
		public string ParseNamespace (string ns)
		{
			if (ns == null || ns == string.Empty)
				return string.Empty;
			if (ns.StartsWith ("^"))
				return ns;
			else {
				foreach (string key in metadicts.Keys)
					if ((metadicts [key] as string) == ns)
						return String.Format ("^{0}", key);
			}
			
			return ns;
		}
		
		public void AddRow (string id, string ns, string table, string cells)
		{
			string ns2 = ParseNamespace (ns);
			
			if (id == string.Empty || ns2 == string.Empty || table == string.Empty || cells == string.Empty)
				return;
			else if (!rows.ContainsKey (ns2))
				rows [ns2] = new Hashtable ();

			(rows [ns2] as Hashtable) [id] = (Exists (id, ns2) ? String.Concat (cells, GetCells (id, ns2)) : cells);
			
			if (!tables.ContainsKey (id))
				tables [id] = table;
		}
		
		public void RemoveRow (string id, string ns)
		{
			string ns2 = ParseNamespace (ns);
			
			if (!rows.ContainsKey (ns2))
				return;
			
			(rows [ns2] as Hashtable).Remove (id);
			tables.Remove (id);
		}
		
		public string GetCells (string id, string ns)
		{
			string ns2 = ParseNamespace (ns);
			
			return (ns2 != null ?(rows [ns2] as Hashtable) [id] as string : null);
		}
		
		public Hashtable Compile (string id, string ns)
		{
			string ns2 = ParseNamespace (ns);
			
			if (!Exists (id, ns2))
				return null;
			
			Hashtable tbl = new Hashtable ();
			Regex reg = new Regex (regex_cell, RegexOptions.Compiled);
			
			foreach (Match m in reg.Matches (GetCells (id, ns2))) {
				string value = (string) (m.Result ("${pvalue}") != string.Empty ? 
							dicts [m.Result("${pvalue}")] : m.Result ("${value}"));
				tbl [metadicts [m.Result ("${key}")]] = Decode (value, Encoding);
			}
			
			tbl ["id"] = id;
			tbl ["table"] = tables [id];
			
			return tbl;
		}

		public bool Exists (string id, string ns)
		{
			string ns2 = ParseNamespace (ns);
			
			return (ns2 != null ? (rows [ns] as Hashtable).ContainsKey (id) : false);
		}
		
		public int GetRowCount (string ns)
		{
			string ns2 = ParseNamespace (ns);
			
			if (ns2 == null || rows [ns2] == null)
				return -1;
			
			return (rows [ns2] as Hashtable).Count;
		}
		
		public int GetRowCount (string ns, string table)
		{
			int count = 0;
			string ns2 = ParseNamespace (ns);
			
			if (ns2 == null || rows [ns2] == null)
				return -1;
			
			foreach (string id in (rows [ns2] as Hashtable).Keys) {
				if ((string) tables [id] == table)
					count++;
			}
			
			return count;
		}
		
		public IEnumerator GetEnumerator ()
		{
			string ns = ParseNamespace (EnumNamespace);
			
			if (ns == null || (rows [ns] as Hashtable) == null || Empty)
				return null;
			
			return (rows [ns] as Hashtable).Keys.GetEnumerator ();
		}
		
		public void Reset ()
		{
			dicts.Clear ();
			metadicts.Clear ();
			rows.Clear ();
			tables.Clear ();
			mork_version = string.Empty;
		}

		public static string Convert (int char1, int char2, System.Text.Encoding to_encoding)
		{
			byte[] bytes;
			System.Text.Encoding from;
			
			if (char2 == -1) {
				from = System.Text.Encoding.UTF7;
				bytes = new byte[] { System.Convert.ToByte (char1) };
			} else {
				from = System.Text.Encoding.UTF8;
				bytes = new byte[] { System.Convert.ToByte (char1), System.Convert.ToByte (char2) };
			}
			
			return to_encoding.GetString (System.Text.Encoding.Convert (from, to_encoding, bytes));
		}
		
		public static string Decode (string str, System.Text.Encoding to_encoding)
		{
			if (str == null || str == string.Empty || to_encoding == null || str.IndexOf ('$') == -1)
				return str;
			
			foreach (Match m in Regex.Matches (str, @"\$(?<1>[0-9A-F]{2})\$(?<2>[0-9A-F]{2})|\$(?<3>[0-9A-F]{2})")) {
				string char1 = m.Result ("${1}"), char2 = m.Result ("${2}"), char3 = m.Result ("${3}");
				
				if (char1 != string.Empty) {
					str = str.Replace (String.Format (@"${0}${1}", char1, char2), 
					    /*
						Convert (Thunderbird.Hex2Dec (char1),
					             Thunderbird.Hex2Dec (char2),
					             to_encoding);
					    */
		             Convert (int.Parse (char1, System.Globalization.NumberStyles.HexNumber),
					          int.Parse (char2, System.Globalization.NumberStyles.HexNumber),
					          to_encoding));
					
					 
				} else {
					str = str.Replace (String.Format (@"${0}", char3), 
						Convert (int.Parse (char3, System.Globalization.NumberStyles.HexNumber), -1, to_encoding));
				}
			}
			return str;
		}

		public int Rows {
			get {
				int count = 0;
				
				foreach (Hashtable r in rows.Values)
					count += r.Count;
			
				return count;
			}
		}
		
		public string EnumNamespace {
			get { return enum_namespace; }
			set { enum_namespace = value; }
		}
		
		public string Filename {
			get { return mork_file; }
		}
		
		public string Version {
			get { return mork_version; }
		}

		// There will always exist an item with id 1 in namespace 80, which means
		// that when there are less than two items in the database, it's empty
		public bool Empty {
			get { return (rows.Count > 1 ? false : true); }
		}
		
		public System.Text.Encoding Encoding {
			get { 
				System.Text.Encoding encoding;
				
				try {
					encoding = System.Text.Encoding.GetEncoding ((string) metadicts ["f"]); 
				} catch { 
					encoding = System.Text.Encoding.GetEncoding ("iso-8859-1");
				}
			
				return encoding;
			}
		}
	}
	
	public class InvalidMorkDatabaseException : System.Exception {
	
		public InvalidMorkDatabaseException (string message) : base (message)
		{
		}
	}
}
