using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using Crispy;
using System;

namespace Test.Scanner
{
    [TestClass]
    public class TsGeneratorTest
    {
        [TestMethod]
        public void Int_is_number() => Check<int>("number");

        [TestMethod]
        public void Double_is_number() => Check<double>("number");

        [TestMethod]
        public void Unsigned_is_number() => Check<UInt64>("number");

        [TestMethod]
        public void Decimal_is_number() => Check<decimal>("number");

        [TestMethod]
        public void String_is_String() => Check<string>("string");

        [TestMethod]
        public void Char_is_also_string() => Check<char>("string");

        [TestMethod]
        public void Array_is_list() => Check<string[]>("string[]");

        [TestMethod]
        public void List_is_list() => Check<List<String>>("string[]");

        [TestMethod]
        public void Int_array_is_number_list() => Check<int[]>("number[]");

        [TestMethod]
        public void IEnumerable_is_list() => Check<IEnumerable<String>>("string[]");

        [TestMethod]
        public void Dictionary() => Check<Dictionary<string, int>>("{ [key:string]: number }");

        [TestMethod]
        public void Void() => Check(typeof(void), "void");

        public class ObjectProperties
        {
            private int _privateNotSerialized { get; set; }
            internal int InternalNotSerialized { get; set; }
            public static int StaticNotSerialzied { get; set; }
            public int PrivateGetterNotSerialized { private get; set; }
            public int Average { get; internal set; }
            public int High { get; set; }
            public int Low { get; set; }
        }

        [TestMethod]
        public void Class_with_int_properites() => Check<ObjectProperties>(
            expected: "{ average: number, high: number, low: number }", 
            options: new TsOptions{ Readable = true, Writeable = false });

        public class ReadWrite
        {
            public int MessageCount { get; private set; }
            public string MessageSubmit { private get; set; }
        }

        [TestMethod]
        public void ReadWrite_read_properties() => Check<ReadWrite>(
            expected: "{ messageCount: number }", 
            options: new TsOptions{ Readable = true, Writeable = false });

        [TestMethod]
        public void ReadWrite_write_properties() => Check<ReadWrite>(
            expected: "{ messageSubmit: string }", 
            options: new TsOptions{ Readable = false, Writeable = true });

        [TestMethod]
        public void ReadWrite_default_behaviour() => Check<ReadWrite>(
            expected: "{ messageCount: number, messageSubmit: string }");

        [TestMethod]
        public void ReadWrite_lowercasing_first_letter_disabled() => Check<ReadWrite>(
            expected: "{ MessageCount: number, MessageSubmit: string }", 
            options: new TsOptions{ LowercaseFirstLetter = false });

        public class ObjectMembers
        {
            public int Computed => 9;
            public int Property { get; set; }
            public int Average;
            internal int Internal;
            private int _private;
        }

        [TestMethod]
        public void ObjectMembers_handled_correcly() => Check<ObjectMembers>(
            expected: "{ computed: number, property: number, average: number }");

        [TestMethod]
        public void ObjectMembers_exclude_properties() => Check<ObjectMembers>(
            expected: "{ average: number }",
            new TsOptions{ Properties = false });

        [TestMethod]
        public void ObjectMembers_explucde_fields() => Check<ObjectMembers>(
            expected: "{ computed: number, property: number }",
            new TsOptions{ Fields = false });

        [TestMethod]
        public void ObjectMembers_non_public() => Check<ObjectMembers>(
            expected: "{ internal: number, _private: number }",
            new TsOptions{ NonPublic = true, Public = false });

        public class SubObject
        {
            public int Value;
        }

        public class CompositeObject
        {
            public int Root;
            public SubObject Child;
        }

        [TestMethod]
        public void Object_hierarchy() => Check<CompositeObject>("{ root: number, child: { value: number } }");

        public enum State
        {
            Open = 0,
            Closed = 1
        }

        public class ObjectWithEnum
        {
            public int Value;
            public State State;
        }

        
        [TestMethod]
        public void Enum_member_default() => Check<ObjectWithEnum>(
            expected: "{ value: number, state: 'Open' | 'Closed' }",
            options: new TsOptions{ EnumNumberValues = false });

        [TestMethod]
        public void Enum_member_numbers() => Check<ObjectWithEnum>(
            expected: "{ value: number, state: 0 | 1 }", 
            options: new TsOptions{ EnumNumberValues = true });

        private void Check<T>(string expected, TsOptions options = null){
            Check(typeof(T), expected, options);
        }

        private void Check(Type type, string expected, TsOptions options = null){
            TsGenerator.GenerateTypescriptType(type, options).Should().Be(expected);
        }
    }
}
