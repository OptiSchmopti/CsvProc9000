using ArrangeContext.Moq;
using CsvProc9000.Csv;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Model.Configuration;
using CsvProc9000.Model.Csv;
using CsvProc9000.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CsvProc9000.Tests.Csv
{
    public class ApplyRulesToCsvFileTests
    {
        [Fact]
        public void Implements_Needed_Interface()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            sut
                .Should()
                .BeAssignableTo<IApplyRulesToCsvFile>();
        }

        [Fact]
        public void Apply_Fails_When_File_Is_Null()
        {
            var (context, _) = CreateContext();
            var sut = context.Build();

            Assert.ThrowsAny<ArgumentNullException>(() => sut.Apply(null!, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public void Apply_Does_Nothing_When_No_Rules_Defined()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();
            
            options.Rules = null;

            var file = new CsvFile("some file");
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            file
                .Rows
                .Should()
                .BeEmpty();

            options.Rules = new List<Rule>();
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            file
                .Rows
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Apply_Skips_Rule_When_Rule_Has_No_Conditions()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            options.Rules = new List<Rule> { new() { Conditions = null } };
            
            var file = new CsvFile("some file");
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            file
                .Rows
                .Should()
                .BeEmpty();
            
            options.Rules = new List<Rule> { new() { Conditions = new List<Condition>() } };
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            file
                .Rows
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Apply_Does_Nothing_When_Row_Does_Not_Meet_Condition()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetFieldValueCondition = "SomeValue";

            options.Rules = new List<Rule>
            {
                new() { Conditions = new List<Condition>{ new() { Field = targetField, Value = targetFieldValueCondition } } }
            };

            // none of this matches the condition above!
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column = new CsvColumn(0, "SomeOtherField");
            row.AddField(column, "something");
            file.AddRow(row);
            var field = row.Fields.FirstOrDefault();
            Assert.NotNull(field);
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            field
                .Value
                .Should()
                .NotBe(targetFieldValueCondition);
        }

        [Fact]
        public void Apply_Does_Nothing_When_Change_Has_No_Target_Field()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetFieldValueCondition = "SomeValue";
            const string targetFieldChangeValue = "this value changed";

            options.Rules = new List<Rule>
            {
                new()
                {
                    Conditions = new List<Condition> { new() { Field = targetField, Value = targetFieldValueCondition } },
                    Changes = new List<Change> { new() { Field = string.Empty, Value = targetFieldChangeValue } } }
            };
            
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column = new CsvColumn(0, targetField);
            row.AddField(column, targetFieldValueCondition);
            file.AddRow(row);
            var field = row.Fields.FirstOrDefault();
            Assert.NotNull(field);
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            field
                .Value
                .Should()
                .NotBe(targetFieldChangeValue);
        }

        [Fact]
        public void Apply_Does_Add_Field_With_Mode_Add()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetFieldValueCondition = "SomeValue";
            const string targetFieldChangeValue = "this value changed";

            options.Rules = new List<Rule>
            {
                new()
                {
                    Conditions = new List<Condition> { new() { Field = targetField, Value = targetFieldValueCondition } },
                    Changes = new List<Change> { new() { Field = targetField, Value = targetFieldChangeValue, Mode = ChangeMode.Add} } }
            };
            
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column = new CsvColumn(0, targetField);
            row.AddField(column, targetFieldValueCondition);
            file.AddRow(row);
            var field = row.Fields.FirstOrDefault();
            Assert.NotNull(field);
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            row
                .Fields
                .Should()
                .HaveCount(2);

            field
                .Value
                .Should()
                .NotBe(targetFieldChangeValue);

            var addedField = row.Fields.LastOrDefault();
            Assert.NotNull(addedField);

            addedField
                .Column
                .Name
                .Should()
                .Be(targetField);

            addedField
                .Value
                .Should()
                .Be(targetFieldChangeValue);
        }

        [Fact]
        public void Apply_Does_Update_Field_With_Mode_AddOrUpdate()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetFieldValueCondition = "SomeValue";
            const string targetFieldChangeValue = "this value changed";

            options.Rules = new List<Rule>
            {
                new()
                {
                    Conditions = new List<Condition> { new() { Field = targetField, Value = targetFieldValueCondition } },
                    Changes = new List<Change> { new() { Field = targetField, Value = targetFieldChangeValue, Mode = ChangeMode.AddOrUpdate} } }
            };
            
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column = new CsvColumn(0, targetField);
            row.AddField(column, targetFieldValueCondition);
            file.AddRow(row);

            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            row
                .Fields
                .Should()
                .HaveCount(1);

            var updatedField = row.Fields.FirstOrDefault();
            Assert.NotNull(updatedField);
            
            updatedField
                .Value
                .Should()
                .Be(targetFieldChangeValue);
        }
        
        [Fact]
        public void Apply_Does_Add_Field_With_Mode_AddOrUpdate()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetAddField = "SomeOtherField";
            const string targetFieldValueCondition = "SomeValue";
            const string targetFieldChangeValue = "this value changed";

            options.Rules = new List<Rule>
            {
                new()
                {
                    Conditions = new List<Condition> { new() { Field = targetField, Value = targetFieldValueCondition } },
                    Changes = new List<Change> { new() { Field = targetAddField, Value = targetFieldChangeValue, Mode = ChangeMode.AddOrUpdate} } }
            };
            
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column = new CsvColumn(0, targetField);
            row.AddField(column, targetFieldValueCondition);
            file.AddRow(row);
            var field = row.Fields.FirstOrDefault();
            Assert.NotNull(field);
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            row
                .Fields
                .Should()
                .HaveCount(2);

            field
                .Value
                .Should()
                .NotBe(targetFieldChangeValue);

            var addedField = row.Fields.LastOrDefault();
            Assert.NotNull(addedField);

            addedField
                .Column
                .Name
                .Should()
                .Be(targetAddField);

            addedField
                .Value
                .Should()
                .Be(targetFieldChangeValue);
        }
        
        [Fact]
        public void Apply_Fails_To_Update_When_Same_Field_With_Mode_AddOrUpdate()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetFieldValueCondition = "SomeValue";
            const string targetFieldChangeValue = "this value changed";

            options.Rules = new List<Rule>
            {
                new()
                {
                    Conditions = new List<Condition> { new() { Field = targetField, Value = targetFieldValueCondition } },
                    Changes = new List<Change> { new() { Field = targetField, Value = targetFieldChangeValue, Mode = ChangeMode.AddOrUpdate} } }
            };
            
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column1 = new CsvColumn(0, targetField);
            var column2 = new CsvColumn(1, targetField);
            row.AddField(column1, targetFieldValueCondition);
            row.AddField(column2, "some value");
            file.AddRow(row);
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            var firstField = row.Fields.FirstOrDefault(field => field.Column.Index == 0);
            var secondField = row.Fields.FirstOrDefault(field => field.Column.Index == 1);
            
            Assert.NotNull(firstField);
            Assert.NotNull(secondField);

            firstField
                .Value
                .Should()
                .Be(targetFieldValueCondition);

            secondField
                .Value
                .Should()
                .Be("some value");
        }
        
        [Fact]
        public void Apply_Updates_When_Same_Field_With_Mode_AddOrUpdate()
        {
            var (context, options) = CreateContext();
            var sut = context.Build();

            const string targetField = "SomeField";
            const string targetFieldValueCondition = "SomeValue";
            const string targetFieldChangeValue = "this value changed";

            options.Rules = new List<Rule>
            {
                new()
                {
                    Conditions = new List<Condition> { new() { Field = targetField, Value = targetFieldValueCondition } },
                    Changes = new List<Change> { new() { Field = targetField, Value = targetFieldChangeValue, Mode = ChangeMode.AddOrUpdate, FieldIndex = 1} } }
            };
            
            var file = new CsvFile("some file");
            var row = new CsvRow();
            var column1 = new CsvColumn(0, targetField);
            var column2 = new CsvColumn(1, targetField);
            row.AddField(column1, targetFieldValueCondition);
            row.AddField(column2, "some value");
            file.AddRow(row);
            
            sut.Apply(file, Guid.NewGuid(), Guid.NewGuid());

            var firstField = row.Fields.FirstOrDefault(field => field.Column.Index == 0);
            var secondField = row.Fields.FirstOrDefault(field => field.Column.Index == 1);
            
            Assert.NotNull(firstField);
            Assert.NotNull(secondField);

            firstField
                .Value
                .Should()
                .Be(targetFieldValueCondition);

            secondField
                .Value
                .Should()
                .Be(targetFieldChangeValue);
        }

        private static (ArrangeContext<ApplyRulesToCsvFile>, CsvProcessorOptions) 
            CreateContext()
        {
            var options = new CsvProcessorOptions();
            var optionsMock = new Mock<IOptions<CsvProcessorOptions>>();
            optionsMock
                .SetupGet(opt => opt.Value)
                .Returns(options);

            var context = new ArrangeContext<ApplyRulesToCsvFile>();
            context.Use(optionsMock);

            return (context, options);
        }
    }
}
