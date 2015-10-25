//-----------------------------------------------------------------------------------------
// Program1.cs                                      (C) Copyright 2015 by Cloud Invent ML
//
// Created:    29.09.2015                           Dmitry Ratner
//-----------------------------------------------------------------------------------------

namespace CloudInvent.Cheetah.Examples.FourLinesWithConstraints
{
    using System;
    using System.Linq;

    // Cloud Invent namespaces
    using Data;
    using Data.Geometry;
    using Data.DataSetBuilder;
    using Data.ValueReference;
    using Parametric;
    using Solver.Cpu10;

    class Program1
    {
        static void Main(string[] args)
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

            // 4. Creating solver object
            var solver = new SolverCpu10();

            // 5. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false, true, true);

            const double precision = 1E-14; // Working with better accuracy then default 1E-12

            CheetahParametricBasic.Settings.Precision = precision;

            // 6. Initializing parametric object using data set
            if (!parametric.Init(dataSet, null, null))
                throw new Exception("Something goes wrong");

            // 7. Regenerating constrained model (running solver)
            if (!parametric.Evaluate())
                throw new Exception("Something goes wrong");

            // 8. Retrieving results (we created rectangle that is "closest" to the initial lines)
            var resultGeometry = parametric.GetSolution(true);

            // 9. We can find corresponding objects by id
            var resultLine1 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line1.Id);
            var resultLine2 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line2.Id);
            var resultLine3 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line3.Id);
            var resultLine4 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line4.Id);

            // 10. Now we can check that all constraints are satisfied (line segments form rectangle)

            if (Math.Abs(resultLine1.End.X - resultLine2.Start.X) > precision || Math.Abs(resultLine1.End.Y - resultLine2.Start.Y) > precision)
                throw new Exception("Something goes wrong");
            if (Math.Abs(resultLine2.End.X - resultLine3.Start.X) > precision || Math.Abs(resultLine2.End.Y - resultLine3.Start.Y) > precision)
                throw new Exception("Something goes wrong");
            if (Math.Abs(resultLine3.End.X - resultLine4.Start.X) > precision || Math.Abs(resultLine3.End.Y - resultLine4.Start.Y) > precision)
                throw new Exception("Something goes wrong");
            if (Math.Abs(resultLine4.End.X - resultLine1.Start.X) > precision || Math.Abs(resultLine4.End.Y - resultLine1.Start.Y) > precision)
                throw new Exception("Something goes wrong");

            if (!IsDivisible(resultLine1.PolarAngle - resultLine2.PolarAngle, Math.PI / 2, precision))
                throw new Exception("Something goes wrong");
            if (!IsDivisible(resultLine2.PolarAngle - resultLine3.PolarAngle, Math.PI / 2, precision))
                throw new Exception("Something goes wrong");

            if (!IsDivisible(resultLine2.PolarAngle - resultLine4.PolarAngle, Math.PI, precision))
                throw new Exception("Something goes wrong");

            GC.Collect();

        } // Main(...)

        private static bool IsDivisible(double dividend, double divider, double precision)
        {
            var quotient = Math.Round(dividend / divider);

            return Math.Abs(quotient * divider - dividend) < precision;
            
        } // IsDivisible(...)

    } // class Program1

} // namespace CloudInvent.Cheetah.Examples.FourLinesWithConstraints
