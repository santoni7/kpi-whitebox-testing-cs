using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using IIG.BinaryFlag;
using System.Collections.Generic;

namespace BinaryFlag.Test
{
	[TestClass]
	public class MultipleBinaryFlagTest
	{
		const ulong MAX_ALLOWED_SIZE = 17179868704;

		[TestMethod]
		public void Constructor_Throws_ArgumentOutOfRange_LengthLessThan2()
		{
			Action test_length_1 = () => { new MultipleBinaryFlag(1); };
			Action test_length_0 = () => { new MultipleBinaryFlag(0); };
			Assert.ThrowsException<ArgumentOutOfRangeException>(test_length_1);
			Assert.ThrowsException<ArgumentOutOfRangeException>(test_length_0);
		}


		[TestMethod]
		public void Constructor_Throws_ArgumentOutOfRange_LengthMoreThanMax()
		{
			Action test_length_out_of_range = () => { new MultipleBinaryFlag(MAX_ALLOWED_SIZE+1); };
			Assert.ThrowsException<ArgumentOutOfRangeException>(test_length_out_of_range);
		}

		[TestMethod]
		public void Test_UIntConcreteBinaryFlag_FullFlow()
		{
			// UIntConcreteBinaryFlag implementation is used when 2<=length<=32. 
			// We'll test lowest and highest length from this range
			var sizes_to_test = new List<ulong>() { 2, 32 };
			sizes_to_test.ForEach((len) => {
				TestFullFlow(len, initialValue: true);
				TestFullFlow(len, initialValue: false);
			});
		}

		[TestMethod]
		public void Test_ULongConcreteBinaryFlag_FullFlow()
		{
			// UIntConcreteBinaryFlag implementation is used when 33<=length<=64. 
			// We'll test lowest and highest length from this range
			var sizes_to_test = new List<ulong>() { 33, 64 };
			sizes_to_test.ForEach((len) => {
				TestFullFlow(len, initialValue: true);
				TestFullFlow(len, initialValue: false);
			});
		}

		[TestMethod]
		public void Test_UIntArrayConcreteBinaryFlag_FullFlow()
		{
			// UIntConcreteBinaryFlag implementation is used when length > 64. 
			var sizes_to_test = new List<ulong>() { 65, 4096 };
			sizes_to_test.ForEach((len) => {
				TestFullFlow(len, initialValue: true);
				TestFullFlow(len, initialValue: false);
			});
		}


		[TestMethod]
		public void Test_UIntConcreteBinaryFlag_ThrowsException_AfterDispose()
		{
			ulong length = 32;
			TestAfterDispose(length, true);
			TestAfterDispose(length, false);
		}

		[TestMethod]
		public void Test_ULongConcreteBinaryFlag_ThrowsException_AfterDispose()
		{
			ulong length = 33;
			TestAfterDispose(length, true);
			TestAfterDispose(length, false);
		}

		[TestMethod]
		public void Test_UIntArrayConcreteBinaryFlag_ThrowsException_AfterDispose()
		{
			ulong length = 65;
			TestAfterDispose(length, true);
			TestAfterDispose(length, false);
		}

		/// <summary>
		/// Runs full test of MultipleBinaryFlag given specified length and initialValue.
		/// Consists of such steps:
		/// 1. Test that new object is setting up initial value correctly by checking GetFlag() and ToString() method's return values.
		/// 2. Test ResetFlag() works as intended by resetting flag at each position of 0..length-1. Checks expected output of GetFlag() and ToString() on each step.
		/// 3. Test SetFlag() similar to p.2 (ResetFlag())
		/// 4. Test ArgumentOutOfRangeException is being thrown when calling ResetFlag() or SetFlag() with position>=length
		/// </summary>
		/// <param name="len"></param>
		/// <param name="initialValue"></param>
		private static void TestFullFlow(ulong length, bool initialValue)
		{
			MultipleBinaryFlag impl = new MultipleBinaryFlag(length, initialValue);

			// 1. Test that new object is setting up initial value correctly.
			char expected_char = initialValue ? 'T' : 'F';
			string expected = new string(expected_char, (int)length);
			char[] expected_arr = expected.ToCharArray();
			Assert.AreEqual(initialValue, impl.GetFlag()); // GetFlag should be equal to initialValue right after object construction

			var actual = impl.ToString();
			Assert.AreEqual<string>(expected, actual, $"Expected string of '{expected_char}' chars of {length} symbols. Got '{actual}' instead");

			// 2. Test ResetFlag(): apply reset for each element by one, compare output (ToString) to expected. 
			// Also checks if GetFlag() returns false (since not all elements are True)
			for (ulong i = 0; i < length; ++i)
			{
				impl.ResetFlag(i);
				expected_arr[i] = 'F';
				expected = new string(expected_arr);

				actual = impl.ToString();
				Assert.AreEqual<string>(expected, actual);
				Assert.IsFalse(impl.GetFlag());
			}

			// 3. Test SetFlag(): set flag for each position by one, compare output (ToString) to expected. 
			// Also checks if GetFlag() returns false (since not all elements are True)
			for (ulong i = 0; i < length; ++i)
			{
				impl.SetFlag(i);
				expected_arr[i] = 'T';
				expected = new string(expected_arr);

				actual = impl.ToString();
				Assert.AreEqual<string>(expected, actual);
				// After last step (i==len-1) flag should become true.
				// Before that it should remain false, as not all flags are true.
				if (i < length - 1)
					Assert.IsFalse(impl.GetFlag());
				else Assert.IsTrue(impl.GetFlag());
			}

			// 4. Test ArgumentOutOfRangeException is being thrown when calling ResetFlag/SetFlag with position>=length
			Action throws_outOfRange_resetFlag = () => { impl.ResetFlag(length); };
			Action throws_outOfRange_setFlag = () => { impl.SetFlag(length); };
			Assert.ThrowsException<ArgumentOutOfRangeException>(throws_outOfRange_setFlag);
			Assert.ThrowsException<ArgumentOutOfRangeException>(throws_outOfRange_resetFlag);

			impl.Dispose();
		}

		private static void TestAfterDispose(ulong length, bool initialValue)
		{
			MultipleBinaryFlag impl = new MultipleBinaryFlag(length, initialValue);
			impl.Dispose();
			try
			{
				impl.GetFlag();
			} catch(Exception e)
			{
				// This is expected.
				return;
			}
			Assert.Fail("Expected exception to be raised when accessing MultipleBinaryFlag after dispose. Got nothing instead");
		}
	}
}
