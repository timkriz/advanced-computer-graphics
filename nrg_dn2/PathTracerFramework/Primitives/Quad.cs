using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    /// <summary>
    /// Example of a Quad Shape 
    /// </summary>
    class Quad : Shape
    {
        private double width;
        private double height;

        public Quad(double w, double h, Transform objectToWorld)
        {
            width = w;
            height = h;
            ObjectToWorld = objectToWorld;
        }

        /// <summary>
        /// Sample point on quad in world
        /// </summary>
        /// <returns>point in world, pdf of point</returns>
        public override (SurfaceInteraction, double) Sample()
        {
            (double x, double y) = Samplers.UniformSampleSquare();

            var pObj = new Vector3((x - 0.5) * width, (y - 0.5) * height, 0);

            var n = new Vector3(0, 0, 1);
            var dpdu = new Vector3(1, 0, 0);
            double pdf = 1 / Area();
            return (ObjectToWorld.Apply(new SurfaceInteraction(pObj, n, Vector3.ZeroVector, dpdu, this)), pdf);
        }

        public override double Area()
        {
            return width * height;
        }

        /// <summary>
        /// Ray-Quad intersection
        /// </summary>
        /// <param name="r">Ray</param>
        /// <returns>t or null if no hit, point on surface</returns>
        public override (double?, SurfaceInteraction) Intersect(Ray r)
        {
            var ray = WorldToObject.Apply(r);

            // Compute plane intersection for quad

            // Reject intersections for rays parallel to the quad's plane
            if (ray.d.z == 0)
                return (null, null);

            double tShapeHit = -ray.o.z / ray.d.z;
            if (tShapeHit <= Renderer.Epsilon)
                return (null, null);

            // See if hit point is inside 
            var pHit = ray.Point(tShapeHit);
            if (pHit.x < -width / 2 || pHit.x > width / 2 || pHit.y < -height / 2 || pHit.y > height / 2)
                return (null, null);

            // Refine disk intersection point
            pHit.z = 0;
            var dpdu = new Vector3(1, 0, 0);

            var si = new SurfaceInteraction(pHit, new Vector3(0, 0, 1), -ray.d, dpdu, this);
            return (tShapeHit, ObjectToWorld.Apply(si));
        }
    }


}
