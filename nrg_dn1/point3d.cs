
using System;
public class Point3D
{
    protected double x, y, z;
    protected double value;


    public Point3D()
    {
    }

    public Point3D(double nx, double ny, double nz, double nValue = 0.0 )
    {
        SetCoordinates(nx, ny, nz);
        SetValue(nValue);
    }

    public double GetX()
    {
        return x;
    }

    public void SetX(double value)
    {
        x = value;
    }

    public double GetY()
    {
        return y;
    }

    public void SetY(double value)
    {
        y = value;
    }

    public double GetZ()
    {
        return z;
    }

    public void SetZ(double value)
    {
        z = value;
    }

    public double GetValue()
    {
        return value;
    }
    public void SetValue(double nValue)
    {
        value = nValue;
    }

    public void SetCoordinates(double nx, double ny, double nz)
    {
        x = nx;
        y = ny;
        z = nz;
    }

    public double DistanceTo(Point3D p2)
    {
        return (double)Math.Sqrt((x - p2.GetX()) * (x - p2.GetX()) +
        (y - p2.GetY()) * (y - p2.GetY()) +
        (z - p2.GetZ()) * (z - p2.GetZ()));
    }
}   
