using System;
using Larsa4D;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new LarsaApp(LarsaAppMode.Standalone);

            // Create Joints

            var joint1 = new LarsaJoint();
            app.Joints.Add(joint1);

            var joint2 = new LarsaJoint();
            app.Joints.Add(joint2);

            var joint3 = new LarsaJoint();
            app.Joints.Add(joint3);

            var joint4 = new LarsaJoint();
            app.Joints.Add(joint4);

            // Create Plates

            var plate1 = new LarsaPlate();
            plate1.IJoint = joint1;
            plate1.JJoint = joint2;
            plate1.KJoint = joint3;
            plate1.LJoint = joint4;
            app.Plates.Add(plate1);

            // Create a Surface

            var lane = new LarsaLane();
            app.Lanes.Add(lane); // must be before setting elementNumber

            var pe = new LarsaPathElement();
            lane.path.Add(pe); // must be before setting elementNumber
            pe.elementType = LarsaPathElement.ElementType.Plate; // must be before setting elementNumber
            pe.elementNumber = plate1.Number;

            pe = new LarsaPathElement();
            lane.path.Add(pe); // must be before setting elementNumber
            pe.elementType = LarsaPathElement.ElementType.Plate; // must be before setting elementNumber
            pe.elementNumber = plate1.Number;
            pe.offsetX = 10;

            Console.ReadLine();
        }
    }
}
