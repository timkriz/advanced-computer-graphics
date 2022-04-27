using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    /// <summary>
    /// Example implementation of a specular reflection material
    /// </summary>
    public class SpecularReflection : BxDF
    {
        /// <summary>
        /// material color
        /// </summary>
        private Spectrum r;
        /// <summary>
        /// Fresnel parameters
        /// </summary>
        private FresnelDielectric fresnel;

        public override bool IsSpecular => true;

        public SpecularReflection(Spectrum r, double fresnel1, double fresnel2)
        {
            this.r = r;
            fresnel = new FresnelDielectric(fresnel1, fresnel2);
        }

        /// <summary>
        /// f of perfect specular transmission is zero (probability also)
        /// </summary>
        /// <param name="wo"></param>
        /// <param name="wi"></param>
        /// <returns></returns>
        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            return Spectrum.ZeroSpectrum;
        }

        /// <summary>
        /// Sample returns the single possible direction
        /// </summary>
        /// <param name="woL">wo in local coordinates</param>
        /// <returns>f as fresnel corrected color, wi as perfect reflection and 1 as pdf</returns>
        public override (Spectrum, Vector3, double) Sample_f(Vector3 woL)
        {
            // perfect specular reflection
            Vector3 wiL = new Vector3(-woL.x, -woL.y, woL.z);
            Spectrum ft = r * fresnel.Evaluate(Utils.CosTheta(wiL));
            return (ft / Utils.AbsCosTheta(wiL), wiL, 1);
        }

        /// <summary>
        /// Probability of any pair is 0
        /// </summary>
        /// <param name="wo"></param>
        /// <param name="wi"></param>
        /// <returns></returns>
        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            return 0;
        }
    }
}
