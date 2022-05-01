using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    /// <summary>
    /// Example BxDF implementation of a Oren-SNayar surface
    /// </summary>
    public class OrenNayar : BxDF
    {
        private Spectrum kd;
        private double sigma; // Roughness parameter
        public OrenNayar(Spectrum r, double sigma)
        {
            kd = r;
            this.sigma = sigma;
        }

        /// <summary>
        /// Lambertian f is kd/pi
        /// </summary>
        /// <param name="wo">output vector</param>
        /// <param name="wi">input vector</param>
        /// <returns></returns>
        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            if (!Utils.SameHemisphere(wo, wi))
                return Spectrum.ZeroSpectrum;

            Spectrum lambertianSpectrum = kd * Utils.PiInv;
            double sigmaPowOfTwo = sigma * sigma;

            double A = 1 - sigmaPowOfTwo / (2 * (sigmaPowOfTwo + 0.33));
            double B = 0.45 * sigmaPowOfTwo / (sigmaPowOfTwo + 0.09);

            double woTheta = vectorToSphericalTheta(wo); //θ
            double woPhi = vectorToSphericalPhi(wo); //φ
            double wiTheta = vectorToSphericalTheta(wi); //θ
            double wiPhi = vectorToSphericalPhi(wi); //φ

            double alpha = Math.Max(woTheta, wiTheta);
            double beta = Math.Min(woTheta, wiTheta);

            double factorAfterB = Math.Max(0, Math.Cos(wiPhi - woPhi));

            Spectrum secondPart = Spectrum.Create( A + B * factorAfterB * Math.Sin(alpha) * Math.Tan(beta));
            return lambertianSpectrum * secondPart;
        }

        public double vectorToSphericalTheta(Vector3 v)
        {
            return Math.Atan(Math.Sqrt(v.x*v.x + v.y*v.y) / v.z);
        }
        public double vectorToSphericalPhi(Vector3 v)
        {
            return Math.Atan(v.y/v.x);
        }

        /// <summary>
        /// Cosine weighted sampling of wi
        /// </summary>
        /// <param name="wo">wo in local</param>
        /// <returns>(f, wi, pdf)</returns>
        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
            var wi = Samplers.CosineSampleHemisphere();
            if (wo.z < 0)
                wi.z *= -1;
            double pdf = Pdf(wo, wi);
            return (f(wo, wi), wi, pdf);
        }

        /// <summary>
        /// returns pdf(wo,wi) as |cosTheta|/pi
        /// </summary>
        /// <param name="wo">output vector in local</param>
        /// <param name="wi">input vector in local</param>
        /// <returns></returns>
        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            if (!Utils.SameHemisphere(wo, wi))
                return 0;

            return Math.Abs(wi.z) * Utils.PiInv; // wi.z == cosTheta
        }
    }
}
