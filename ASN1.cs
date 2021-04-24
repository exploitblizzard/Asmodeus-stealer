using System;
using System.Collections.Generic;

namespace UploadAFile
{
    class ASN1
    {
        enum ASN1Types
        {
            SEQUENCE = 0x30,
            OCTETSTRING = 4,
            OBJECTIDENTIFIER = 6,
            INTEGER = 2,
            NULL = 5
        }
        public Sequence RootSequence { get; set; }
        public byte[] Asn1ByteArray { get; set; }
        bool finished = false;

        public ASN1(byte[] asn1Data)
        {
            Asn1ByteArray = asn1Data;
            RootSequence = new Sequence();
            ParseTLV(RootSequence, 0);
        }

        public void ParseTLV(Sequence sequenceCurrent, int index)
        {
            //store the type value 
            int i = index;
            int type = (int)Asn1ByteArray[i];
            //and increment the index to the length field
            i += 1;

            //Check whether the length field is in short or long form then get length of the value
            int lengthForm = CheckLenthForm(Asn1ByteArray[i]); //Actually the number of octets used to represent the Length field
            int length = GetLength(i, lengthForm);

            //Increment the index to the value field
            i += lengthForm;

            //Check the type value and store each TLV's contents in the relevant sequence object`
            if (type == (int)ASN1Types.SEQUENCE)
            {
                //Create a temporary index value for the next recursive function call into this sequence
                int tempIndex = i;
                Sequence sequenceNew = new Sequence();
                sequenceCurrent.Sequences.Add(sequenceNew);

                ParseTLV(sequenceNew, i);
                
                //Reset index to correct position after each recursion
                i += tempIndex + length;
            }
            else if (type == (int)ASN1Types.OBJECTIDENTIFIER)
            {
                byte[] tmpArray = new byte[length];

                Buffer.BlockCopy(Asn1ByteArray, i, tmpArray, 0, length);

                sequenceCurrent.ObjectIdentifiers.Add(tmpArray);

                i += length;
            }
            else if (type == (int)ASN1Types.OCTETSTRING)
            {
                byte[] tmpArray = new byte[length];

                Buffer.BlockCopy(Asn1ByteArray, i, tmpArray, 0, length);

                sequenceCurrent.OctetStrings.Add(tmpArray);

                i += length;
            }
            else if (type == (int)ASN1Types.INTEGER)
            {
                byte[] tmpArray = new byte[length];

                Buffer.BlockCopy(Asn1ByteArray, i, tmpArray, 0, length);

                sequenceCurrent.Integers.Add(tmpArray);

                i += 1;
            }
            else if (type == (int)ASN1Types.NULL)
            {
                //'Contents' octets are empty. 
                //If BER encoded and length octets are in long form, increment 1 additional byte to next TLV. If DER encoded, index is already at next TLV
                if (lengthForm > 1)
                {
                    i += 1;
                }
            }
            else
            {
                //Some other type not accounted for in the ASN1Types enum
                i += length;
            }

            //Checked every type, meaning there was more than one element in this sequence. Move to next element
            //But first, check that we haven't hit the end of the ASN encoded data
            if (i < Asn1ByteArray.Length && finished == false)
            {
                ParseTLV(sequenceCurrent, i);
            }
            else
            {
                finished = true;
            }
        }

        public int CheckLenthForm(byte length)
        {
            if ((length & 0x80) > 0) //Bit 8 of first octet has value 1 and bits 7-1 give number of additional length octets
            {
                //Long Form
                //Get number of additional length octets
                return (int)(length & 0x7f);
            }
            else
            {
                //Return 1 to indicate that the length field is stored in short form
                //Incrementing the index by the length form value will set the index to the start of the value field
                return 1;
            }
        }
        public int GetLength(int index, int lFormLength)
        {
            byte length = Asn1ByteArray[index];
            int longFormLength = lFormLength - 1;

            /*
            http://luca[.]ntop.org/Teaching/Appunti/asn1.html Chapter 3.1 describtes length octets of TL
            Check if length value is in long or short format
            */
            if (longFormLength > 1) //Set to 1 if length value is in short form
            {

                //Create new bytearray to store long form length value
                byte[] longFormBytes = new byte[longFormLength];

                //Copy length bytes from full byte array for conversion
                for (int i = 0; i < longFormLength + 1; i++)
                {
                    longFormBytes[i] = Asn1ByteArray[index + i];
                }

                try
                {
                    return BitConverter.ToInt32(longFormBytes, 0);
                }
                catch (Exception e)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Exception: {e}");
                    Console.ResetColor();
                    return (int)length;
                }
            }
            else
            {
                //Short Form
                return (int)length;
            }
        }

        public class Sequence
        {
            public List<Sequence> Sequences { get; set; }
            public List<byte[]> Integers { get; set; }
            public List<byte[]> OctetStrings { get; set; }
            public List<byte[]> ObjectIdentifiers { get; set; }

            public Sequence()
            {
                Sequences = new List<Sequence>();
                Integers = new List<byte[]>();
                OctetStrings = new List<byte[]>();
                ObjectIdentifiers = new List<byte[]>();
            }
        }    
    }
}
