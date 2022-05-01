# advanced-computer-graphics
Homeworks for Advanced computer graphics, FRI 2022.

## 1st homework: Scattered data interpolation
Inverse distance weighting (IDW) with its basic form, the Shepard’s method and Modified Shepard’s method with limited radius of influence of each data point. ***Interpolate.exe*** reads standard input data (*Console.ReadLine*) and outputs floats to binary file ***output.raw***. There is also an option to output byte binary file for visualization of the volume.

Compile c# with:
```console
csc *.cs
```
Run interpolate.exe with ***basic*** method (command prompt, not powershell):
```console
./interpolate < input1k.txt --method basic --p 2 --min-x -1.5 --min-y -1.5 --min-z -1  --max-x 1.5 --max-y 1.5 --max-z 1  --res-x 128 --res-y 128 --res-z 64
```
or ***modified*** method:
```console
./interpolate < input1k.txt --method modified --r 0.5 --min-x -1.5 --min-y -1.5 --min-z -1  --max-x 1.5 --max-y 1.5 --max-z 1  --res-x 128 --res-y 128 --res-z 64
```
## 2nd homework: Path tracing
Physically based rendering path tracing framework with basic path tracing algorithm with  Russian roulette for stopping and importance sampling of choosing ray directions, support for spherical light sources and Oren-Nayar material.

<a href="url"><img src="https://github.com/timkriz/advanced-computer-graphics/blob/master/nrg_dn2/output_images/01_path_tracer.PNG" align="middle" width="300" ></a>
