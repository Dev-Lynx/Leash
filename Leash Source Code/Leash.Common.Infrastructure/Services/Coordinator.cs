using Leash.Common.Infrastructure.Models;
using Leash.Common.Infrastructure.Extensions;
using Leash.Common.Infrastructure.Services.Interfaces;
using Prism.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Windows;

namespace Leash.Common.Infrastructure.Services
{
    public class Coordinator : BindableBase, ICoordinator
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        #endregion

        #region Bindables
        public ObservableCollection<string> Roles { get; } = new ObservableCollection<string>(Enum.GetNames(typeof(Roles)));
        public ContestInfo Info { get; private set; } = new ContestInfo();
        public ContestantInfo CurrentContestantInfo { get; private set; }
        #endregion

        #region Internals
        Random Randomizer { get; } = new Random();
        #endregion

        #endregion

        #region Constructors
        public Coordinator(ILoggerFacade logger)
        {
            Logger = logger;
        }
        #endregion

        #region Methods

        #region ICoordinator Implementation
        public void AddContestant(Contestant contestant)
        {
            var hdAppearances = Info.Contestants.Count(c => c.Name.ToLower() == contestant.Name.ToLower());

            if (hdAppearances >= Info.MaxAppearances)
            {
                MessageBox.Show($"{contestant.Name} has been registered {Info.MaxAppearances} time(s) already");
                return;
            }
                

            int hlAppearances = 0;

            var partner = contestant.Partner;
            if (partner != null)
            {
                hlAppearances = Info.Contestants.Count(c => c.Name.ToLower() == partner.Name.ToLower());

                if (hlAppearances >= Info.MaxAppearances)
                {
                    MessageBox.Show($"{partner.Name} has been registered {Info.MaxAppearances} time(s) already");
                    return;
                }
                   
            }
            
            

            if (contestant == partner)
            {
                if ((hdAppearances + 2) > Info.MaxAppearances)
                {
                    MessageBox.Show($"Adding this team will make {contestant.Name} exeed the maximum amount of entries");
                    return;
                }
            }

            var team = new Team();
            Info.Contestants.Add(contestant);
            if (Info.OriginalContestants.Count(c => c.Name.ToLower() == contestant.Name.ToLower()) == 0)
                Info.OriginalContestants.Add(contestant);

            if (contestant.Role == Models.Roles.Header)
                team.Header = contestant;
            else
                team.Heeler = contestant;


            if (contestant.Partner != null)
            {
                contestant.Partner.Partner = contestant;

                if (contestant.Partner.Role == Models.Roles.Header)
                    team.Header = contestant.Partner;
                else
                    team.Heeler = contestant.Partner;

                Info.Contestants.Add(contestant.Partner);
                if (Info.OriginalContestants.Count(c => c.Name.ToLower() == contestant.Partner.Name.ToLower()) == 0)
                    Info.OriginalContestants.Add(contestant.Partner);
            }

            Info.AddTeam(team);
            RaisePropertyChanged("Info");


            Logger.Debug("Added {0} ({1}) - {2} ({3})", contestant.Name, contestant.Role, contestant.Partner?.Name, contestant.Partner?.Role);
        }

        public void NavigateToPreviousRound()
        {
            if (Info.ContestPosition <= 1) return;
            int current = --Info.ContestPosition;
            Info.Teams = new ObservableCollection<Team>(Info.TeamEntries[Info.ContestPosition - 1]);

            RaisePropertyChanged("Info");
        }

        public bool AdvanceToNextRound()
        {
            if (Info.ContestPosition >= (Info.Picks + Info.Draws) * Info.Entries) return false;
            int current = ++Info.ContestPosition;

            Info.ContestActive = true;

            if (!Info.ContestComplete)
                Info.ContestComplete = current >= (Info.Picks + Info.Draws) * Info.Entries;

            if (Info.ContestPosition - 1 < Info.TeamEntries.Count)
            {
                Info.Teams = new ObservableCollection<Team>(Info.TeamEntries[Info.ContestPosition - 1]);
                RaisePropertyChanged("Info");
                return true;
            }
           

            if (Info.AtEntryStart)
                MatchTeams();
            else RandomizeTeams();

            

            return current >= (Info.Picks + Info.Draws)*Info.Entries;
        }

        public bool GenerateCSVTable(string path, out string message)
        {
            message = "";

            var ext = Path.GetExtension(path);

            if (ext != ".csv")
                path += ".csv";
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    writer.WriteLine("\"{0}\"",Info.Date.ToLongDateString());
                    writer.WriteLine("Leash Contest Information".ToUpper());
                    writer.WriteLine("System: ,{0}".ToUpper(), Info.System.ToUpper());
                    writer.WriteLine("Total Rounds: ,{0}".ToUpper(), Info.TotalRounds);
                    writer.WriteLine("Total Contestants:, {0}".ToUpper(), Info.Contestants.Count);

                    // Compile Team Entries
                    for (int i = 0; i < Info.TeamEntries.Count; i++)
                    {
                        for (int x = 0; x < 3; x++)
                            writer.WriteLine();
                        writer.WriteLine("Round {0}".ToUpper(), i+1);

                        for (int j = 0; j < Info.TeamEntries[i].Count; j++)
                        {
                            var teamSet = Info.TeamEntries[i][j];
                            writer.WriteLine("{0},{1}", teamSet.Header, teamSet.Heeler);
                        }
                    }
                    //builder.AppendLine()
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageBox.Show("Failed to export info");
                message = "An error occured while generating the csv file";
                return false;
            }
            return true;
        }

        public bool GenerateTable(string path, out string message)
        {
            string name = Path.GetFileNameWithoutExtension(path);

            try
            {
                using (var set = new DataSet(name))
                using (var table = new DataTable())
                {
                    for (int i = 1; i <= 3; i++)
                        table.Columns.Add("");


                    table.Rows.Add(Info.Date.ToLongDateString());
                    table.Rows.Add("Leash Contest Information".ToUpper());
                    table.Rows.Add("System: ".ToUpper(), Info.System);
                    table.Rows.Add("Total Rounds: ".ToUpper(), Info.TotalRounds);
                    table.Rows.Add("Total Contestants: ".ToUpper(), Info.Contestants.Count);

                    // Compile Team Entries
                    for (int i = 0; i < Info.TeamEntries.Count; i++)
                    {
                        for (int x = 0; x < 3; x++)
                            table.Rows.Add("", "");
                        table.Rows.Add(string.Format("Round {0}".ToUpper(), i + 1), "");

                        for (int j = 0; j < Info.TeamEntries[i].Count; j++)
                        {
                            var teamSet = Info.TeamEntries[i][j];
                            table.Rows.Add(teamSet.Header, teamSet.Heeler);
                        }
                    }

                    set.Tables.Add(table);
                    Logger.Debug(set.GetXml());

                    if (Path.GetExtension(path) != ".xlsx")
                        path += ".xlsx";

                    CreateExcelFile.CreateExcelDocument(set, path);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageBox.Show("Failed to export info");
            }
            
                
            
            message = "The table has been successfully generated";
            return true;
        }

        public bool ExportContestantInfo(ContestantInfo info, ExportTypes type, string path, out string message)
        {
            message = "";
            string ext = string.Empty;
            switch (type)
            {
                case ExportTypes.Excel:
                    try
                    {
                        using (var set = new DataSet())
                        using (var table = new DataTable())
                        {
                            for (int i = 1; i <= 3; i++)
                                table.Columns.Add("");


                            table.Rows.Add(Info.Date.ToLongDateString());
                            table.Rows.Add("Leash Contestant Information".ToUpper());
                            table.Rows.Add("System: ".ToUpper(), info.ContestInfo.System);
                            table.Rows.Add("Total Rounds: ".ToUpper(), info.ContestInfo.TotalRounds);
                            table.Rows.Add("Contestant Name: ".ToUpper(), info.Contestant);

                            string role1 = info.IsHeader ? $"{info.Contestant} (Header) roping with: ".ToUpper() : string.Empty;
                            string role2 = info.IsHeeler ? $"{info.Contestant} (Heeler) roping with: ".ToUpper() : string.Empty;

                            for (int x = 0; x < 3; x++)
                                table.Rows.Add("", "");

                            table.Rows.Add("", role1, role2);
                            table.Rows.Add("", "");

                            for (int i = 0; i < info.Entries.Count; i++)
                            {
                                var entry = info.Entries[i];
                                table.Rows.Add(entry.Title);
                                table.Rows.Add("", "");

                                for (int j = 0; j < entry.Headers.Count; j++)
                                {
                                    var header = entry.Headers[j];
                                    var heeler = entry.Heelers[j];
                                    table.Rows.Add($"ROUND {header.Round}", $"{heeler.Contestant} {heeler.Origin}", $"{header.Contestant} {header.Origin}");
                                }

                                for (int x = 0; x < 3; x++)
                                    table.Rows.Add("", "");
                            }


                            set.Tables.Add(table);
                            Logger.Debug(set.GetXml());
                            if (Path.GetExtension(path) != ".xlsx")
                                path += ".xlsx";

                            CreateExcelFile.CreateExcelDocument(set, path);
                            message = "Successful";
                        }
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        MessageBox.Show("Failed to export info.");
                        break;
                    }
                case ExportTypes.CSV:
                    ext = Path.GetExtension(path);

                    if (ext != ".csv")
                        path += ".csv";
                    try
                    {
                        using (var writer = new StreamWriter(path))
                        {
                            writer.WriteLine($"\"{Info.Date.ToLongDateString()}\"");
                            writer.WriteLine("Leash Contestant Information".ToUpper());
                            writer.WriteLine("System: ,{0}".ToUpper(), info.ContestInfo.System.ToUpper());
                            writer.WriteLine("Total Rounds: ,{0}".ToUpper(), info.ContestInfo.TotalRounds);
                            writer.WriteLine("Contestant Name: , {0}".ToUpper(), info.Contestant);

                            string role1 = info.IsHeader ? $"{info.Contestant} (Header) roping with: ".ToUpper() : string.Empty;
                            string role2 = info.IsHeeler ? $"{info.Contestant} (Heeler) roping with: ".ToUpper() : string.Empty;

                            for (int x = 0; x < 3; x++)
                                writer.WriteLine();

                            writer.WriteLine($"{string.Empty}, {role1}, {role2}");
                            writer.WriteLine();

                            for (int i = 0; i < info.Entries.Count; i++)
                            {
                                var entry = info.Entries[i];
                                writer.WriteLine(entry.Title);
                                writer.WriteLine();
                                for (int j = 0; j < entry.Headers.Count; j++)
                                {
                                    var header = entry.Headers[j];
                                    var heeler = entry.Heelers[j];
                                    writer.WriteLine($"ROUND {header.Round}, {heeler.Contestant} {heeler.Origin}, {header.Contestant} {header.Origin}");
                                }

                                for (int x = 0; x < 3; x++)
                                    writer.WriteLine();
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        MessageBox.Show("Failed to export info.");
                        message = "An error occured while generating the csv file";
                        return false;
                    }
                    break;

                case ExportTypes.Text:
                    
                    break;
            }
            return true;
        }

        public void Reset()
        {
            Info = new ContestInfo();
            RaisePropertyChanged("Info");
        }

        public ContestantInfo CompileInfo(Contestant c)
        {
            if (c == null) return null;

            ContestantInfo info = new ContestantInfo();
            info.Contestant = c;

            bool isHeader = Info.Headers.Where(con => c.Name.ToLower() == con.Name.ToLower()).FirstOrDefault() != null;

            if (isHeader) c.Role = Models.Roles.Header;

            switch (c.Role)
            {
                case Models.Roles.Header:
                    info.IsHeader = true;
                    var heeler = Info.Heelers.Where(con => c.Name.ToLower() == con.Name.ToLower()).FirstOrDefault();
                    info.IsHeeler = heeler != null;

                    for (int i = 0; i < Info.TeamEntries.Count; i++)
                    {
                        var entry = Info.TeamEntries[i];

                        bool addedHeader = false;
                        bool addedHeeler = false;
                        bool hdDone = false;
                        bool hlDone = false;

                        for (int j = 0; j < Info.TeamEntries[i].Count; j++)
                        {
                            var team = Info.TeamEntries[i][j];

                            if (!addedHeeler)
                                addedHeeler = team.Header == c;
                            if (!addedHeader)
                                addedHeader = info.IsHeeler && team.Heeler == c;

                            if (addedHeeler && !hlDone)
                            { info.HeelerEntries.Add(team.Heeler); hlDone = true; }
                            if (addedHeader && !hdDone) { info.HeaderEntries.Add(team.Header); hdDone = true; }
                        }

                        hlDone = hdDone = false;

                        if (!addedHeeler) info.HeelerEntries.Add(null);
                        if (!addedHeader) info.HeaderEntries.Add(null);
                    }
                    break;

                case Models.Roles.Heeler:
                    info.IsHeeler = true;
                    for (int i = 0; i < Info.TeamEntries.Count; i++)
                    {
                        var entry = Info.TeamEntries[i];
                        bool addedHeader = false;
                        bool hdDone = false;

                        foreach (var team in entry)
                        {
                            if (!addedHeader)
                                addedHeader = team.Heeler == c;

                            if (addedHeader && !hdDone) { info.HeaderEntries.Add(team.Header); info.HeelerEntries.Add(null); hdDone = true; break; }
                        }

                        hdDone = false;
                        if (!addedHeader)
                        {
                            info.HeaderEntries.Add(null);
                            info.HeelerEntries.Add(null);
                        }
                    }
                    break;
            }
            info.ContestInfo = Info;

            CurrentContestantInfo = info;
            RaisePropertyChanged("CurrentContestantInfo");
            return info;
        }
        #endregion

        void MatchTeams()
        {
            List<Team> teams = new List<Team>();
            List<Contestant> headers = Info.Headers.ToList();
            List<Contestant> heelers = Info.Heelers.ToList();

            while (headers.Count > 0 && heelers.Count > 0)
            {
                RESTART:
                var hd = Randomizer.Next(headers.Count);
                var hl = Randomizer.Next(heelers.Count);

                int count = heelers.Count;
                int index = 0;

                if (headers[hd].Partner != null)
                    hl = heelers.IndexOf(headers[hd].Partner);
                else
                {
                    headers.RemoveAt(hd);
                    continue;
                }
                /*
                else while (heelers[hl].Partner != null)
                {
                    hl = Randomizer.Next(0, heelers.Count);

                    if (index >= count) goto RESTART;

                    index++;
                }
                    */

                var team = new Team(headers[hd], heelers[hl]);
                teams.Add(team);

                headers.RemoveAt(hd);
                heelers.RemoveAt(hl);
            }

            /*
            while (headers.Count > 0)
            {
                var hd = Randomizer.Next(0, headers.Count);
                teams.Add(new Team(headers[hd], null));
                headers.RemoveAt(hd);
            }

            while (heelers.Count > 0)
            {
                var hl = Randomizer.Next(0, heelers.Count);
                teams.Add(new Team(null, heelers[hl]));
                heelers.RemoveAt(hl);
            }
            */

            Info.Teams = new ObservableCollection<Team>(teams);
            Info.TeamEntries.Add(teams);
            RaisePropertyChanged("Info");

            /*
            List<Team> teams = new List<Team>();
            var contestants = new List<Contestant>(Info.Contestants);

            // Pick each team according to their prefered partners
            for (int i = 0; i < contestants.Count; i++)
            {
                if (contestants[i].Role == Models.Roles.Header)
                    if (contestants[i].Partner != null)
                    {
                        teams.Add(new Team(contestants[i], contestants[i].Partner));

                        var contestant = contestants[i];
                        
                        int count = contestants.RemoveAll(c => c.Name.ToLower() == contestants[i].Partner?.Name.ToLower());
                        contestants.Remove(contestant);

                        i = -1;//i > 2 ? i - 2 : i = 0; 
                    }
            }


            // Team mates without partners will be matched to the next available contestant
            for (int i = 0; i < contestants.Count; i++)
            {
                if (contestants[i].Role == Models.Roles.Header)
                    for (int j = 0; j < contestants.Count; j++)
                        if (contestants[j].Role == Models.Roles.Heeler)
                        {
                            var header = contestants[i];
                            var heeler = contestants[j];
                            teams.Add(new Team(header, heeler));
                            contestants.Remove(header);
                            contestants.Remove(heeler);
                              
                            i = -1;//i > 2 ? i - 2 : i = 0;

                            break;
                        }
            }

            Info.Teams = new ObservableCollection<Team>(teams);
            Info.TeamEntries.Add(teams);
            RaisePropertyChanged("Info");
            */
        }

        void RandomizeTeams()
        {
            List<Team> teams = new List<Team>();
            List<Contestant> headers = Info.Headers.ToList();
            List<Contestant> heelers = Info.Heelers.ToList();


            while (headers.Count > 0 && heelers.Count > 0)
            {
                var hd = Randomizer.Next(headers.Count);
                var hl = Randomizer.Next(heelers.Count);

                var team = new Team(headers[hd], heelers[hl]);
                teams.Add(team);

                headers.RemoveAt(hd);
                heelers.RemoveAt(hl);
            }

            while (headers.Count > 0)
            {
                var hd = Randomizer.Next(headers.Count);
                teams.Add(new Team(headers[hd], null));
                headers.RemoveAt(hd);
            }

            while (heelers.Count > 0)
            {
                var hl = Randomizer.Next(heelers.Count);
                teams.Add(new Team(null, heelers[hl]));
                heelers.RemoveAt(hl);
            }

            Info.Teams = new ObservableCollection<Team>(teams);
            Info.TeamEntries.Add(teams);
            RaisePropertyChanged("Info");

            /*
            List<Team> teams = new List<Team>();
            List<int> selectedHeelers = new List<int>();
            var contestants = new List<Contestant>(Info.Contestants);

            // Count of total headers or heelers
            int heelerCount = contestants.Count / 2;
            Contestant[] heelers = new Contestant[heelerCount];

            // Compile the total heelers
            for (int x = 0, y = 0; x < contestants.Count; x++)
                if (contestants[x].Role == Models.Roles.Heeler)
                {
                    heelers[y++] = contestants[x];
                    contestants.RemoveAt(x);
                    x = 0;
                }
                    

                
            for (int i = 0; i < contestants.Count; i++)
            {
                // Pick a random heeler
                int index = Randomizer.Next(0, heelerCount);

                // Make sure the heeler has not been selected
                while (selectedHeelers.Contains(index))
                    index = Randomizer.Next(0, heelerCount);

                // Add the new team
                teams.Add(new Team(contestants[i], heelers[index]));
                selectedHeelers.Add(index);
            }

            // Send the team to the model for display.
            Info.Teams = new ObservableCollection<Team>(teams);
            Info.TeamEntries.Add(teams);
            RaisePropertyChanged("Info");
            */
        }

        
        string ExportDataSet(DataSet set, string destination)
        {
            try
            {
                using (var package = SpreadsheetDocument.Create(destination, SpreadsheetDocumentType.Workbook))
                {
                    var wbPart = package.AddWorkbookPart();
                    wbPart.Workbook = new Workbook();

                    Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());

                    var data = new SheetData();
                    WorksheetPart worksheetpart = wbPart.AddNewPart<WorksheetPart>();
                    worksheetpart.Worksheet = new Worksheet(data);
                    worksheetpart.Worksheet.Save();


                    

                    Sheet sheet = new Sheet()
                    {
                        Id = wbPart.GetIdOfPart(worksheetpart),
                        SheetId = (uint)(sheets.Count() + 1),
                    };
                    sheets.AppendChild(sheet);



                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return "An error occured while trying to export the excel sheet";
            }
            return "";
        }
        
        #endregion
    }
}
