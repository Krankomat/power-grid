using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MathUtilTest
    {
        
        [UnityTest]
        public IEnumerator TestSteppedNumber()
        {
            // Because float is so imprecise, Mathf.Approximately() has to be used to check if equal 
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(7.89f, 0.2f), 7.8f), true); 
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(0.23f, 0.2f), 0.2f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(-0.4f, 0.2f), -0.4f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(-22.09f, 0.2f), -22f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(-5.19f, 0.2f), -5.2f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(0.99f, 0.2f), 1f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(35.34f, 0.2f), 35.4f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(4.77f, -0.2f), -4.8f), true);
            Assert.AreEqual(Mathf.Approximately(MathUtil.SteppedNumber(6.38f, -1.99f), -5.97f), true);

            yield return null;
        }
    }
}
