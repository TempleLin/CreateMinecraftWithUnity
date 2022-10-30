When making heights a terrain, if each x and z keeps a y axis value, it'll take up too much memory.

Instead, using a formula can save memory and make every vertex in the terrain be able to find its height through the formula.

For example:

    height(y) = Sin(x) + Cos(z) * 16

Noise:

    Noise is an algorithm for generating wavelets that are seemingly pseudorandom, but mathematically predictable, and used for generating terrains and textures.

Perlin Noise is a good choice of noise algorithm for generating Minecraft terrain.
Giving different increment value to Perlin Noise gives different details/bump level of the terrain. Increment = 0.1 gives a terrain with lots of bumps and details; 0.01 makes it kinda smooth. (But value too low or too high makes it flat.)

Different increment value is like expanding or reducing size of the noise.

Noises with different incrementation, frequency, and amplitude, can be combined to produce a different noise with more randomness, that suits more to create a better terrain.
