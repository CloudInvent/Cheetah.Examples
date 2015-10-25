//-----------------------------------------------------------------------------------------
// Program3.cs                                      (C) Copyright 2015 by Cloud Invent ML
//
// Created:    30.09.2015                           Nick Sidorenko
//-----------------------------------------------------------------------------------------

namespace CloudInvent.Cheetah.Examples.RoundedRectangle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Cloud Invent namespaces
    using Data;
    using Data.Geometry;
    using Data.DataSetBuilder;
    using Data.ValueReference;
    using Interfaces;
    using Parametric;
    using Solver.Cpu10;

    class Program3
    {
        static void Main(string[] args)
        {
            // 1. Creating geometric model

            var dataSet = new CheetahDataSet();

            var entIdList = new List<long>();

            CreateGeometricModel(dataSet, entIdList);

            // 2. Creating solver object
            var solver = new SolverCpu10();

            // 3. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false);

            const double precision = 1E-15; // Working with much better accuracy then the default 1E-12 

            CheetahParametricBasic.Settings.Precision = precision;

            // 4. Initializing parametric object using data set
            if (!parametric.Init(dataSet, null, null))
                throw new Exception("Something goes wrong");

            // 5. Running constraints solver
            if (!parametric.Evaluate())
                throw new Exception("Something goes wrong");

            // 6. Retrieving results (we created rectangle with fillets that is "closest" to the initial lines and arcs)
            var resultGeometry = parametric.GetSolution(true);

            // 7. Checking that all geometric constraints are satisfied.
            //    Actually, we have no need to check - if Evaluate returns true that everything is OK
            if (!CheckResults(resultGeometry, entIdList, precision))
                throw new Exception("Something goes wrong");

            GC.Collect();
        }

        private static void CreateGeometricModel(CheetahDataSet dataSet, ICollection<long> entIdList)
        {
            // 1. Creating line segments and appending them to the data set

            var line1 = new CheetahLine2D(0.5, -0.5, 8.5, 0.5);
            var line2 = new CheetahLine2D(10.5, 1.5, 9.5, 8.5);
            var line3 = new CheetahLine2D(9.5, 9.5, 0.5, 10.5);
            var line4 = new CheetahLine2D(0.5, 8.5, -0.5, 1.5);

            entIdList.Add(line1.Id);
            entIdList.Add(line2.Id);
            entIdList.Add(line3.Id);
            entIdList.Add(line4.Id);

            // 2. Creating circle arcs and appending them to the data set

            var arc1 = new CheetahArc2D(new CheetahPoint2D(1.5, 1.5), -Math.PI, -Math.PI * 0.5, 1.25);
            var arc2 = new CheetahArc2D(new CheetahPoint2D(9.25, 0.75), -Math.PI * 0.5, 0.0, 0.5);
            var arc3 = new CheetahArc2D(new CheetahPoint2D(9.5, 9.5), Math.PI * 0.1, Math.PI * 0.5, 1.5);
            var arc4 = new CheetahArc2D(new CheetahPoint2D(1.2, 8.5), Math.PI * 0.25, Math.PI, 0.75);

            entIdList.Add(arc1.Id);
            entIdList.Add(arc2.Id);
            entIdList.Add(arc3.Id);
            entIdList.Add(arc4.Id);

            // 3. Creating geometric constraints

            // 3.1. Creating coinsident constraints for the end points of arcs amd line segments

            dataSet.AddCoincidence(arc1, IdentifiableValueReferences.ArcEnd,
                line1, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line1, IdentifiableValueReferences.LineEnd,
                arc2, IdentifiableValueReferences.ArcStart);

            dataSet.AddCoincidence(arc2, IdentifiableValueReferences.ArcEnd,
                line2, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line2, IdentifiableValueReferences.LineEnd,
                arc3, IdentifiableValueReferences.ArcStart);

            dataSet.AddCoincidence(arc3, IdentifiableValueReferences.ArcEnd,
                line3, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line3, IdentifiableValueReferences.LineEnd,
                arc4, IdentifiableValueReferences.ArcStart);

            dataSet.AddCoincidence(arc4, IdentifiableValueReferences.ArcEnd,
                line4, IdentifiableValueReferences.LineStart);

            dataSet.AddCoincidence(line4, IdentifiableValueReferences.LineEnd,
                arc1, IdentifiableValueReferences.ArcStart);

            // 3.2. Creating perpendicular constraints between line segments 1 and 2

            dataSet.AddPerpendicular(line1, line2);

            // 3.3. Creating two parallel constraints between line segments 1 and 3 and line segments 2 and 4 

            dataSet.AddParallel(line1, line3);
            dataSet.AddParallel(line2, line4);

            // 3.4. Creating "equal radius" constraints for all arcs

            dataSet.AddEqual(arc1, arc2);
            dataSet.AddEqual(arc2, arc3);
            dataSet.AddEqual(arc3, arc4);

            // 3.5 Creating tangent constraints on the connection points of arcs and line segments

            dataSet.AddTangent(arc1, line1);
            dataSet.AddTangent(arc2, line1);
            dataSet.AddTangent(arc2, line2);
            dataSet.AddTangent(arc3, line2);
            dataSet.AddTangent(arc3, line3);
            dataSet.AddTangent(arc4, line3);
            dataSet.AddTangent(arc4, line4);
            dataSet.AddTangent(arc1, line4);

            // 4. Geometric model is created, but constraints are not satisfied yet!
        }

        private static bool CheckResults(ICollection<CheetahCurve> resultGeometry, IReadOnlyList<long> entId, double precision)
        {
            // 1. We can find corresponding objects by id
            var resultLine1 = (CheetahLine2D)resultGeometry.Single(x => x.Id == entId[0]);
            var resultLine2 = (CheetahLine2D)resultGeometry.Single(x => x.Id == entId[1]);
            var resultLine3 = (CheetahLine2D)resultGeometry.Single(x => x.Id == entId[2]);
            var resultLine4 = (CheetahLine2D)resultGeometry.Single(x => x.Id == entId[3]);
            var resultArc1 = (CheetahArc2D)resultGeometry.Single(x => x.Id == entId[4]);
            var resultArc2 = (CheetahArc2D)resultGeometry.Single(x => x.Id == entId[5]);
            var resultArc3 = (CheetahArc2D)resultGeometry.Single(x => x.Id == entId[6]);
            var resultArc4 = (CheetahArc2D)resultGeometry.Single(x => x.Id == entId[7]);

            // 2. Now we can check that all constraints are satisfied (we check that line segments and arcs form rectangle with fillets)

            // Check coinsident constraint betweem end-point of arc 1 and start-point of line segment 1
            if (Math.Abs(resultArc1.End.X - resultLine1.Start.X) > precision ||
                Math.Abs(resultArc1.End.Y - resultLine1.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of line segment 1 and start-point of arc 2
            if (Math.Abs(resultLine1.End.X - resultArc2.Start.X) > precision ||
                Math.Abs(resultLine1.End.Y - resultArc2.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of arc 2 and start-point of line segment 2
            if (Math.Abs(resultArc2.End.X - resultLine2.Start.X) > precision ||
                Math.Abs(resultArc2.End.Y - resultLine2.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of line segment 2 and start-point of arc 3
            if (Math.Abs(resultLine2.End.X - resultArc3.Start.X) > precision ||
                Math.Abs(resultLine2.End.Y - resultArc3.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of arc 3 and start-point of line segment 3
            if (Math.Abs(resultArc3.End.X - resultLine3.Start.X) > precision ||
                Math.Abs(resultArc3.End.Y - resultLine3.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of line segment 3 and start-point of arc 4
            if (Math.Abs(resultLine3.End.X - resultArc4.Start.X) > precision ||
                Math.Abs(resultLine3.End.Y - resultArc4.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of arc 4 and start-point of line segment 4
            if (Math.Abs(resultArc4.End.X - resultLine4.Start.X) > precision ||
                Math.Abs(resultArc4.End.Y - resultLine4.Start.Y) > precision)
                return false;

            // Check coinsident constraint betweem end-point of line segment 4 and start-point of arc 1
            if (Math.Abs(resultLine4.End.X - resultArc1.Start.X) > precision ||
                Math.Abs(resultLine4.End.Y - resultArc1.Start.Y) > precision)
                return false;

            // Check perpendicular constraint between line segments 1 and 2
            if (!IsDivisible(resultLine1.PolarAngle - resultLine2.PolarAngle, Math.PI / 2, precision))
                return false;

            // Check parallel constraint between line segments 1 and 3
            if (!IsDivisible(resultLine1.PolarAngle - resultLine3.PolarAngle, Math.PI, precision))
                return false;

            // Check parallel constraint between line segments 2 and 4
            if (!IsDivisible(resultLine2.PolarAngle - resultLine4.PolarAngle, Math.PI, precision))
                return false;

            // Check equal constraint betweem arc 1 and arc 2
            if (Math.Abs(resultArc1.Radius - resultArc2.Radius) > precision)
                return false;

            // Check equal constraint betweem arc 2 and arc 3
            if (Math.Abs(resultArc2.Radius - resultArc3.Radius) > precision)
                return false;

            // Check equal constraint betweem arc 3 and arc 4
            if (Math.Abs(resultArc3.Radius - resultArc4.Radius) > precision)
                return false;

            // Check tangent constraint between arc 1 and line segment 1
            if (!IsDivisible(resultLine1.PolarAngle - resultArc1.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 2 and line segment 1
            if (!IsDivisible(resultLine1.PolarAngle - resultArc2.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 2 and line segment 2
            if (!IsDivisible(resultLine2.PolarAngle - resultArc2.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 3 and line segment 2
            if (!IsDivisible(resultLine2.PolarAngle - resultArc3.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 3 and line segment 3
            if (!IsDivisible(resultLine3.PolarAngle - resultArc3.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 4 and line segment 3
            if (!IsDivisible(resultLine3.PolarAngle - resultArc4.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 4 and line segment 4
            if (!IsDivisible(resultLine4.PolarAngle - resultArc4.EndAngle, Math.PI / 2, precision))
                return false;

            // Check tangent constraint between arc 1 and line segment 4
            if (!IsDivisible(resultLine4.PolarAngle - resultArc1.EndAngle, Math.PI / 2, precision))
                return false;

            return true;
        }

        private static bool IsDivisible(double dividend, double divider, double precision)
        {
            var quotient = Math.Round(dividend / divider);

            return Math.Abs(quotient * divider - dividend) < precision;
        }

    } // class Program3

} // namespace CloudInvent.Cheetah.Examples.RoundedRectangle
