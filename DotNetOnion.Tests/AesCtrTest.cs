using NUnit.Framework;
using DotNetOnion.Crypto;
using DotNetOnion.Helpers;

namespace DotNetOnion.Tests
{

    public class AesCtrTest
    {
        [SetUp]
        public void Setup()
        {
        }

        /*
         * Test vectors from NIST Special Pub 800-38A
         */

        [Test]
        public void AesCtrEnecryptionTest()
        {
            byte[] key =
                HexHelpers.HexToByteArray("2b7e151628aed2a6abf7158809cf4f3c");
            byte[] iv =
                HexHelpers.HexToByteArray("f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff");

            TorStreamCipher cipher = new(key, iv);

            byte[] plainText1 =
                HexHelpers.HexToByteArray("6bc1bee22e409f96e93d7e117393172a");
            byte[] expectedCipherText1 =
                HexHelpers.HexToByteArray("874d6191b620e3261bef6864990db6ce");
            byte[] computedCipherText1 =
                cipher.Encrypt(plainText1);

            CollectionAssert.AreEqual(computedCipherText1, expectedCipherText1);

            byte[] plainText2 =
                HexHelpers.HexToByteArray("ae2d8a571e03ac9c9eb76fac45af8e51");
            byte[] expectedCipherText2 =
                HexHelpers.HexToByteArray("9806f66b7970fdff8617187bb9fffdff");
            byte[] computedCipherText2 =
                cipher.Encrypt(plainText2);

            CollectionAssert.AreEqual(computedCipherText2, expectedCipherText2);

            byte[] plainText3 =
                HexHelpers.HexToByteArray("30c81c46a35ce411e5fbc1191a0a52ef");
            byte[] expectedCipherText3 =
                HexHelpers.HexToByteArray("5ae4df3edbd5d35e5b4f09020db03eab");
            byte[] computedCipherText3 =
                cipher.Encrypt(plainText3);

            CollectionAssert.AreEqual(computedCipherText3, expectedCipherText3);

            byte[] plainText4 =
                HexHelpers.HexToByteArray("f69f2445df4f9b17ad2b417be66c3710");
            byte[] expectedCipherText4 =
                HexHelpers.HexToByteArray("1e031dda2fbe03d1792170a0f3009cee");
            byte[] computedCipherText4 =
                cipher.Encrypt(plainText4);

            CollectionAssert.AreEqual(computedCipherText4, expectedCipherText4);
        }
    }
}