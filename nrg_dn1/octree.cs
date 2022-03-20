using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System;

public class Octree<TType> 
{
    OctreeNode<TType> rootNode;

    public Octree(Point3D position, double size, int bucketSize)
    {
        this.rootNode = new OctreeNode<TType>(position, size, bucketSize);
    }
    public List<Point3D> queryNeighbours(Point3D point, double radius) {
        return rootNode.queryNeighbours(new Cube(point, radius), new List<Point3D>());
    }
    public void insertPoint(Point3D point) {
        rootNode.insert(point);
    }

    public class OctreeNode<T>
    {
        Cube space;                             // Center position and size
        List<Point3D> allPoints;                // All points
        List<OctreeNode<TType>> children;       // 0 or 8 child nodes
        int bucketSize;                         // Max num. of points in node
        bool divided = false;


        public OctreeNode(Point3D position, double size, int bucketSize)
        {
            this.space = new Cube(position, size);
            this.bucketSize = bucketSize;
            this.allPoints = new List<Point3D>();
            this.children = new List<OctreeNode<TType>>();
        }

         public bool insert(Point3D point) {
            if (!this.space.contains(point)) {
                return false;                   // If a point does not belong inside this subspace don't do anything
            }
            
            if (this.allPoints.Count < this.bucketSize)
            {
                this.allPoints.Add(point);      // There is room inside this node for a new point
                return true;
            } else {
                if (!this.divided) {
                    this.subdivide();           // If node hasn't been divided yet, divide space into 8 subnodes
                    this.divided = true;
                }
                for (int i = 0; i < children.Count; i++) {
                    if(children[i].insert(point)){
                        return true;
                    }
                }
                return false;
            }
        }

        public void subdivide()
        {
            double nSize = this.space.size / 2; // Size of child
            double centerOffset = nSize / 2;
            Point3D center = this.space.position;
            OctreeNode<TType> topLeftFront = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y + centerOffset, center.z + centerOffset), nSize, bucketSize);
            OctreeNode<TType> topRightFront = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y + centerOffset, center.z + centerOffset), nSize, bucketSize);
            OctreeNode<TType> bottomLeftFront = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y - centerOffset, center.z + centerOffset), nSize, bucketSize);
            OctreeNode<TType> bottomRightFront = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y - centerOffset, center.z + centerOffset), nSize, bucketSize);
            OctreeNode<TType> topLeftBack = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y + centerOffset, center.z - centerOffset), nSize, bucketSize);
            OctreeNode<TType> topRightBack = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y + centerOffset, center.z - centerOffset), nSize, bucketSize);
            OctreeNode<TType> bottomLeftBack = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y - centerOffset, center.z - centerOffset), nSize, bucketSize);
            OctreeNode<TType> bottomRightBack = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y - centerOffset, center.z - centerOffset), nSize, bucketSize);
            children.Add(topLeftFront);
            children.Add(topRightFront);
            children.Add(bottomLeftFront);
            children.Add(bottomRightFront);
            children.Add(topLeftBack);
            children.Add(topRightBack);
            children.Add(bottomLeftBack);
            children.Add(bottomRightBack);
        }

        public List<Point3D> queryNeighbours(Cube qRange, List<Point3D> found)
        {
            if(!this.space.intersects(qRange)) {
                return found;       // Node is outside of query range
            }

            this.allPoints.ForEach(point => {
                if (qRange.contains(point)) {
                    found.Add(point); //
                }
            });
            
            if(this.divided) {
                for (int i = 0; i < children.Count; i++) {
                    children[i].queryNeighbours(qRange, found);
                }
            }
            return found;
        }
    }

    /**** 3-dimensional subspace ****/
    public class Cube
    {
        public Point3D position;    // Center
        public double size;         // Width

        public Cube(Point3D position, double size)
        {
            this.position = position;
            this.size = size;
        }

        public bool contains(Point3D point)
        {
            return (point.x >= position.x - size/2 && point.x <= position.x + size/2 &&
                    point.y >= position.y - size/2 && point.y <= position.y + size/2 && 
                    point.z >= position.z - size/2 && point.z <= position.z + size/2 );
        }

        public bool intersects(Cube range)
        {
            
            Point3D point = range.position;
            double radius= range.size;

            double cubeAminx = position.x - size/2;
            double cubeAminy = position.y - size/2;
            double cubeAminz = position.z - size/2;
            double cubeAmaxx = position.x + size/2;
            double cubeAmaxy = position.y + size/2;
            double cubeAmaxz = position.z + size/2;

            double cubeBminx = point.x - radius;
            double cubeBminy = point.y - radius;
            double cubeBminz = point.z - radius;
            double cubeBmaxx = point.x + radius;
            double cubeBmaxy = point.y + radius;
            double cubeBmaxz = point.z + radius;

            return ( 
            ((cubeAminx <= cubeBminx && cubeBminx <= cubeAmaxx) || (cubeBminx <= cubeAminx && cubeAminx <= cubeBmaxx)) &&
            ((cubeAminy <= cubeBminy && cubeBminy <= cubeAmaxy) || (cubeBminy <= cubeAminy && cubeAminy <= cubeBmaxy)) &&
            ((cubeAminz <= cubeBminz && cubeBminz <= cubeAmaxz) || (cubeBminz <= cubeAminz && cubeAminz <= cubeBmaxz)) );
        }
    }
}