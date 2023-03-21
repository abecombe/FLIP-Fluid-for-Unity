# FLIP-Fluid for Unity

Real-time particle-based 3D fluid simulation and rendering in Unity HDRP with GPU (Compute Shader) & VFXGraph.

<img src="https://user-images.githubusercontent.com/55338725/226588264-e6921f63-cc40-48c5-b628-79e599459cac.png" width="640px">

## FLIP (Fluid Implicit Particle) Fluid Simulation
[Cataclysm: a FLIP Solver with GPU Particles](https://developer.nvidia.com/cataclysm-flip-solver-gpu-particles)
* PIC / FLIP Combination (Flipness)
* Staggered MAC Grid
* Particle <-> Grid Transferring
    * Linear Kernel
    * Quadratic Kernel
* Mouse Interaction
* Viscous Diffusion
* Pressure Projection
* Particle Advection
    * Forward Euler
    * 2nd order Runge-Kutta
    * 3rd order Runge-Kutta
* Density Projection for Volume Conserving
* Interactive Camera
* Runtime UI
<img src="https://user-images.githubusercontent.com/55338725/226634933-0736adc3-1383-4427-80ad-6d884fa1d882.png" width="320px">

## Movie
<img src="https://user-images.githubusercontent.com/55338725/226597691-2cee823e-4592-4849-9feb-ab47eed611d3.gif" width="600px">

## References
* [Fluid Simulation for Computer Graphics, Second Edition](https://www.routledge.com/Fluid-Simulation-for-Computer-Graphics/Bridson/p/book/9781482232837#)
* [Implicit Density Projection for Volume Conserving Liquids](https://animation.rwth-aachen.de/media/papers/66/2019-TVCG-ImplicitDensityProjection.pdf)
* [fluid](https://github.com/dli/fluid)
* [List of Runge-Kutta methods](https://en.wikipedia.org/wiki/List_of_Runge-Kutta_methods)