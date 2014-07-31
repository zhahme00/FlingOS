﻿#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Kernel.Debug.Data;

namespace Kernel.Compiler
{
    /// <summary>
    /// Represents an IL scanner's current state (such as current method that is being scanned, Method IDs etc.)
    /// </summary>
    public class ILScannerState
    {
        /// <summary>
        /// The method to use as the kernel's Add Exception Handler Info method.
        /// </summary>
        public MethodBase AddExceptionHandlerInfoMethod;
        /// <summary>
        /// The method to use as the kernel's Handle Leave method.
        /// </summary>
        public MethodBase ExceptionsHandleLeaveMethod;
        /// <summary>
        /// The method to use as the kernel's Handle End Finally method.
        /// </summary>
        public MethodBase ExceptionsHandleEndFinallyMethod;
        /// <summary>
        /// The method to use as the kernel's GC New Obj method.
        /// </summary>
        public MethodBase NewObjMethod;
        /// <summary>
        /// The method to use as the kernel's GC New Arr method.
        /// </summary>
        public MethodBase NewArrMethod;
        /// <summary>
        /// The method to use as the kernel's GC Increment Ref Count method.
        /// </summary>
        public MethodBase IncrementRefCountMethod;
        /// <summary>
        /// The method to use as the kernel's GC Decrement Ref Count method.
        /// </summary>
        public MethodBase DecrementRefCountMethod;
        /// <summary>
        /// The method to use as the kernel's Halt method.
        /// </summary>
        public MethodBase HaltMethod;
        /// <summary>
        /// The method to use to throw a NullReferenceException.
        /// </summary>
        public MethodBase ThrowNullReferenceExceptionMethod;
        /// <summary>
        /// The method to use to throw a ArrayTypeMismatchException.
        /// </summary>
        public MethodBase ThrowArrayTypeMismatchExceptionMethod;
        /// <summary>
        /// The method to use to throw a IndexOutOfRangeException.
        /// </summary>
        public MethodBase ThrowIndexOutOfRangeExceptionMethod;

        /// <summary>
        /// The class to use as the kernel's Type class.
        /// </summary>
        public Type TypeClass;
        /// <summary>
        /// The class to use as the kernel's Array class.
        /// </summary>
        public Type ArrayClass;
        /// <summary>
        /// The class to use as the kernel's String class.
        /// </summary>
        public Type StringClass;

        /// <summary>
        /// Whether to do a debug build or not. Read-only - copied from Settings when IL scanner starts. 
        /// </summary>
        public readonly bool DebugBuild;

        /// <summary>
        /// The ASM chunk that contains the String Literals data (hard-coded string values generated by "AString" in C# code.)
        /// </summary>
        public ASMChunk StringLiteralsDataBlock = new ASMChunk();
        /// <summary>
        /// The ASM chunk that contains the Static Fields data.
        /// </summary>
        public ASMChunk StaticFieldsDataBlock = new ASMChunk();
        /// <summary>
        /// The ASM chunk that contains the Types Table data.
        /// </summary>
        public ASMChunk TypesTableDataBlock = new ASMChunk();
        /// <summary>
        /// The ASM chunk that contains the Methods Table data.
        /// </summary>
        public List<ASMChunk> MethodTablesDataBlock = new List<ASMChunk>();
        
        /// <summary>
        /// The stack frame for the method currently being scanned.
        /// </summary>
        public StackFrame CurrentStackFrame = null;
        /// <summary>
        /// The IL chunk that is currently being scanned.
        /// </summary>
        public ILChunk CurrentILChunk;

        /// <summary>
        /// A dictionary of all the method signatures to method IDs scanned so far.
        /// </summary>
        private Dictionary<string, string> MethodIDs = new Dictionary<string, string>();
        /// <summary>
        /// A dictionary of all the method signatures to method ID values scanned so far.
        /// </summary>
        private Dictionary<string, string> MethodIDValues = new Dictionary<string, string>();
        /// <summary>
        /// A dictionary of all the field signatures to field IDs scanned so far.
        /// </summary>
        private Dictionary<string, string> StaticFieldIDs = new Dictionary<string, string>();
        /// <summary>
        /// A dictionary of all the types to type IDs scanned so far.
        /// </summary>
        private Dictionary<string, string> TypeIDs = new Dictionary<string, string>();
        
        /// <summary>
        /// Initialises a new, empty scanner state.
        /// </summary>
        public ILScannerState(bool debugBuild)
        {
            DebugBuild = debugBuild;

            InitStringLiteralsDataBlock();
            InitStaticFieldsDataBlock();
            InitTypesTablesDataBlock();
        }

        private int currentMethodIDValue = 1;
        /// <summary>
        /// Gets the specified method's ID. If the method has not been scanned already, a new ID is created.
        /// </summary>
        /// <param name="aMethod">The method to get the ID of.</param>
        /// <returns>The method's ID.</returns>
        public string GetMethodID(MethodBase aMethod)
        {
            string methodSignature = Utils.GetMethodSignature(aMethod);
            string result = null;
            if (MethodIDs.ContainsKey(methodSignature))
            {
                result = MethodIDs[methodSignature];
            }
            else
            {
                result = Utils.CreateMethodID(methodSignature);
                MethodIDs.Add(methodSignature, result);
            }
            return result;
        }
        /// <summary>
        /// Gets the specified method's ID Value. If the method has not been scanned already, a new ID Value is created.
        /// </summary>
        /// <param name="aMethod">The method to get the ID Value of.</param>
        /// <returns>The method's ID Value.</returns>
        public string GetMethodIDValue(MethodInfo aMethod)
        {
            string methodSignature = Utils.GetMethodSignature(aMethod);
            string result = null;
            if (MethodIDValues.ContainsKey(methodSignature))
            {
                result = MethodIDValues[methodSignature];
            }
            else
            {
                //Check if method is actually an override
                // - If so, use the same method ID value as parent method
                MethodInfo baseDefMethod = aMethod.GetBaseDefinition();
                if (baseDefMethod != aMethod)
                {
                    result = GetMethodIDValue(baseDefMethod);
                }
                else
                {
                    result = (currentMethodIDValue++).ToString();
                }
                MethodIDValues.Add(methodSignature, result);
            }
            return result;
        }
        /// <summary>
        /// Gets the specified field's ID. If the field has not been scanned already, a new ID is created.
        /// </summary>
        /// <param name="aField">The field to get the ID of.</param>
        /// <returns>The field's ID.</returns>
        public string GetStaticFieldID(FieldInfo aField)
        {
            string fieldSignature = Utils.GetFieldSignature(aField);
            string result = null;
            if (StaticFieldIDs.ContainsKey(fieldSignature))
            {
                result = StaticFieldIDs[fieldSignature];
            }
            else
            {
                result = fieldSignature;
                result = "staticfield_" + Utils.FilterIdentifierForInvalidChars(result);
                StaticFieldIDs.Add(fieldSignature, result);
            }
            return result;
        }
        /// <summary>
        /// Retruns whether the specified static field has an ID yet or not.
        /// </summary>
        /// <param name="theField">The static field to get the ID of.</param>
        /// <returns>Whether the specified static field has an ID yet or not.</returns>
        public bool ContainsStaticFieldID(FieldInfo theField)
        {
            string fieldSignature = Utils.GetFieldSignature(theField);
            fieldSignature = "staticfield_" + Utils.FilterIdentifierForInvalidChars(fieldSignature);
            return StaticFieldIDs.ContainsKey(fieldSignature);
        }
        /// <summary>
        /// Gets the specified type's ID. If the type has not been scanned already, a new ID is created.
        /// </summary>
        /// <param name="theType">The type to get the ID of.</param>
        /// <returns>The type's ID.</returns>
        public string GetTypeID(Type theType)
        {
            string assemblyQualifiedName = theType.AssemblyQualifiedName;
            string result = Utils.GetMD5Hash(
                Encoding.UTF8.GetBytes(theType.AssemblyQualifiedName));
            if(TypeIDs.ContainsKey(assemblyQualifiedName))
            {
                result = TypeIDs[assemblyQualifiedName];
            }
            else
            {
                TypeIDs.Add(assemblyQualifiedName, result);
            }
            return result;
        }
        /// <summary>
        /// Retruns whether the specified type has an ID yet or not.
        /// </summary>
        /// <param name="theType">The type to get the ID of.</param>
        /// <returns>Whether the specified type has an ID yet or not.</returns>
        public bool ContainsTypeID(Type theType)
        {
            return TypeIDs.ContainsKey(theType.AssemblyQualifiedName);
        }
        /// <summary>
        /// Get's the Id string (assembler label) of the specified type ID.
        /// </summary>
        /// <param name="typeId">The type ID to convert to a label.</param>
        /// <returns>The ID string.</returns>
        public string GetTypeIdString(string typeId)
        {
            return Utils.FilterIdentifierForInvalidChars("type_" + typeId);
        }

        /// <summary>
        /// Adds a new string literal of specified value to the string literals data block.
        /// </summary>
        /// <param name="value">The value of the string to add.</param>
        /// <param name="ilOpInfo">The ILOpInfo that is adding the string literal.</param>
        /// <returns>The ID (label) of the string.</returns>
        public string AddStringLiteral(string value, ILOpInfo ilOpInfo)
        {
            string stringID = Utils.GetMD5Hash(
                Encoding.UTF8.GetBytes(value));
            string label = Utils.FilterIdentifierForInvalidChars("StringLiteral_" + stringID);
            
            if (!StringLiteralsDataBlock.ASM.ToString().Contains(stringID))
            {
                Encoding xEncoding = Encoding.ASCII;

                var NumBytes = xEncoding.GetByteCount(value);
                var stringData = new byte[4 + NumBytes];
                Array.Copy(BitConverter.GetBytes(value.Length), 0, stringData, 0, 4);
                Array.Copy(xEncoding.GetBytes(value), 0, stringData, 4, NumBytes);

                //This is UTF-16 (Unicode)/ASCII text
                StringLiteralsDataBlock.ASM.AppendLine(string.Format("{0}:", label));
                //Put in type info as FOS_System.String type
                StringLiteralsDataBlock.ASM.AppendLine("dd STRING_TYPE_ID");
                //Put in string length bytes
                StringLiteralsDataBlock.ASM.Append("db ");
                for (int i = 0; i < 3; i++)
                {
                    StringLiteralsDataBlock.ASM.Append(stringData[i]);
                    StringLiteralsDataBlock.ASM.Append(", ");
                }
                StringLiteralsDataBlock.ASM.Append(stringData[3]);
                //Put in string characters (as words)
                StringLiteralsDataBlock.ASM.Append("\ndw ");
                for (int i = 4; i < (stringData.Length - 1); i++)
                {
                    StringLiteralsDataBlock.ASM.Append(stringData[i]);
                    StringLiteralsDataBlock.ASM.Append(", ");
                }
                StringLiteralsDataBlock.ASM.Append(stringData.Last());
                StringLiteralsDataBlock.ASM.AppendLine();

                if (DebugBuild)
                {
                    DB_StringLiteral dbStringLiteral = new DB_StringLiteral();
                    dbStringLiteral.Id = stringID;
                    dbStringLiteral.ILOpInfoID = ilOpInfo.DBILOpInfo.Id;
                    dbStringLiteral.Value = value;
                    DebugDatabase.AddStringLiteral(dbStringLiteral);
                }
            }

            return label;
        }
        /// <summary>
        /// Adds the specified static field to the static fields data block.
        /// </summary>
        /// <param name="aField">The field info to add.</param>
        /// <returns>The ID (label) of the static field.</returns>
        public string AddStaticField(FieldInfo aField)
        {
            //Don't add twice...
            string FieldID = null;
            if (!ContainsStaticFieldID(aField))
            {
                FieldID = GetStaticFieldID(aField);
                Type fieldType = aField.FieldType;
                int fieldSize = Utils.GetNumBytesForType(fieldType);
                StaticFieldsDataBlock.ASM.AppendLine(string.Format("{0}: times {1} db 0", FieldID, fieldSize));
            }
            else
            {
                FieldID = GetStaticFieldID(aField);
            }

            return FieldID;
        }
        
        /// <summary>
        /// A count of the total number of entries added to the Types Table.
        /// Used for allocating FOS_System.Type.Id numbers.
        /// </summary>
        private int TypesTable_TotalNumEntries = 0;
        /// <summary>
        /// Adds the specified type to the Types Table.
        /// </summary>
        /// <param name="TheDBType">The type to add.</param>
        public void AddType(DB_Type TheDBType)
        {
            //The structure of an entry in the Types Table must be the same as
            //the FOS_System.Type class

            string TypeId = GetTypeIdString(TheDBType.Id);
            string SizeVal = TheDBType.BytesSize.ToString();
            string IdVal = (TypesTable_TotalNumEntries++).ToString();
            string StackSizeVal = TheDBType.StackBytesSize.ToString();
            string IsValueTypeVal = (TheDBType.IsValueType ? "1" : "0");
            string MethodTablePointer = TypeId + "_MethodTable";
            
            TypesTableDataBlock.ASM.AppendLine(string.Format("{0}: dd {1}, {2}, {3}, {4}, {6} \t\t; {5}", 
                TypeId, SizeVal, IdVal, StackSizeVal, IsValueTypeVal, TheDBType.Signature, MethodTablePointer));
        }

        private long currentMethodTablePriorityOffset = (long.MaxValue / 2) + 1;
        /// <summary>
        /// Adds the methods of the specified type to the type table.
        /// </summary>
        /// <param name="TheType">The type to process.</param>
        public void AddTypeMethods(Type TheType)
        {
            string currentTypeId = GetTypeIdString(GetTypeID(TheType));
            ASMChunk methodTable = new ASMChunk();
            methodTable.SequencePriority = currentMethodTablePriorityOffset++;
            
            methodTable.ASM.AppendLine("; Method Table - " + TheType.FullName);
            methodTable.ASM.AppendLine(currentTypeId + "_MethodTable:");

            if (TheType.BaseType == null || TheType.BaseType.FullName != "System.Array")
            {
                MethodInfo[] OwnMethods = GetInstanceMethods(TheType);
                foreach (MethodInfo anOwnMethod in OwnMethods)
                {
                    if (!anOwnMethod.IsAbstract)
                    {
                        string methodID = GetMethodID(anOwnMethod);
                        string methodIDValue = GetMethodIDValue(anOwnMethod);
                        methodTable.ASM.AppendLine("dd " + methodIDValue + ", " + methodID);
                    }
                }
            }

            string parentTypeMethodTablePtr = "0";
            if (TheType.BaseType != null)
            {
                if (!TheType.BaseType.AssemblyQualifiedName.Contains("mscorlib"))
                {
                    parentTypeMethodTablePtr = GetTypeIdString(GetTypeID(TheType.BaseType)) + "_MethodTable";
                }
            }
            methodTable.ASM.AppendLine("dd 0, " + parentTypeMethodTablePtr);

            methodTable.ASM.AppendLine("; Method Table End - " + TheType.FullName);

            MethodTablesDataBlock.Add(methodTable);
        }
        /// <summary>
        /// Returns all the instance (i.e. non-static) methods (including private methods) of the specified type excluding inherited methods.
        /// </summary>
        /// <param name="TheType">The type to get methods of.</param>
        /// <returns>All the instance methods.</returns>
        private MethodInfo[] GetInstanceMethods(Type TheType)
        {
            if (TheType.AssemblyQualifiedName.Contains("mscorlib"))
            {
                return new MethodInfo[0];
            }
            return TheType.GetMethods(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Initialises the string literals data block.
        /// </summary>
        private void InitStringLiteralsDataBlock()
        {
            StringLiteralsDataBlock.ASM.AppendLine("; BEGIN - String Literals");
            //Make them appear just before then end 
            // - not right at the end since the end file stuff has to go there
            //      (See ILScanner.Execute():endChunk)
            StringLiteralsDataBlock.SequencePriority = (long.MaxValue / 2) - 2;
        }
        /// <summary>
        /// Finalises the string literals data block (adds ending labels).
        /// </summary>
        public void FinaliseStringLiteralsDataBlock()
        {
            StringLiteralsDataBlock.ASM.AppendLine("; END - String Literals");

            StringLiteralsDataBlock.ASM.Replace("STRING_TYPE_ID", GetTypeIdString(GetTypeID(StringClass)));
        }

        /// <summary>
        /// Initialises the static fields data block.
        /// </summary>
        private void InitStaticFieldsDataBlock()
        {
            StaticFieldsDataBlock.ASM.AppendLine("; BEGIN - Static Fields");
            //Make them appear just before then end 
            // - not right at the end since the end file stuff has to go there
            //      (See ILScanner.Execute():endChunk)
            StaticFieldsDataBlock.SequencePriority = (long.MaxValue / 2) - 1;
        }
        /// <summary>
        /// Finalises the static fields data block (adds ending labels).
        /// </summary>
        public void FinaliseStaticFieldsDataBlock()
        {
            StaticFieldsDataBlock.ASM.AppendLine("; END - Static Fields");
        }

        /// <summary>
        /// Initialises the types tables data block.
        /// </summary>
        private void InitTypesTablesDataBlock()
        {
            TypesTableDataBlock.ASM.AppendLine("; BEGIN - Types Table");
            //Make them appear just before then end 
            // - not right at the end since the end file stuff has to go there
            //      (See ILScanner.Execute():endChunk)
            TypesTableDataBlock.SequencePriority = (long.MaxValue / 2) - 2;
        }
        /// <summary>
        /// Finalises the types table data block (adds ending labels).
        /// </summary>
        public void FinaliseTypesTablesDataBlock()
        {
            TypesTableDataBlock.ASM.AppendLine("; END - Types Table");
        }
                
        /// <summary>
        /// Finalises the IL scanner state so the IL scanner is ready for use in an ASM sequencer.
        /// </summary>
        public void Finalise()
        {
            FinaliseStringLiteralsDataBlock();
            FinaliseStaticFieldsDataBlock();
            FinaliseTypesTablesDataBlock();
        }
    }
    /// <summary>
    /// Represents a stack frame.
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// The stack of items in the current stack frame.
        /// </summary>
        public Stack<StackItem> Stack = new Stack<StackItem>();
    }
    /// <summary>
    /// Represents an item on a stack.
    /// </summary>
    public class StackItem
    {
        /// <summary>
        /// The size of the item (in bytes) on the stack.
        /// </summary>
        public int sizeOnStackInBytes;
        /// <summary>
        /// Whether the item represents a floating point number (single or double precision).
        /// </summary>
        public bool isFloat;

        /// <summary>
        /// Whether the item on the stack is actually a pointer to an object
        /// that has just been created by a NewObj IL op.
        /// </summary>
        /// <remarks>
        /// Used for correctly decrementing / incrementing GC ref count
        /// when a new object is created and either stored or the reference
        /// is popped off the stack.
        /// </remarks>
        public bool isNewGCObject = false;
    }
    /// <summary>
    /// Represents a local variable.
    /// </summary>
    public class LocalVariable : StackItem
    {
        /// <summary>
        /// The type of the local variable.
        /// </summary>
        public Type TheType;
    }
}
