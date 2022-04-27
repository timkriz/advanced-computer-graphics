using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    /// <summary>
    /// Base class representing point on surface, with its normal and du dv directions
    /// </summary>
    public class SurfaceInteraction
    {
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; private set; }
        public Vector3 Dpdu { get; private set; }
        public Vector3 Dpdv { get; private set; }
        public Vector3 Wo { get; set; }
        public Primitive Obj { get; set; }
        public SurfaceInteraction(Vector3 point, Vector3 normal, Vector3 wo, Vector3 dpdu, Primitive obj)
        {
            Point = point;
            Normal = normal.Clone().Normalize();
            Wo = wo;
            Obj = obj;
            Dpdu = dpdu.Clone().Normalize();
            Dpdv = Vector3.Cross(Normal, Dpdu);
        }

        /// <summary>
        /// Emission, only lights emit
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public Spectrum Le(Vector3 w)
        {
            if (Obj is Light)
                return (Obj as Light).L(this, w);
            return Spectrum.ZeroSpectrum;
        }

        /// <summary>
        /// Start ray in direction wi from point on surface
        /// </summary>
        /// <param name="wi"></param>
        /// <returns></returns>
        public Ray SpawnRay(Vector3 wi)
        {
            return new Ray(Point, wi);
        }
    }
}
