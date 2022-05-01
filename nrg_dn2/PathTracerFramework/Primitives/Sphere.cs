using System;
using MathNet.Numerics.Integration;

namespace PathTracer
{
    /// <summary>
    /// Sphere Shape template class - NOT implemented completely
    /// </summary>
    class Sphere : Shape
    {
        public double Radius { get; set; }
        public Sphere(double radius, Transform objectToWorld)
        {
            Radius = radius;
            ObjectToWorld = objectToWorld;
        }

        /// <summary>
        /// Ray-Sphere intersection - NOT implemented
        /// </summary>
        /// <param name="r">Ray</param>
        /// <returns>t or null if no hit, point on surface</returns>
        public override (double?, SurfaceInteraction) Intersect(Ray ray)
        {
            Ray r = WorldToObject.Apply(ray);

            // TODO: Compute quadratic sphere coefficients
            double a = Vector3.Dot(r.d, r.d);
            double b = 2 * Vector3.Dot(r.d, r.o);
            double c = Vector3.Dot(r.o, r.o) - Radius * Radius;

            // TODO: Solve quadratic equation for _t_ values
            (bool solvable, double t0, double t1) = Utils.Quadratic(a, b, c);

            if (!solvable || t1 <= 0) {
                return (null, null);
            }

            // TODO: Check quadric shape _t0_ and _t1_ for nearest intersection
            double t = t0;
            t = t > 0 ? t : t1;

            // TODO: Compute sphere hit position and $\phi$
            Vector3 point = r.Point(t);

            // TODO: Return shape hit and surface interaction
            Vector3 normal = point.Clone().Normalize();
            Vector3 wOut = - r.d;
            Vector3 dpdu = new Vector3(-point.y, point.x, 0);

            SurfaceInteraction interection = new SurfaceInteraction(point, normal, wOut, dpdu, this);
            SurfaceInteraction worldCoordsIntersection = ObjectToWorld.Apply(interection);

            return (t, worldCoordsIntersection);
        }

        /// <summary>
        /// Sample point on sphere in world
        /// </summary>
        /// <returns>point in world, pdf of point</returns>
        public override (SurfaceInteraction, double) Sample()
        {
            // TODO: Implement Sphere sampling
            Vector3 sampledPointOnObject = new Vector3(0, 0, 0) + Radius * Samplers.UniformSampleSphere();

            // TODO: Return surface interaction and pdf
            bool outsideOrientation = true;
            Vector3 normal = ObjectToWorld.ApplyNormal(sampledPointOnObject);
            normal = outsideOrientation ? normal : -normal;

            sampledPointOnObject *= Radius / sampledPointOnObject.Length();
            Vector3 dpdu = new Vector3(-sampledPointOnObject.y, sampledPointOnObject.x, 0);
            double pdf = 1 / Area();

            return (ObjectToWorld.Apply(new SurfaceInteraction(sampledPointOnObject, normal, Vector3.ZeroVector, dpdu, this)), pdf);
        }

        public override double Area() { return 4 * Math.PI * Radius * Radius; }

        /// <summary>
        /// Estimates pdf of wi starting from point si
        /// </summary>
        /// <param name="si">point on surface that wi starts from</param>
        /// <param name="wi">wi</param>
        /// <returns>pdf of wi given this shape</returns>
        public override double Pdf(SurfaceInteraction si, Vector3 wi)
        {
            throw new NotImplementedException();
        }

    }
}
