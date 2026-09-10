# Heightify

A fast C# desktop utility for heightmap generation, normal reconstruction, and raster image analysis, built for computer graphics and terrain pipelines.

---

## Why it's fast (Under the hood)

* **Raw Pointer Arithmetic:** Bypasses slow GDI+ `GetPixel`/`SetPixel` in favor of `LockBits` and `unsafe` blocks. Processes 4K textures up to **98% faster**.
* **OpenCV Integration:** Replaced nested loops with SIMD-accelerated vector operations via OpenCvSharp4 for heavy lifting (`Cv2.Magnitude`, `Cv2.Normalize`).
* **Strict Memory Management:** Zero-leak native interop. All OpenCV matrices (`Mat`) and unmanaged GDI+ resources are strictly wrapped in `IDisposable` patterns to keep the memory footprint light.
* **Decoupled Architecture:** Heavy computation runs entirely separate from the Windows Forms UI thread, ensuring the app remains responsive during 4K rendering.

---

## Core Features

| Module | Description |
| :--- | :--- |
| **Transfer Function Mapping** | Apply non-linear height curves (logarithmic, polynomial, exponential) with highly optimized inner loops. |
| **Normal to Height** | Derives surface displacement directly from 3-axis tangent-space normal maps. |
| **Channel Mixing** | Fast BGR channel separation and dynamic range normalization into single-channel grayscale height maps. |
| **Edge Detection Pipeline** | Extracts contours using 3x3 Gaussian blur and dual-axis Sobel operators. |

---

## Tech Stack

* **Platform:** .NET Framework / Windows Forms (C# Unsafe context)
* **Libraries:** 
  * [OpenCvSharp4](https://github.com/shimat/opencvsharp) (Computer Vision & image convolutions)
  * [Guna.UI2](https://gunai.io/) (Windows Forms interface controls)

---

## Getting Started

### Prerequisites
* Visual Studio 2022 (v17.0+) with **.NET desktop development** workload.
* NuGet Package Manager.

### Building
1. Clone the repository:
   ```bash
   git clone https://github.com/bohdandash/ProjectHeightify.git

### Preview
1. Main Screen:
   ![image alt](https://github.com/bohdandash/Heightify/blob/30c762a6a10a366b999ecda0dc88ba8ae845682e/images/Main%20Screen.png)

2. Sobel - Gauss Method:
   ![image alt](https://github.com/bohdandash/Heightify/blob/30c762a6a10a366b999ecda0dc88ba8ae845682e/images/Sobel-Gauss.png)

3. Image - Height Method:
   ![image alt](https://github.com/bohdandash/Heightify/blob/30c762a6a10a366b999ecda0dc88ba8ae845682e/images/ImageToHeight.png)

4. Color - Channel Method:
   ![image alt](https://github.com/bohdandash/Heightify/blob/30c762a6a10a366b999ecda0dc88ba8ae845682e/images/RGB.png)

5. Pixel - Intensity Method:
   ![image alt](https://github.com/bohdandash/Heightify/blob/30c762a6a10a366b999ecda0dc88ba8ae845682e/images/MathFunc.png)
