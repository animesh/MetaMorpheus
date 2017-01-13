﻿using System.Collections.Generic;
using System.Text;

namespace InternalLogicEngineLayer
{
    public class IndexResults : MyResults
    {
        #region Public Constructors

        public IndexResults(List<CompactPeptide> myDictionary, Dictionary<float, List<int>> myFragmentDictionary, IndexEngine indexParams) : base(indexParams)
        {
            this.peptideIndex = myDictionary;
            this.fragmentIndexDict = myFragmentDictionary;
        }

        #endregion Public Constructors

        #region Public Properties

        public Dictionary<float, List<int>> fragmentIndexDict { get; private set; }
        public List<CompactPeptide> peptideIndex { get; private set; }

        #endregion Public Properties

        #region Protected Methods

        protected override string GetStringForOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("fragmentIndexDict.Count: " + fragmentIndexDict.Count);
            sb.Append("peptideIndex.Count: " + peptideIndex.Count);
            return sb.ToString();
        }

        #endregion Protected Methods
    }
}