//-----------------------------------------------------------------------------------------
// Program2.cs                                      (C) Copyright 2015 by Cloud Invent ML
//
// Created:    29.09.2015                           Dmitry Ratner
//-----------------------------------------------------------------------------------------

namespace CloudInvent.Cheetah.Examples.SaveLoadModel
{
    using System;
    using System.Collections.Generic;

    // Cloud Invent namespaces
    using Data;
    using Data.Geometry;
    using Data.DataSetBuilder;
    using Data.ValueReference;
    using Parametric;
    using Solver.Cpu10;

    class Program2
    {
        static void Main(string[] args)
        {
            const string filePath = "dataset.xml";

            // 1. Creating entities and constraints (constraints are not satisfied yet)
            var dataSet = CreateDataSet();

            // 2. Saving data set to xml-file
            dataSet.SaveToXml(filePath);

            // 3. Loading data set from xml-file
            var dataSetFromFile = CheetahDataSet.LoadFromXml(filePath);

            // 4. Creating solver object
            var solver = new SolverCpu10();

            // 5. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false, true, true);

            const double precision = 1E-8; // Working with lower accuracy then default 1E-12 

            CheetahParametricBasic.Settings.Precision = precision;

            // 6. Initializing parametric object using data set
            if (!parametric.Init(dataSetFromFile, null, null))
                throw new Exception("Something goes wrong");

            // 7. Regenerating constrained model (running solver)
            if (!parametric.Evaluate())
                throw new Exception("Something goes wrong");

            // 8. Retrieving results (we created rectangle that is "closest" to the initial lines)
            var resultGeometry = parametric.GetSolution(true);

            GC.Collect();

        } // Main(...)

        private static CheetahDataSet CreateDataSet()
        {
            // 1. Creating data set
            var dataSet = new CheetahDataSet();

            // 2. Creating geometry
            var line1 = new CheetahLine2D(0, 0, 10, 1);
            var line2 = new CheetahLine2D(10, 0, 10, 11);
            var line3 = new CheetahLine2D(10, 10, 1, 10);
            var line4 = new CheetahLine2D(0, 10, 1, 1);

            // 3. Creating constraints
            dataSet.AddCoincidence(line1, IdentifiableValueReferences.LineEnd,
                line2, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line2, IdentifiableValueReferences.LineEnd,
                line3, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line3, IdentifiableValueReferences.LineEnd,
                line4, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line4, IdentifiableValueReferences.LineEnd,
                line1, IdentifiableValueReferences.LineStart);

            dataSet.AddPerpendicular(line1, line2);
            dataSet.AddPerpendicular(line2, line3);

            dataSet.AddParallel(line2, line4);

            // 4. Now we have geometric model (4 line segments and 7 constraints)
            //    with everything necessary for creating rectangle.
            //    Geometric constraints are not satisfied yet.

            return dataSet;

        } // CreateDataSet()

    } // class Program2

}  // namespace CloudInvent.Cheetah.Examples.SaveLoadModel
