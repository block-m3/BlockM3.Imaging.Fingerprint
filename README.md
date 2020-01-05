# BlockM3.Imaging.Fingerprint
Image Fingerprint/Watermarking Library

Using wavelets and discreet cosine transformation and reed solomon.

This library first use haar wavelet transformation. and then use the middle frequency coefficients in DCT to store the fingerprint with reed solomon.
You can chose which wavelet sub band to store the fingerprint.

See the Test Project, of how to use it...

Main methods are

**Fingerprint.Insert**

and

**Fingerprint.Extract**



