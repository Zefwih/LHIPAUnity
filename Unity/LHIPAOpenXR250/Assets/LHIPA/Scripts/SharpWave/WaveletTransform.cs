///
/// SharpWave - A refactored port of JWave
/// https://github.com/graetz23/JWave
///
/// MIT License
///
/// Copyright (c) 2020-2024 Christian (graetz23@gmail.com)
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
///
using System;

namespace SharpWave
{

  ///<summary>Base class for Wavelet Transforms.</summary>
  ///<remarks>Christian (graetz23@gmail.com) 15.02.2014 21:05:33</remarks>
  public abstract class WaveletTransform : Algorithm {

    ///<summary>
    /// The used Eavelet object presenting 1-D forward and reverse transform.
    ///</summary>
    ///<remarks>Christian (graetz23@gmail.com) 15.02.2014 21:05:33</remarks>
    protected Wavelet _wavelet;

     ///<summary>
     /// Constructor checks whether the given Wavelet object is all right.
     ///</summary>
     ///<remarks>Christian (graetz23@gmail.com) 15.02.2014 21:05:33</remarks>
    protected WaveletTransform( String name, Wavelet wavelet ) : base( name ) {
      if( wavelet == null )
        throw new Types.Types_NotExistent( "WaveletTransform - " +
          "given Wavelet object is null!" );
      if( !(wavelet is Wavelet) )
        throw new Types.Types_NotPossible( "WaveletTransform - " +
          "given object is not of type Wavelet!" );
      _wavelet = wavelet;
    } // method

    ///<summary>Getter that returns the stored Wavelet object.</summary>
    ///<remarks>Christian (graetz23@gmail.com) 14.03.2015 18:27:05</remarks>
    ///<returns>Returns the stored Wavelet object.</returns>
    public new Wavelet WAVELET { get { return _wavelet; } } // method

    ///<summary>
    /// Performs a 1-D forward transform from time domain to Hilbert domain
    /// using one kind of wavelet transform algorithm for a given array of
    // dimension (length) 2^p | pEN; N = 2, 4, 8, 16, 32, 64, 128, .., so on.
    ///</summary>
    ///<remarks>Christian (graetz23@gmail.com) 10.02.2010 08:23:24</remarks>
    ///<returns>Coefficients of 1-D frequency or Hilbert space.</returns>
    override public double[ ] forward( double[ ] arrTime ) {
      if( !isBinary( arrTime.Length ) )
        throw new Types.Data_NotValid( "WaveletTransform.forward - " +
          "array length is not 2^p | p E N ... = 1, 2, 4, 8, 16, 32, .. " +
          "use the Ancient Egyptian Decomposition for odd array length!" );
      int maxLevel = calcExponent( arrTime.Length );
      return forward( arrTime, maxLevel ); // forward by maximal steps
    } // forward

    ///<summary>
    /// Performs a 1-D reverse transform from Hilbert domain to time domain
    /// using one kind of wavelet transform algorithm for a given array of
    /// dimension (length) 2^p | pEN; N = 2, 4, 8, 16, 32, 64, 128, .., so on.
    /// </summary>
    ///<remarks>Christian (graetz23@gmail.com) 10.02.2010 08:23:24</remarks>
    ///<returns>
    /// Coefficients of time series of 1-D frequency or Hilbert space.
    ///</returns>
    override public double[ ] reverse( double[ ] arrHilb ) {
      if( !isBinary( arrHilb.Length ) )
        throw new Types.Data_NotValid( "WaveletTransform#reverse - " +
        "array length is not 2^p | p E N ... = 1, 2, 4, 8, 16, 32, .. " +
        "use the Ancient Egyptian Decomposition for any other array length!" );
      int maxLevel = calcExponent( arrHilb.Length );
      return reverse( arrHilb, maxLevel ); // reverse by maximal steps
    } // reverse

    /**
     * Performs several 1-D forward transforms from time domain to all possible
     * Hilbert domains using one kind of wavelet transform algorithm for a given
     * array of dimension (length) 2^p | pEN; N = 2, 4, 8, 16, 32, 64, 128, ..,
     * and so on. However, the algorithm stores all levels in a matrix that has in
     * first dimension the range of 0, .., p and in second dimension the
     * coefficients (energy & details) of a certain level. From any level a full
     * reconstruction can be performed. The first dimension is keeping the time
     * series, due to being the Hilbert space of level 0. All following dimensions
     * are keeping the next higher Hilbert spaces, so the next step in wavelet
     * filtering.
     *
     * This method was updated to be used with Unity and the LHIPA algorithm.
     *
     * @author Christian (graetz23@gmail.com)
     * @date 22.03.2015 14:28:49
     * @param arrTime
     *          coefficients of time domain
     * @return matDeComp coefficients of frequency or Hilbert domain in 2-D
     *         spaces: [ 0 .. p ][ 0 .. M ] where p is the exponent of M=2^p | pEN
     * @throws JWaveException
     *           if something does not match upon the criteria of input
     * @see jwave.transforms.BasicTransform#decompose(double[])
     */
    public double[][] decompose(double[] arrTime)
    {
      int length = arrTime.Length;
      int levels = calcExponent(length);
      double[][] matDeComp = new double[levels + 1][];

      for (int p = 0; p <= levels; p++)
      {
        matDeComp[p] = new double[length];
        Array.Copy(forward(arrTime, p), 0, matDeComp[p], 0, length);
      }
      return matDeComp;
    }
// decompose

    /**
     * Performs one 1-D reverse transform from Hilbert domain to time domain using
     * one kind of wavelet transform algorithm for a given array of dimension
     * (length) 2^p | pEN; N = 2, 4, 8, 16, 32, 64, 128, .., and so on. However,
     * the algorithm uses on of level in a matrix that has in first dimension the
     * range of 0, .., p and in second dimension the coefficients (energy &
     * details) the level. From any level a full a reconstruction can be
     * performed; so from the selected by "level". Anyway, the first dimension is
     * keeping the time series, due to being the Hilbert space of level 0. All
     * following dimensions are keeping the next higher Hilbert spaces, so the
     * next step in wavelet filtering. If one want to denoise each level in the
     * same way and compare results after reverse transform, this is the best
     * input for it.
     *
     * @author Christian (graetz23@gmail.com)
     * @date 22.03.2015 14:29:01
     * @see jwave.transforms.BasicTransform#recompose(double[][], int)
     * @param matDeComp
     *          2-D Hilbert spaces: [ 0 .. p ][ 0 .. M ] where p is the exponent
     *          of M=2^p | pEN
     * @throws JWaveException
     *           if something does not match upon the criteria of input
     * @return a 1-D time domain signal
     * @see jwave.transforms.BasicTransform#recompose(double[])
     */
    // public double[ ] recompose( double[ ][ ] matDeComp, int level ) {
    //
    //   if( level < 0 || level >= matDeComp.length )
    //     throw new Types.Data_NotValid( "WaveletTransform#recompose - "
    //         + "given level is out of range" );
    //
    //   return reverse( matDeComp[ level ], level );
    //
    // } // recompose

  } // WaveletTransform

} // namespace
