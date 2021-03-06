﻿#region Using References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using CPT331.Core.Extensions;
using CPT331.Core.Logging;
using CPT331.Core.ObjectModel;

#endregion

namespace CPT331.Data.Parsers
{
	/// <summary>
	/// Represents an NtXmlParser type, used to manipulate XML data from a file.
	/// </summary>
	public class NtXmlParser : XmlParser
	{
		/// <summary>
		/// Constructs a new NtXmlParser object.
		/// </summary>
		/// <param name="dataSourceDirectory">The path to the directory containing the XML data sources.</param>
		public NtXmlParser(string dataSourceDirectory)
			: base(dataSourceDirectory, NT)
		{
		}

		internal const string NT = "NT";

		/// <summary>
		/// Performs parsing operations and constructs a list of Coordinate objects as the result.
		/// </summary>
		/// <param name="fileName">The path to the file containing the XML information to parse.</param>
		/// <param name="crimes">The list of Crime objects to serialise the XML information into.</param>
		protected override void OnParse(string fileName, List<Crime> crimes)
		{
			OutputStreams.WriteLine($"Parsing {NT} data...");

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);

			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("/Workbook/Worksheet/Table");

			State state = DataProvider.StateRepository.GetStateByAbbreviatedName(NT);
			List<LocalGovernmentArea> localGovernmentAreas = DataProvider.LocalGovernmentAreaRepository.GetLocalGovernmentAreasByStateID(state.ID);
			Dictionary<string, Offence> offences = new Dictionary<string, Offence>();
			DataProvider.OffenceRepository.GetOffences().ForEach(m => offences.Add(m.Name.ToUpper(), m));

			foreach (XmlNode xmlNode in xmlNodeList)
			{
				XmlNode localGovermentAreaXmlNode = xmlNode.SelectSingleNode("Row[position() = 1]");
				string localGovernmentAreaName = localGovermentAreaXmlNode.InnerText.Trim();
				LocalGovernmentArea localGovernmentArea = localGovernmentAreas.Where(m => (m.Name.EqualsIgnoreCase(localGovernmentAreaName) == true)).FirstOrDefault();

				List<DateTime> dateTimeList = new List<DateTime>();
				XmlNodeList datesXmlNodeList = xmlNode.SelectNodes("Row[position() = 2]/Cell[position() > 1]");
				datesXmlNodeList.OfType<XmlNode>().ToList().ForEach(m => dateTimeList.Add(DateTime.Parse(m.InnerText)));

				XmlNodeList offenceXmlNodeList = xmlNode.SelectNodes("Row[position() > 2]");
				foreach (XmlNode offenceXmlNode in offenceXmlNodeList)
				{
					Offence offence = null;
					string offenceName = offenceXmlNode.ChildNodes[0].InnerText.Trim().ToUpper();

					if ((String.IsNullOrEmpty(offenceName) == false) && (offences.ContainsKey(offenceName) == true))
					{
						offence = offences[offenceName];
					}

					for (int i = 0, j = 1; j < offenceXmlNode.ChildNodes.Count; i++, j++)
					{
						int count = Convert.ToInt32(offenceXmlNode.ChildNodes[j].InnerText);
						DateTime dateTime = dateTimeList[i];

						crimes.Add(new Crime(count, localGovernmentArea.ID, dateTime.Month, offence.ID, dateTime.Year));
					}
				}
			}

			base.OnParse(fileName, crimes);
		}
	}
}
