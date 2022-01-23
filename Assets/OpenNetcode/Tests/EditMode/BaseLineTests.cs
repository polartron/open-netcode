using System;
using NUnit.Framework;
using OpenNetcode.Shared;
using Unity.Collections;
using Unity.Entities;

namespace OpenNetcode.Tests.EditMode
{
    public class BaseLineTests
    {
        private struct TestData
        {
            public int someData;
            public int otherData;
        }
        
        [Test]
        public void read_write_baseline()
        {
            int baseLine = 42;
            BaseLines<TestData> baseLineData = new BaseLines<TestData>(100, 10);
            NativeArray<TestData> data = new NativeArray<TestData>(10, Allocator.Temp);
            
            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = new TestData()
                    {
                        someData = 10,
                        otherData = 10
                    };
                }

                baseLineData.UpdateBaseline(data, baseLine, data.Length);

                var slice = baseLineData.GetBaseline(baseLine);

                Assert.AreEqual(data.Length, slice.Length);
            
                for (int i = 0; i < slice.Length; i++)
                {
                    Assert.AreEqual(data[i].someData, slice[i].someData);
                    Assert.AreEqual(data[i].otherData, slice[i].otherData);
                }
            }
            finally
            {
                baseLineData.Dispose();
                data.Dispose();
            }
        }
    }
}
