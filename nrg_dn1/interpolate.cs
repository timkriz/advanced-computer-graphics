using System;
using System.Collections.Generic;
class Interpolate
{   
    public enum Method
{
    basic,
    modified,
}
    static void Main(string[] args)
    {   
        string outputFileName = @"\output.raw";

        Method method = Method.basic;
        List<Point3D> inputPoints = new List<Point3D>();
        List<byte> outputValues = new List<byte>();

        int p = 3;
        double r = 0.5;
        float minX = -1.5f;
        float minY = -1.5f;
        float minZ = -1.0f;

        float maxX = 1.5f;
        float maxY = 1.5f;
        float maxZ = 1.0f;

        int resX = 128;
        int resY = 128;
        int resZ = 64;

        // Read file
        string line;
        while ((line = Console.ReadLine()) != null) {
            string[] tokens = line.Split();
            double x = (double.Parse(tokens[0]));
            double y = (double.Parse(tokens[1]));
            double z = (double.Parse(tokens[2]));
            double value = (double.Parse(tokens[3]));
            if (x > maxX || x < minX || y > maxY || y < minY || z > maxZ || z < minZ) {
                // Remove outliers
            } else {
                Point3D point = new Point3D (x, y, z, value);
                inputPoints.Add(point);
            }
        }
        // Determine max value of points
        double maxValue = findMaxValue(inputPoints);

        // TODO: Parse arguments
        //Console.WriteLine("Argument length: " + args.Length);
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

        // Ouput
        double stepX = Math.Abs(maxX-minX)/(resX-1);
        double stepY = Math.Abs(maxY-minY)/(resY-1);
        double stepZ = Math.Abs(maxZ-minZ)/(resZ-1);

        for (int z = 0; z < resZ; z++) {
            for (int y = 0; y < resY; y++) {
                for (int x = 0; x < resX; x++) {
                    double xp = minX + x*stepX;
                    double yp = minY + y*stepY;
                    double zp = minZ + z*stepZ;

                    Point3D interpolatedPoint = new Point3D (xp, yp, zp); // Point in volume

                    double stevec = 0;
                    double imenovalec = 0;
                    for (int k = 0; k < inputPoints.Count; k++) {
                        double pointValue = inputPoints[k].GetValue();
                        double distance = interpolatedPoint.DistanceTo(inputPoints[k]);
                        double wk = (float)(1 / Math.Pow(distance, p));
                        stevec += wk * pointValue;
                        imenovalec += wk;
                        if (distance == 0) {
                            stevec = pointValue;
                            imenovalec = 1;
                            break;
                        }

                    }
                    double thisValue = stevec/imenovalec;
                    interpolatedPoint.SetValue(thisValue);

                    byte roundedValue = (byte)Convert.ToByte(mapRange(0, maxValue, 0, 255, thisValue)); // Map from range 0-maxValue to range 0-255
                    outputValues.Add(roundedValue);
                }
            }
        }
        ByteArrayToFile(Environment.CurrentDirectory + outputFileName, outputValues);
    }
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
    public static bool ByteArrayToFile(string fileName, List<byte> byteList)
    {
        try
        {   
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fs, System.Text.Encoding.UTF8))
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
