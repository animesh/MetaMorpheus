﻿using EngineLayer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TaskLayer
{
    public class EverythingRunnerEngine : MyEngine
    {

        #region Private Fields

        private readonly List<Tuple<string, MetaMorpheusTask>> taskList;
        private List<string> currentRawDataFilenameList;
        private List<DbForTask> currentXmlDbFilenameList;

        #endregion Private Fields

        #region Public Constructors

        public EverythingRunnerEngine(List<Tuple<string, MetaMorpheusTask>> taskList, List<string> startingRawFilenameList, List<DbForTask> startingXmlDbFilenameList)
        {
            this.taskList = taskList;
            currentRawDataFilenameList = startingRawFilenameList;
            currentXmlDbFilenameList = startingXmlDbFilenameList;
        }

        #endregion Public Constructors

        #region Public Events

        public static event EventHandler startingAllTasksEngineHandler;

        public static event EventHandler finishedAllTasksEngineHandler;

        public static event EventHandler<XmlForTaskListEventArgs> newDbsHandler;

        public static event EventHandler<StringListEventArgs> newSpectrasHandler;

        #endregion Public Events

        #region Protected Methods

        protected override MyResults RunSpecific()
        {
            StartingAllTasks();

            if (!currentRawDataFilenameList.Any())
            {
                FinishedAllTasks();
                return new MyErroredResults(this, "No data files selected");
            }

            var startTimeForAllFilenames = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);

            var MatchingChars =
                from len in Enumerable.Range(0, currentRawDataFilenameList.Min(s => s.Length)).Reverse()
                let possibleMatch = currentRawDataFilenameList.First().Substring(0, len)
                where currentRawDataFilenameList.All(f => f.StartsWith(possibleMatch, StringComparison.Ordinal))
                select possibleMatch;

            var longestDir = Path.GetDirectoryName(MatchingChars.First());

            for (int i = 0; i < taskList.Count; i++)
            {
                if (!currentRawDataFilenameList.Any())
                {
                    FinishedAllTasks();
                    return new MyErroredResults(this, "Cannot proceed. No data files selected.");
                }
                if (!currentXmlDbFilenameList.Any())
                {
                    FinishedAllTasks();
                    return new MyErroredResults(this, "Cannot proceed. No xml files selected.");
                }
                var ok = taskList[i];
                string outputFolderForThisTask = Path.Combine(longestDir, startTimeForAllFilenames);
                outputFolderForThisTask = Path.Combine(outputFolderForThisTask, ok.Item1);

                if (!Directory.Exists(outputFolderForThisTask))
                    Directory.CreateDirectory(outputFolderForThisTask);

                var myTaskResults = ok.Item2.RunTask(outputFolderForThisTask, currentXmlDbFilenameList, currentRawDataFilenameList, ok.Item1);
                if (myTaskResults.newDatabases != null)
                {
                    currentXmlDbFilenameList = myTaskResults.newDatabases;
                    NewDBs(myTaskResults.newDatabases);
                }
                if (myTaskResults.newSpectra != null)
                {
                    currentRawDataFilenameList = myTaskResults.newSpectra;
                    NewSpectras(myTaskResults.newSpectra);
                }
            }
            FinishedAllTasks();
            return new EverythingRunnerResults(this);
        }

        #endregion Protected Methods

        #region Private Methods

        private void StartingAllTasks()
        {
            startingAllTasksEngineHandler?.Invoke(this, EventArgs.Empty);
        }

        private void FinishedAllTasks()
        {
            finishedAllTasksEngineHandler?.Invoke(this, EventArgs.Empty);
        }

        private void NewSpectras(List<string> newSpectra)
        {
            newSpectrasHandler?.Invoke(this, new StringListEventArgs(newSpectra));
        }

        private void NewDBs(List<DbForTask> newDatabases)
        {
            newDbsHandler?.Invoke(this, new XmlForTaskListEventArgs(newDatabases));
        }

        #endregion Private Methods

    }
}