using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace AI.Common.Extensions.Sys
{
	public class ExcelReader
	{
		public DataSet ReadWholeFile(string file)
		{
			var sortedList = new List<string>();
			var ds = new DataSet();
			var sheets = GetWorksheets(file);
			foreach (var sheetName in sheets)
			{
				sortedList.Add(sheetName);
			}
			sortedList.Sort();
			foreach (var sheetName in sortedList)
			{
				var dt = GetTable(file, sheetName);
				ds.Tables.Add(dt);
			}
			return ds;
		}

		private void Execute(string file, Action<OleDbConnection> method)
		{
			var props = new Dictionary<string, string>();
			props["Provider"] = "Microsoft.ACE.OLEDB.12.0";
			props["Data Source"] = file;
			props["Extended Properties"] = "Excel 12.0";

			var sb = new StringBuilder();
			foreach (KeyValuePair<string, string> prop in props)
			{
				sb.Append(prop.Key);
				sb.Append('=');
				sb.Append(prop.Value);
				sb.Append(';');
			}
			string properties = sb.ToString();
			using (var conn = new OleDbConnection(properties))
			{
				conn.Open();
				method(conn);
			}
		}

		private IEnumerable<string> GetWorksheets(string file)
		{
			List<string> listSheet = null;
			Execute(file, conn =>
			{
				DataTable dtSheet = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
				listSheet = new List<string>();
				foreach (DataRow drSheet in dtSheet.Rows)
				{
					string tableName = drSheet["TABLE_NAME"].ToString();
					listSheet.Add(tableName);
				}
			});
			return listSheet;
		}

		private DataTable GetTable(string file, string worksheet)
		{
			DataTable dt = null;
			Execute(file, conn =>
			{
				using (var da = new OleDbDataAdapter(
					"SELECT " + "*" + " FROM [" + worksheet + "]", conn))
				{
					dt = new DataTable(worksheet);
					da.Fill(dt);
				}
			});
			return dt;
		}
	}
}