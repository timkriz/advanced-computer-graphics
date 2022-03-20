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
    static int p;                                           // Controls the shape of the interpolant.
    static double r;                                        // Radius of influence in modified method
    static int octreeBucketSize;
    
    static void Main(string[] args)
    {   
        string outputFileName = @"\output.raw";

        method = Method.basic;
        octreeBucketSize = 4;
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

        List<float> outputValues = shepardsInterpolation(inputPoints, method);          // Interpolated values

        // 1. OUTPUT: Float represenatation
        floatListToFile(Environment.CurrentDirectory + outputFileName, outputValues);   // Write to binary file

        // 2. OUTPUT: Byte represenatation for visualization
        float maxValue = findMaxValue(outputValues);
        List<byte> mappedOutputValues = new List<byte>();
        for (int i = 0; i < outputValues.Count; i++) {
            mappedOutputValues.Add((byte)Convert.ToByte(mapRange(0, maxValue, 0, 255, outputValues[i]))); // Map range from float to byte
        }
        //byteListToFile(Environment.CurrentDirectory + outputFileName, mappedOutputValues); // Write to binary file
    }

    public static List<float> shepardsInterpolation(List<Point3D> points, Method method)
    {
        
        double stepX = Math.Abs(maxX-minX)/(resX-1);
        double stepY = Math.Abs(maxY-minY)/(resY-1);
        double stepZ = Math.Abs(maxZ-minZ)/(resZ-1);

        List<float> outputValues = new List<float>();
        Octree<Point3D> octree = null;  // Initialize only for modified method

        if (method == Method.modified) {
            octree = constructOctree(points, new Point3D(0.0, 0.0, 0.0), Math.Abs(maxX-minX), octreeBucketSize);
        }

        for (int z = 0; z < resZ; z++) {
            for (int y = 0; y < resY; y++) {
                for (int x = 0; x < resX; x++) {
                    double xp = minX + x*stepX;
                    double yp = minY + y*stepY;
                    double zp = minZ + z*stepZ;

                    Point3D interpolatedPoint = new Point3D (xp, yp, zp); // Point in volume
                    
                    float interpolatedPointValue = method == Method.basic ? (float)getBasicInterpolatedPointValue(interpolatedPoint, points) : (float)getModifiedInterpolatedPointValue(interpolatedPoint, points, octree);
                    interpolatedPoint.SetValue(interpolatedPointValue);
                    outputValues.Add(interpolatedPointValue);
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
        List<Point3D> radiusNeighbours = octree.queryNeighbours(interpolatedPoint, r);
        if (radiusNeighbours.Count == 0) {
            double bigger_r = r;    // No neighbours in this radius. Find nearest point.
            while (radiusNeighbours.Count < 1) {
                bigger_r= bigger_r + bigger_r/4;
                radiusNeighbours = octree.queryNeighbours(interpolatedPoint, bigger_r);
            }
            return radiusNeighbours[0].GetValue();
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

    public static Octree<Point3D> constructOctree(List<Point3D> inputPoints, Point3D position, double size, int bucketSize)
    {
        Octree<Point3D> octree = new Octree<Point3D>(position, size, bucketSize);
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

    public static float findMaxValue(List<float> list)
    {
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Empty list");
        }
        float maxValue = float.MinValue;
        foreach (float value in list)
        {
            if (value > maxValue)
            {
                maxValue = value;
            }
        }
        return maxValue;
    }

    public static bool byteListToFile(string fileName, List<byte> byteList)
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
    public static bool floatListToFile(string fileName, List<float> floatList)
    {
        try
        {   
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fs))
            {
                float[] floatArray = floatList.ToArray();
                foreach (float value in floatArray)
                {
                    writer.Write(value);
                }
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
