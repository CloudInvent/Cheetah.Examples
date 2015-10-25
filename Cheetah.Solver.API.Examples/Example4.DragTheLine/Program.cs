using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudInvent.Cheetah.Examples.DragTheLine
{
    using Data;
    using Data.DataSetBuilder;
    using Data.Geometry;
    using Data.ValueReference;

    using Parametric;
    using Solver.Cpu10;
    using Interfaces;

    class Program4
    {
        static void Main(string[] args)
        {
            // 1. Creating data set
            var dataSet = new CheetahDataSet();

            // 2. Creating geometry
            var line1 = new CheetahLine2D(13, 20, 10, 12);
            var line2 = new CheetahLine2D(8, 10, 20, 9);

            // 3. Creating constraints (End point of line1 on line2)
            dataSet.AddPointOnCurve(line1, IdentifiableValueReferences.LineEnd,
                line2);

            // 4. Creating solver object
            var solver = new SolverCpu10();

            // 5. First we will evaluate the system the way as we just have added new constraint
            var resultGeometry = EvaluationOnTheFirstApplicationOfConstraint(solver, dataSet);

            // 6. Apply the solution to data set
            var resultLine1 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line1.Id);
            var resultLine2 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line2.Id);
            line1.FillData(resultLine1);
            line2.FillData(resultLine2);

            // 7. Just to check, we don't need to do it all the time, because if parametric.Evaluate returned true then the problem is solved correctly
            if (!IsPointOnLine(line1.End, line2))
                throw new Exception("Something goes wrong");

            // 8. Now we will try to simulate drag situation
            resultGeometry = EvaluationOnDragginTheLine(solver, dataSet, line1, line1.End);

            // 9. Apply the solution to data set
            resultLine1 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line1.Id);
            resultLine2 = (CheetahLine2D)resultGeometry.Single(x => x.Id == line2.Id);
            line1.FillData(resultLine1);
            line2.FillData(resultLine2);


            // 10. Just to check again
            if (!IsPointOnLine(line1.End, line2))
                throw new Exception("Something goes wrong");

        }

        static ICollection<CheetahCurve> EvaluationOnTheFirstApplicationOfConstraint(ICheetahSolver solver, CheetahDataSet dataSet)
        {
            // 1. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false, true, true);

            // 2. Initializing parametric object using data set
            // On this step we compiling the model and creating system of equation
            // After this we will ready to run the solver
            if (!parametric.Init(dataSet, null, null))
                // If parametric.Init will return FALSE, then there is critical error in data set
                // So we are not able to make system of equation and to evaluate the problem
                // We should cancel the procedure and rebuild data set
                throw new Exception("Something goes wrong");

            // 3. Regenerating constrained model (running solver)
            // On this step we are solving the system of equation, that we've made in the previous step
            if (!parametric.Evaluate())
                throw new Exception("Something goes wrong");

            // 4. Retrieving results (we created rectangle that is "closest" to the initial lines)
            var resultGeometry = parametric.GetSolution(true);

            // 5. We have to clear compiled data
            parametric.ClearSolver();

            return resultGeometry;
        }

        static ICollection<CheetahCurve> EvaluationOnDragginTheLine(ICheetahSolver solver, CheetahDataSet dataSet, CheetahLine2D draggingLine, CheetahPoint2D draggingPoint)
        {
            // 1. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false, true, true);

            // 2. Initializing parametric object using data set
            // On this step we compiling the model and creating system of equation
            // After this we will ready to run the solver
            // We will drag the line, so we need to specify the curve to drag and the dragging point
            if (!parametric.Init(dataSet, new[] { draggingLine }, new[] { draggingPoint }))
                // If parametric.Init will return FALSE, then there is critical error in data set
                // So we are not able to make system of equation and to evaluate the problem
                // We should cancel the procedure and rebuild data set
                throw new Exception("Something goes wrong");

            // 3. Now we can drag. We don't need to recompile the model on each drag iteration,
            // because the model will be the same - only initial value (the drag point) will change
            // We can simulate drag using loop
            for (int i = 0; i < 100; i++)
            {
                // 4. Our parametric object initialized by dataSet object
                // So the current values of dataSet curves are initial values of the system
                // If we will change some value - the initial values will be changed appropriately
                // All that we need to reinitialize model by the new point from the screen is to reset the value for the dragging line
                draggingLine.End.X *= 1.01;
                draggingLine.End.Y *= 1.05;

                // 5. Regenerating constrained model (running solver)
                // On this step we are solving the system of equation, that we've made on the previous step
                // Now we are dragging the curve and because we do it manually by mouse we can not be precise 
                // We don't need to calculate on each step with 10^-12 or more precision
                // So we can cheat a little and decrease the precision for drag iterations
                // This why we are using parametric.EvaluateFast function
                if (!parametric.EvaluateFast())
                    // If parametric.EvaluateFast will return FALSE it usually means that we can not evaluate the system with this initial values (we have reached maximum number of iterations)
                    // It is NOT a critical problem. This situation can appear while we drag some curve in position that conflict with constraints
                    // For example, while we rounding out the arc and get negative radius, or squeeze the line to zero length (and get negative length)
                    // In our plugin, if we are getting this situation - we are restoring the previous drag iteration solution
                    // On a scheme it looks like we stuck and when we will move the line back our equation system will be again resolvable
                    throw new Exception("Something goes wrong");
            }

            // 6. Regenerating constrained model (running solver)
            // Now we click (or leave) the mouse button to end the drag
            // We need to evaluate the system with exact precision
            // We can use last iteration solution as new initial value to evaluate the system much more fast (set recompile parameter to false)
            if (!parametric.Evaluate(false))
                throw new Exception("Something goes wrong");

            // 7. Retrieving results (we created rectangle that is "closest" to the initial lines)
            var resultGeometry = parametric.GetSolution(true);

            // 8. We have to clear compiled data
            parametric.ClearSolver();

            return resultGeometry;
        }

        static bool IsPointOnLine(CheetahPoint2D point, CheetahLine2D line)
        {
            var k = (line.End.Y - line.Start.Y) / (line.End.X - line.Start.X);
            var b = (line.Start.Y * line.End.X - line.End.Y * line.Start.X) / (line.End.X - line.Start.X);

            var y = k * point.X + b;

            return Math.Abs(y - point.Y) < CheetahParametricBasic.Settings.Precision;
        }
    }
}
