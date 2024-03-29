﻿using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Configuration
{
    [ExcludeFromCodeCoverage] // DTO
    public class Change
    {
        [UsedImplicitly]
        public string Field { get; set; }

        /// <summary>
        ///     When the Field-Name is not unique
        /// </summary>
        [UsedImplicitly]
        public int? FieldIndex { get; set; }

        [UsedImplicitly]
        public ChangeMode Mode { get; set; } = ChangeMode.AddOrUpdate;

        [UsedImplicitly]
        public string Value { get; set; }
    }
}
