using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System;

public class Octree<TType> {
    OctreeNode<TType> node;

    public Octree(Point3D position, double size, int nCapacity)
    {
        this.node = new OctreeNode<TType>(position, size, nCapacity);
    }
    public List<Point3D> query(Point3D point, double radius) {
        return node.queryNeighbours(new Cube(point, radius), new List<Point3D>());
    }
    public void insertPoint(Point3D point) {
        node.insert(point);
    }


    public class OctreeNode<T>
    {
        Cube subspace;
        int capacity; // Resolution, how small the cubes get. (num. of subdivisions)
        List<Point3D> allPoints;
        List<OctreeNode<TType>> children;
        bool divided = false;


        public OctreeNode(Point3D pos, double size, int nCapacity = 4)
        {
            this.subspace = new Cube(pos, size);
            this.capacity = nCapacity;
            this.allPoints = new List<Point3D>();
            this.children = new List<OctreeNode<TType>>();
        }

         public bool insert(Point3D point) {
            if (!this.subspace.contains(point)) {
                //Console.WriteLine("ignore: " + point.x);
                return false; // If this point is not inside this subspace just ignore.
            }
            
            if (this.allPoints.Count < this.capacity)
            {
                this.allPoints.Add(point); // There is room in this node for a point
                //Console.WriteLine("New point added");
                return true;
            } else {
                if (!this.divided) {
                    this.subdivide(); // If it hasn't been divided, divide space
                    //Console.WriteLine("Subdivision: " + point.x);
                    this.divided = true;
                }
                //Console.WriteLine(children);
                for (int i = 0; i < children.Count; i++) {
                    if(children[i].insert(point)){
                        //Console.WriteLine("point: " + point.x + "   in: " + children[i].subspace.position.x + "  " + children[i].subspace.position.y + "  "+ children[i].subspace.position.z);
                        return true;
                    }
                }
                return false;
            }
        }

        public void subdivide() {
            double nSize = this.subspace.size / 2;
            double centerOffset = nSize / 2;
            Point3D center = this.subspace.position;
            OctreeNode<TType> topLeftFront = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y + centerOffset, center.z + centerOffset), nSize);
            OctreeNode<TType> topRightFront = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y + centerOffset, center.z + centerOffset), nSize);
            OctreeNode<TType> bottomLeftFront = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y - centerOffset, center.z + centerOffset), nSize);
            OctreeNode<TType> bottomRightFront = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y - centerOffset, center.z + centerOffset), nSize);
            OctreeNode<TType> topLeftBack = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y + centerOffset, center.z - centerOffset), nSize);
            OctreeNode<TType> topRightBack = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y + centerOffset, center.z - centerOffset), nSize);
            OctreeNode<TType> bottomLeftBack = new OctreeNode<TType>(new Point3D(center.x - centerOffset, center.y - centerOffset, center.z - centerOffset), nSize);
            OctreeNode<TType> bottomRightBack = new OctreeNode<TType>(new Point3D(center.x + centerOffset, center.y - centerOffset, center.z - centerOffset), nSize);
            children.Add(topLeftFront);
            children.Add(topRightFront);
            children.Add(bottomLeftFront);
            children.Add(bottomRightFront);
            children.Add(topLeftBack);
            children.Add(topRightBack);
            children.Add(bottomLeftBack);
            children.Add(bottomRightBack);
        }

        public List<Point3D> queryNeighbours(Cube range, List<Point3D> found) {
            //Console.WriteLine("Query");
            if(!this.subspace.intersects(range)) {
                //Console.WriteLine("Query empty ");
                return found;
            }

            allPoints.ForEach(point => {
                //Console.WriteLine("Query: "+ point.x);
                if (range.contains(point)) {
                    //Console.WriteLine("Query FOUND: "+ point.x);
                    found.Add(point);
                }
            });
            
            if(this.divided) {
                for (int i = 0; i < children.Count; i++) {
                    children[i].queryNeighbours(range, found);
                }
            }
            return found;
        }
    }

    public class Cube {
        public Point3D position; // Center of cube
        public double size; // Width

        public Cube(Point3D pos, double size)
        {
            this.position = pos;
            this.size = size;
        }

        public bool contains(Point3D point) {
            return (point.x >= position.x - size/2 && point.x <= position.x + size/2 &&
                    point.y >= position.y - size/2 && point.y <= position.y + size/2 && 
                    point.z >= position.z - size/2 && point.z <= position.z + size/2
            );
        }

        public bool intersects(Cube range) {
            
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
            ((cubeAminz <= cubeBminz && cubeBminz <= cubeAmaxz) || (cubeBminz <= cubeAminz && cubeAminz <= cubeBmaxz)) 
            );
        }

    }
}