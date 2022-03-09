using System;
using System.Collections.Generic;
class Interpolate
{
    static void Main(string[] args)
    {   
        List<Point3D> inputPoints = new List<Point3D>();
        List<double> inputValues = new List<double>();
        List<byte> outputValues = new List<byte>();

        int p = 3;
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
                Point3D point = new Point3D (x, y, z);
                inputPoints.Add(point);
                inputValues.Add(value);
            }
        }
        // Determine max value of points (for maping to uint8 range later)
        double maxValue = findMaxValue(inputValues);

        // TODO: Parse arguments
        //Console.WriteLine("Argument length: " + args.Length);
        for (int i = 0; i < args.Length; i++) {
            //Console.WriteLine(args[i]);
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
                        double pointValue = inputValues[k];
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
                    
                    //byte value = (byte) 255;
                    double thisValue = stevec/imenovalec;
                    //outputValues.Add(thisValue);
                    //Console.WriteLine(xp + " " + yp + " " + " " + zp + " " + "     " + thisValue); 

                    byte roundedValue = (byte)Convert.ToByte((255/maxValue) * thisValue); // Map from range 0-maxValue ro range 0-255
                    outputValues.Add(roundedValue);
                    //byte valueToByte =  Convert.ToByte(thisValue);
                    //Console.WriteLine(roundedValue);
                    //byteArray[x+y+z] = valueToByte;
                    
                    
                    //Console.WriteLine(stevec + " " + imenovalec + " "  + thisValue + " " + Math.Round(mapRange(0, findMaxValue(inputValues), 0, 255, thisValue)));

                    //Console.WriteLine(Convert.ToString((int)Math.Round(mapRange(0, findMaxValue(inputValues), 0, 255, thisValue)), 2));
                }
            }
        }
        ByteArrayToFile(Environment.CurrentDirectory + @"\output2.raw", outputValues);
    }
    public static float mapRange(float a1,float a2,float b1,float b2,float s)
        {
	        return b1 + (s-a1)*(b2-b1)/(a2-a1);
        }
    public static double findMaxValue(List<double> list)
    {
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Empty list");
        }
        double maxAge = double.MinValue;
        foreach (double type in list)
        {
            if (type > maxAge)
            {
                maxAge = type;
            }
        }
        return maxAge;
    }
    public static bool ByteArrayToFile(string fileName, List<byte> doubleArray)
    {
        try
        {   
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(fs, System.Text.Encoding.UTF8))
            {
                byte[] myArryByte = doubleArray.ToArray();
                writer.Write(myArryByte);
                /*foreach (var b in doubleArray)
                {
                    writer.Write(b);
                }*/
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

    static public string ToReadableByteArray(byte[] bytes)
    {
        return string.Join(", ", bytes);
    }
}
