﻿using EngineLayer;
using Proteomics;
using System;
using System.Collections.Generic;

namespace Test
{
    internal class TestParentSpectrumMatch : PsmParent
    {

        #region Public Constructors

        public TestParentSpectrumMatch(int scanNumber, int scanPrecursorCharge) : base(null, double.NaN, double.NaN, double.NaN, scanNumber, 0, scanPrecursorCharge, 0, double.NaN, double.NaN, double.NaN, 1)
        {
        }

        #endregion Public Constructors

        #region Public Methods

        public override CompactPeptide GetCompactPeptide(List<ModificationWithMass> variableModifications, List<ModificationWithMass> localizeableModifications, List<ModificationWithMass> fixedModifications)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods

    }
}