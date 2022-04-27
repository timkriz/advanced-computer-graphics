using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathTracer.Samplers;

namespace PathTracer
{
    class PathTracer
    {
        /// <summary>
        /// Given Ray r and Scene s, trace the ray over the scene and return the estimated radiance
        /// </summary>
        /// <param name="r">Ray direction</param>
        /// <param name="s">Scene to trace</param>
        /// <returns>Estimated radiance in the ray direction</returns>
        public Spectrum Li(Ray r, Scene s)
        {
            var L = Spectrum.ZeroSpectrum;
            /* Implement */

            Spectrum beta = Spectrum.Create(1);
            int nBounces = 0;
            int maxNBounces = 10;
            

            while (nBounces < maxNBounces) {

                (double?, SurfaceInteraction) intersectResult = s.Intersect(r);
                double? distance = intersectResult.Item1;
                SurfaceInteraction intersection = intersectResult.Item2;

                // 1. Ray does not intersect with scene
                if (!distance.HasValue)
                {
                    break;
                }

                Vector3 wOut = -r.d;

                // 2. Ray intersects with light
                if (intersection.Obj is Light) {
                    L = nBounces == 0 ? beta * intersection.Le(wOut) : L; // I1. Path reuse
                    break;
                }
                // I1. Path reuse
                Spectrum Ld = Light.UniformSampleOneLight(intersection, s);
                L = L.AddTo(beta * Ld);


                // 3. Ray intersects with object in the scene but not light
                Shape shape = intersection.Obj as Shape;
                (Spectrum f, Vector3 wi, double probabilty, _) = shape.BSDF.Sample_f(wOut, intersection);

                double cosineOfAngle = Vector3.AbsDot(wi, intersection.Normal);
                beta = beta * f * cosineOfAngle / probabilty;

                Ray wIn = intersection.SpawnRay(wi);
                r = wIn;

                // I2. Russian roulette

                if (nBounces > 3) {
                    double q = 1 - beta.Max();
                    if (ThreadSafeRandom.NextDouble() < q) {
                        break; // In q cases, stop integration
                    }
                    beta = beta / (1 - q); // Correction for q skipped samples
                }

                nBounces++;
            }

            return L;
        }

        public Light getHitLight(Ray r, List<Light> lights) {
            Light hitLight = null;
            foreach (Light light in lights) {
                if (light.Intersect(r).Item1.HasValue) {
                    return light;
                }
            }
            return hitLight;
        }

        public Dictionary<Primitive, SurfaceInteraction> getSceneIntersections(Ray r, Scene s) {
            Dictionary < Primitive, SurfaceInteraction > returnSceneIntersections = new Dictionary<Primitive, SurfaceInteraction >();

            foreach (Primitive primitive in s.Elements)
            {
                (double?, SurfaceInteraction) intersection = primitive.Intersect(r);

                if (intersection.Item1 != null)
                {
                    returnSceneIntersections.Add(primitive, intersection.Item2);
                }

            }
            return returnSceneIntersections;
        }  
        public void printVector(Vector3 vector) {
            Console.WriteLine("[" + vector.x.ToString() + " " + vector.y.ToString() + " " + vector.z.ToString() + "]");
        }

        public void printScene(Scene s) {
            //Console.WriteLine(s.Elements.Count);
            foreach (Primitive primitive in s.Elements) {
                if (primitive is Shape) {
                    
                }
                Console.WriteLine(primitive.GetType());
            }
        }

    }
}
