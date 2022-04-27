using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    /// <summary>
    /// Example implementation of a diffuse area light
    /// </summary>
    class DiffuseAreaLight : Light
    {
        /// <summary>
        /// shape of light
        /// </summary>
        Shape shape;
        /// <summary>
        /// Color of light
        /// </summary>
        Spectrum Lemit;

        public DiffuseAreaLight(Shape s, Spectrum l, double intensity = 1)
        {
            shape = s;
            Lemit = l * intensity;
        }

        /// <summary>
        /// Intersection of ray with light
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public override (double?, SurfaceInteraction) Intersect(Ray r)
        {
            (double? t, SurfaceInteraction si) = shape.Intersect(r);
            if (si != null)
                si.Obj = this;
            return (t, si);
        }

        /// <summary>
        /// Sample point on light
        /// </summary>
        /// <returns></returns>
        public override (SurfaceInteraction, double) Sample()
        {
            return shape.Sample();
        }

        /// <summary>
        /// Samples a light ray from the given point to the light
        /// </summary>
        /// <param name="source">point to start ray from</param>
        /// <returns>light spectrum, sampled wi, pdf of wi, point on light</returns>
        public override (Spectrum, Vector3, double, Vector3) Sample_Li(SurfaceInteraction source)
        {
            (SurfaceInteraction pShape, double pdf) = shape.Sample(source);

            if (pdf == 0 || (pShape.Point - source.Point).LengthSquared() < Renderer.Epsilon)
            {
                return (Spectrum.ZeroSpectrum, Vector3.ZeroVector, 0, Vector3.ZeroVector);
            }

            var wi = (pShape.Point - source.Point).Normalize();
            var Li = L(pShape, -wi);
            return (Li, wi, pdf, pShape.Point);
        }

        /// <summary>
        /// Return emmited radiance
        /// </summary>
        /// <param name="intr">point on surface</param>
        /// <param name="w">direction of emission</param>
        /// <returns></returns>
        public override Spectrum L(SurfaceInteraction intr, Vector3 w)
        {
            return (Vector3.Dot(intr.Normal, w) > 0) ? Lemit : Spectrum.ZeroSpectrum;
        }

        /// <summary>
        /// Returns pdf given starting point si and wi
        /// </summary>
        /// <param name="si">starting point</param>
        /// <param name="wi">direction</param>
        /// <returns>pdf</returns>
        public override double Pdf_Li(SurfaceInteraction si, Vector3 wi)
        {
            return shape.Pdf(si, wi);
        }

    }
}
