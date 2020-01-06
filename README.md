# BlockM3.Imaging.Fingerprint
Image Fingerprint/Watermarking Library

Using wavelets, discreet cosine transformation and reed solomon.

This library first use haar wavelet transformation. and then use the middle frequency coefficients in DCT to store the fingerprint with reed solomon.
You can chose which wavelet sub band to store the fingerprint.

DCT and HAAR are optimized for mobile use.

See the Test Project, of how to use it...

Main methods are

**Fingerprint.Insert**

and

**Fingerprint.Extract**


this library uses SkiaSharp for image I/O.

Part of this libraries uses:

[Accord.Net DCT & Haar Wavelets algorithms](https://github.com/accord-net/framework) modified and optimized for this library.

[Reed Solomon Algorithms from ZXING](https://github.com/micjahn/ZXing.Net).

