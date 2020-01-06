# BlockM3.Imaging.Fingerprint
Image Fingerprint/Watermarking Library

Using wavelets, discreet cosine transformation and reed solomon.

This library convert the image to YCbCr then with the Cb channel, applies haar wavelet transformation, selects a subband and applies 4x4 Discreet Cosine Transformation, and then use the middle frequency coefficients in DCT to store the fingerprint with reed solomon, after that it inverses the process and write back the image.

You can chose which wavelet sub band to store the fingerprint.

DCT and HAAR are optimized for mobile use.

See the Test Project, of how to use it...

Main methods are

**Fingerprint.Insert**

and

**Fingerprint.Extract**


this library uses [SkiaSharp](https://github.com/mono/SkiaSharp) nuget for image I/O.

Part of this libraries uses:

[Accord.Net DCT & Haar Wavelets algorithms](https://github.com/accord-net/framework) modified and optimized for this library.

[Reed Solomon Algorithms from ZXING](https://github.com/micjahn/ZXing.Net).

