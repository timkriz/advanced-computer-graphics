
using System;
public class Point3D
{
    protected double x, y, z;


    public Point3D()
    {
    }

    public Point3D(double nx, double ny, double nz)
    {
        MoveTo(nx, ny, nz);
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

    public void MoveTo(double nx, double ny, double nz)
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
