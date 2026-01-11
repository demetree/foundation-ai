using System;

namespace Foundation
{
    // The purpose of this is to provide a string iterator interface similar to the C++ string iterator
    public class StringIterator
    {
        private string data;

        public class Iterator : IEquatable<Iterator>
        {
            private StringIterator si;
            private int index;

            public Iterator(StringIterator si)
            {
                this.si = si;
            }

            public Iterator(Iterator ie)
            {
                //
                // Copy the provided iterator element
                //
                this.si = ie.si;
                this.index = ie.index;
            }

            public char Value
            {
                get
                {

                    // end iterator will have the index of the length of the string.
                    if (index < si.data.Length)
                    {
                        // regular data
                        return si.data[index];
                    }
                    else
                    {
                        // end position
                        return (char)0x00;
                    }
                }
            }

            public Iterator(StringIterator si, int index)
            {
                if (si.data == null)
                {
                    throw new Exception("Invalid data");
                }

                this.si = si;

                if (index < 0 || index > si.data.Length)
                {
                    throw new Exception("Invalid index");
                }

                this.index = index;
            }

            public override bool Equals(object obj)
            {
                if (obj is Iterator ie)
                {
                    if (this.index == ie.index &&
                        this.si.data == ie.si.data)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return index;
            }

            public bool IsFirst()
            {
                return index <= 0;
            }

            public bool IsLast()
            {
                return index >= (si.data.Length - 1);
            }

            public char Data()
            {
                return si.data[index];
            }

            public Iterator Add(int indexesToAdd)
            {
                //
                // This will return an interator element that is 'indexesToAdd' more than the current index.
                // 
                // check if the addition will make the index out of bounds, and if it will, then return null.  Otherwise, return the new iterator.
                //

                int targetIndex = this.index + indexesToAdd;

                // allow the index to go one past the actual data to mark the end position.
                if (targetIndex < 0 || targetIndex > si.data.Length)
                {
                    // the value to add would make the index out of bounds.  Return a null.
                    return null;
                }
                else
                {
                    return new Iterator(si, targetIndex);
                }
            }

            public bool CompareNextCharacters(string nextCharactersToLookFor)
            {
                if (nextCharactersToLookFor == null)
                {
                    return false;
                }

                int endIndex = nextCharactersToLookFor.Length + index;

                if (endIndex >= si.data.Length)
                {
                    return false;
                }
                else
                {
                    string nextCharacters = this.si.data.Substring(index, nextCharactersToLookFor.Length);

                    if (nextCharacters == nextCharactersToLookFor)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public void Next()
            {
                index++;
            }

            public void Previous()
            {
                index--;
            }

            public void Jump(int numberToJump)
            {
                if ((index + numberToJump) < 0 || (index + numberToJump) >= si.data.Length)
                {
                    throw new Exception("Number of characters to jump will overrun the source string length.");
                }
                index += numberToJump;
            }

            public bool IsLessThan(Iterator end)
            {
                if (this.si.Equals(end.si) == false)
                {
                    throw new Exception("Iterators are not compatible.");
                }

                if (this.index < end.index)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool IsLessThanOrEqualTo(Iterator end)
            {
                if (this.si.Equals(end.si) == false)
                {
                    throw new Exception("Iterators are not compatible.");
                }

                if (this.index <= end.index)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool IsGreaterThan(Iterator end)
            {
                if (this.si.Equals(end.si) == false)
                {
                    throw new Exception("Iterators are not compatible.");
                }

                if (this.index > end.index)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool IsGreaterThanOrEqualTo(Iterator end)
            {
                if (this.si.Equals(end.si) == false)
                {
                    throw new Exception("Iterators are not compatible.");
                }

                if (this.index >= end.index)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            public void Assign(Iterator it)
            {
                if (it != null)
                {
                    this.index = it.index;
                    this.si = it.si;
                }
            }

            public bool Equals(Iterator other)
            {
                if (this.index == other.index &&
                    this.si.data == other.si.data)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public StringIterator(string str)
        {
            data = str;
        }

        public Iterator Begin()
        {
            return new Iterator(this, 0);
        }

        public Iterator End()
        {
            // The end iterator is one past the last character, to be consistent with the C++ implementation that this is a copy of.
            return new Iterator(this, data.Length);
        }
    }
}
