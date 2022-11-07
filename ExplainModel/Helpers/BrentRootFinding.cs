namespace Explain.Helpers;
public class BrentRootFinding
{
     public BrentResult Brent(Func<double, double> func, double lowerLimit, double upperLimit, double maxIter, double accuracy)
    {
      var a = lowerLimit;
      var b = upperLimit;
      var c = a;
      var fa = func(a);
      var fb = func(b);
      var fc = fa;

      // define a result object
    var result = new BrentResult();
    var setMaxIterations = maxIter;
    
    while (maxIter-- > 0) {
      var prevStep = b - a;

      if (Math.Abs(fc) < Math.Abs(fb)) {
        // Swap data for b to be the best approximation
        a = b;
        b = c;
        c = a;
        fa = fb;
        fb = fc; 
        fc = fa;
      }

      var tolAct = 1e-15 * Math.Abs(b) + accuracy / 2;
      var newStep = (c - b) / 2;

      if (Math.Abs(newStep) <= tolAct || fb == 0) {
        result.Result = b;
        result.Error = false;
        result.Iterations = setMaxIterations - maxIter;
        return result; // Acceptable approx. is found
      }
      // Decide if the interpolation can be tried
      if (Math.Abs(prevStep) >= tolAct && Math.Abs(fa) > Math.Abs(fb)) {
        // If prev_step was large enough and was in true direction, Interpolatiom may be tried
        var cb = c - b;
        double p = 0;
        double q = 0;
        double t1 = 0;
        double t2 = 0;
        if (a.Equals(c)) {
          // If we have only two distinct points linear interpolation can only be applied
          t1 = fb / fa;
          p = cb * t1;
          q = 1.0 - t1;
        } else {
          // Quadric inverse interpolation
          q = fa / fc;
          t1 = fb / fc;
          t2 = fb / fa;
          p = t2 * (cb * q * (q - t1) - (b - a) * (t1 - 1));
          q = (q - 1) * (t1 - 1) * (t2 - 1);
        }

        if (p > 0) {
          q = -q; // p was calculated with the opposite sign; make p positive
        } else {
          p = -p; // and assign possible minus to q
        }

        if ( p < 0.75 * cb * q - Math.Abs(tolAct * q) / 2 && p < Math.Abs((prevStep * q) / 2)) 
        {
          // If (b + p / q) falls in [b,c] and isn't too large it is accepted
          newStep = p / q;
        }

        // If p/q is too large then the bissection procedure can reduce [b,c] range to more extent
      }

      if (Math.Abs(newStep) < tolAct) {
        // Adjust the step to be not less than tolerance
        newStep = newStep > 0 ? tolAct : -tolAct;
      }

      // Save the previous approx.
      a = b;
      fa = fb;
      b += newStep;
      fb = func(b); // Do step to a new approxim.

      if ((fb > 0 && fc > 0) || (fb < 0 && fc < 0))
      {
        c = a;
        fc = fa; // Adjust c for it to have a sign opposite to that of b
      }
    }

    // configure the return object if not within range
    result.Result = -1;
    result.Error = true;
    result.Iterations = setMaxIterations - maxIter;
    return result; // No acceptable approximation. is found
    }
     public struct BrentResult
     {
       public double Result;
       public double Iterations;
       public bool Error;
     }
}
