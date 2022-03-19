using System;
using System.Collections.Generic;

class Interpolate
{   
    public enum Method
    {
        basic,
        modified,
    }
    static Method method;
    static float minX, minY, minZ, maxX, maxY, maxZ;
    static int resX, resY, resZ;
    static int p; // Controls the shape of the interpolant.
    static double r; // Radius of influence in modified method
    
    static void Main(string[] args)
    {   
        string outputFileName = @"\output.raw";

        method = Method.basic;
        p = 2; 
        r = 0.5;
        minX = -1.5f;
        minY = -1.5f;
        minZ = -1.0f;

        maxX = 1.5f;
        maxY = 1.5f;
        maxZ = 1.0f;

        resX = 128;
        resY = 128;
        resZ = 64;

        List<Point3D> inputPoints = readInputFile(); // Read input
        parseInputArguments(args);

        List<byte> outputValues = shepardsInterpolation(inputPoints, method); // Interpolate

        byteArrayToFile(Environment.CurrentDirectory + outputFileName, outputValues); // Write to file
    }

    public static List<Byte> shepardsInterpolation(List<Point3D> points, Method method)
    {
        
        double stepX = Math.Abs(maxX-minX)/(resX-1);
        double stepY = Math.Abs(maxY-minY)/(resY-1);
        double stepZ = Math.Abs(maxZ-minZ)/(resZ-1);

        double maxValue = findMaxValue(points); // Determine max value of points for range calculation
        List<byte> outputValues = new List<byte>();
        Octree<Point3D> octree = null;

        if (method == Method.modified) {
            octree = constructOctree(points);
        }

        for (int z = 0; z < resZ; z++) {
            for (int y = 0; y < resY; y++) {
                for (int x = 0; x < resX; x++) {
                    double xp = minX + x*stepX;
                    double yp = minY + y*stepY;
                    double zp = minZ + z*stepZ;

                    Point3D interpolatedPoint = new Point3D (xp, yp, zp); // Point in volume
                    
                    double interpolatedPointValue = method == Method.basic ? getBasicInterpolatedPointValue(interpolatedPoint, points) : getModifiedInterpolatedPointValue(interpolatedPoint, points, octree);
                    interpolatedPoint.SetValue(interpolatedPointValue);

                    byte roundedValue = (byte)Convert.ToByte(mapRange(0, maxValue, 0, 255, interpolatedPointValue)); // Map from range 0-maxValue to range 0-255
                    outputValues.Add(roundedValue);
                }
            }
        }
        return outputValues;
    }

    public static double getBasicInterpolatedPointValue(Point3D interpolatedPoint, List<Point3D> inputPoints)
    {
        double numerator = 0;
        double denominator = 0;
        for (int k = 0; k < inputPoints.Count; k++) {
            double pointValue = inputPoints[k].GetValue();
            double distance = interpolatedPoint.DistanceTo(inputPoints[k]);
            double wk = (double)(1 / Math.Pow(distance, p));  // Weight
            numerator += wk * pointValue;
            denominator += wk;
            if (distance == 0.0) {
                numerator = pointValue;
                denominator = 1;
                break;
            }
        }
        return numerator/denominator;
    }

    public static double getModifiedInterpolatedPointValue(Point3D interpolatedPoint, List<Point3D> inputPoints, Octree<Point3D> octree)
    {
        List<Point3D> radiusNeighbours = octree.query(interpolatedPoint, r);
        if (radiusNeighbours.Count == 0) {
            while (radiusNeighbours.Count<1) {
                r= r+0.1;
                Console.WriteLine("Not found in radius, making it bigger to " + r);
                radiusNeighbours = octree.query(interpolatedPoint, r);
            }
        }

        double numerator = 0;
        double denominator = 0;
        for (int k = 0; k < radiusNeighbours.Count; k++) {
            double pointValue = radiusNeighbours[k].GetValue();
            double distance = interpolatedPoint.DistanceTo(radiusNeighbours[k]);

            double wk = (double) Math.Pow((Math.Max(0, r-distance)) / r * distance, 2); // Weight
            numerator += wk * pointValue;
            denominator += wk;
            if (distance == 0) {
                numerator = pointValue;
                denominator = 1;
                break;
            }
        }
        return numerator/denominator;
    }

    public static Octree<Point3D> constructOctree(List<Point3D> inputPoints)
    {
        Octree<Point3D> octree = new Octree<Point3D>(new Point3D(0.0, 0.0, 0.0), 3, 4);
        for (int j = 0; j< inputPoints.Count; j++) {
            octree.insertPoint(inputPoints[j]);
        }
        return octree;
    }


    /**** Parse input ****/

    public static List<Point3D> readInputFile() 
    {
        List<Point3D> points = new List<Point3D>();
        string line;
        while ((line = Console.ReadLine()) != null) {
            string[] tokens = line.Split();
            double x = (double.Parse(tokens[0]));
            double y = (double.Parse(tokens[1]));
            double z = (double.Parse(tokens[2]));
            double value = (double.Parse(tokens[3]));
            if (x > maxX || x < minX || y > maxY || y < minY || z > maxZ || z < minZ) {
                // Ignore outliers
            } else {
                Point3D point = new Point3D (x, y, z);
                point.SetValue(value);
                points.Add(point);
            }
        }
        return points;
    }

    public static void parseInputArguments(string[] args) 
    {
        for (int i = 0; i < args.Length; i++) {
            if(String.Equals(args[i], "--r")) { r = Convert.ToDouble(args[i+1]);};
            if(String.Equals(args[i], "--p")) { p = Int32.Parse(args[i+1]);};
            if(String.Equals(args[i], "--min-x")) { minX = float.Parse(args[i+1]);};
            if(String.Equals(args[i], "--min-y")) { minY = float.Parse(args[i+1]);};
            if(String.Equals(args[i], "--min-z")) { minZ = float.Parse(args[i+1]);};
            if(String.Equals(args[i], "--max-x")) { maxX = float.Parse(args[i+1]);};
            if(String.Equals(args[i], "--max-y")) { maxY = float.Parse(args[i+1]);};
            if(String.Equals(args[i], "--max-z")) { maxZ = float.Parse(args[i+1]);};
            if(String.Equals(args[i], "--res-x")) { resX = Int32.Parse(args[i+1]);};
            if(String.Equals(args[i], "--res-y")) { resY = Int32.Parse(args[i+1]);};
            if(String.Equals(args[i], "--res-z")) { resZ = Int32.Parse(args[i+1]);};
            if(String.Equals(args[i], "--method")) 
            { 
                if (String.Equals(args[i+1], "modified")) 
                {
                    method = Method.modified;
                } else {
                    method = Method.basic;
                }
            }
        }
    }


    /**** Util functions ****/

    public static double mapRange(double a1,double a2,double b1,double b2,double s)
    {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    }

    public static double findMaxValue(List<Point3D> list)
    {
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Empty list");
        }
        double maxValue = double.MinValue;
        foreach (Point3D point in list)
        {
            if (point.GetValue() > maxValue)
            {
                maxValue = point.GetValue();
            }
        }
        return maxValue;
    }

    public static bool byteArrayToFile(string fileName, List<byte> byteList)
    {
        try
        {   
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fs))
            {
                byte[] byteArray = byteList.ToArray();
                writer.Write(byteArray);
                writer.Close();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception caught in process: {0}", ex);
            return false;
        }
    }
}
